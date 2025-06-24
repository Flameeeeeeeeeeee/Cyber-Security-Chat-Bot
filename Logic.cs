using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CyberSecurityBotGUI.StartUpServices;
using CyberSecurityBotGUI.TaskLogic;
using CyberSecurityBotGUI.QuizLogic;
using CyberSecurityBotGUI.Data;
namespace CyberSecurityBotGUI.Logic
{
    public class ChatBotLogic
    {
        private readonly Random _random = new Random();
        private string _userName;
        private string _currentTopicKey;
        private readonly Dictionary<string, int> _topicRequestCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly TaskManager _taskManager = new TaskManager();
        private TaskItem _lastAddedTask = null;
        private string _lastPrompt = null;
        private readonly QuizManager _quizManager = new QuizManager();
        private readonly ActivityLogger _activityLogger = new ActivityLogger();//activity logger
       

        public bool HasUserName => !string.IsNullOrWhiteSpace(_userName);

        public string StartConversation()
        {
            return "👋 Hello! Welcome to CyberSecurityBot.\nWhat's your name?";
        }
        //master input method
        public string ProcessUserInput(string input)
        {
            input = input?.Trim() ?? "";
            var topicMatch = TryMatchTopic(input);

            if (!HasUserName)
            {
                _userName = SanitizeName(input);
                return $"Nice to meet you, {_userName}! How can I assist you with cybersecurity today?\n{CyberData.MenuText}";

            }
            // Start the Quiz
            if (input.Equals("start quiz", StringComparison.OrdinalIgnoreCase))
            {
                return _quizManager.StartQuiz();
            }
            // ✅ Insert this new block:
            if (_quizManager.IsQuizActive)
            {
                return _quizManager.SubmitAnswer(input);
            }


            // If user is answering quiz questions
            if (!_quizManager.IsQuizActive && input.ToLower().Contains("quiz"))
            {
                return "The quiz has ended. If you'd like to try it again, just say 'start quiz'.\n\n" + CyberData.MenuText;
            }
            // Add Task
            if (input.StartsWith("add task", StringComparison.OrdinalIgnoreCase))
            {
                string taskText = input.Substring("add task".Length).Trim();
                if (string.IsNullOrWhiteSpace(taskText))
                    return "Please provide a task title after 'add task'.";

                string title = taskText;
                string description = $"Review or perform task: {taskText}";

                string response = _taskManager.AddTask(title, description, null);
                _lastAddedTask = _taskManager.GetLastTask(); // Save reference to allow follow-up
                _activityLogger.Log($"Task added: '{title}'");//logging details
                return $"{response} Would you like a reminder?";
            }

            // When user says yes after bot asks about reminder
            // 1. If user says "yes" and we just added a task without a reminder
            if (IsAffirmative(input)
                && _lastAddedTask != null && !_lastAddedTask.ReminderDate.HasValue
                && _lastPrompt != "waiting-for-reminder-time")
            {
                _lastPrompt = "waiting-for-reminder-time";
                return "Great! Please tell me how long — e.g., 'Remind me in 3 days.'";
            }


            // 2. If waiting for reminder time
            if (_lastPrompt == "waiting-for-reminder-time" && _lastAddedTask != null)
            {
                return TryParseReminder(input);
            
            }
            // View Tasks
            if (input.Equals("view tasks", StringComparison.OrdinalIgnoreCase))
                return _taskManager.ViewTasks();

            // Complete Task
            if (input.StartsWith("complete task", StringComparison.OrdinalIgnoreCase))
            {
                var indexStr = input.Substring("complete task".Length).Trim();
                if (int.TryParse(indexStr, out int index))
                {
                    _activityLogger.Log($"Task marked as completed: #{index}");
                    return _taskManager.CompleteTask(index);
                }
                return "Please provide a valid task number to complete.";
            }

            // Delete Task
            if (input.StartsWith("delete task", StringComparison.OrdinalIgnoreCase))
            {
                var indexStr = input.Substring("delete task".Length).Trim();
                if (int.TryParse(indexStr, out int index))
                {
                    _activityLogger.Log($"Task deleted: #{index}");
                    return _taskManager.DeleteTask(index);
                }
                return "Please provide a valid task number to delete.";
            }
            // Activity Log Command
            if (IsActivityLogRequest(input))
            {
                return _activityLogger.GetRecentLog();
            }


            // Greetings
            string greetingResponse = TryGetGreetingResponse(input);
            if (greetingResponse != null)
                return $"{_userName}, {greetingResponse}";

            // Help/Menu
            if (IsHelpRequest(input))
                return CyberData.MenuText;

            // Sentiment Detection
            // --- Sentiment + Topic Matching ---
            string sentimentResponse = TryGetSentimentResponse(input);


            // Topic Matching
            if (topicMatch != null)
                return GenerateTopicResponse(topicMatch.Value, sentimentResponse);
            // Handle numeric menu selections (1-6)
            switch (input)
            {
                case "1":
                    return $"{_userName}, {GetRandomResponse(CyberData.RegexResponses["password"].Responses)}";
                case "2":
                    return $"{_userName}, {GetRandomResponse(CyberData.RegexResponses["phishing"].Responses)}";
                case "3":
                    return $"{_userName}, {GetRandomResponse(CyberData.RegexResponses["browsing"].Responses)}";
                case "4":
                    return $"{_userName}, {GetRandomResponse(CyberData.RegexResponses["vpn"].Responses)}";
                case "5":
                    return $"{_userName}, {GetRandomResponse(CyberData.RegexResponses["privacy"].Responses)}";
                case "6":
                    return _quizManager.StartQuiz(); // If quiz manager exists
            }
            // Continue from last topic
            if (!string.IsNullOrEmpty(_currentTopicKey))
                return $"{_userName}, {GetRandomResponse(CyberData.RegexResponses[_currentTopicKey].Responses)}";

            return $"Sorry {_userName}, I’m not sure I understand. Try asking about topics like passwords, phishing, or privacy.";
        }
        //response helper
        private string GenerateTopicResponse(KeyValuePair<string, (string Pattern, string[] Responses)> topicMatch, string sentiment = null)
        {
            _currentTopicKey = topicMatch.Key;

            if (_topicRequestCounts.ContainsKey(_currentTopicKey))
                _topicRequestCounts[_currentTopicKey]++;
            else
                _topicRequestCounts[_currentTopicKey] = 1;

            string baseResponse = GetRandomResponse(topicMatch.Value.Responses);

            if (_topicRequestCounts[_currentTopicKey] > 1 &&
                CyberData.PersistentInterestResponses.ContainsKey(_currentTopicKey))
            {
                string persistence = GetRandomResponse(CyberData.PersistentInterestResponses[_currentTopicKey]);
                baseResponse = $"{persistence}\n{baseResponse}";
            }

            return sentiment != null
                ? $"{_userName}, {sentiment}\n\n{baseResponse}"
                : baseResponse;
        }

        private string TryParseReminder(string input)
        {
            var match = Regex.Match(input, @"remind me in (\d+)\s*(minute|minutes|hour|hours|day|days|week|weeks)", RegexOptions.IgnoreCase);
            if (!match.Success)
                return "Sorry, I couldn't understand that. Please say something like 'Remind me in 5 minutes' or 'Remind me in 3 days.'";

            int number = int.Parse(match.Groups[1].Value);
            string unit = match.Groups[2].Value.ToLower();

            DateTime reminderDate = DateTime.Now;

            switch (unit)
            {
                case "minute":
                case "minutes":
                    reminderDate = reminderDate.AddMinutes(number);
                    break;

                case "hour":
                case "hours":
                    reminderDate = reminderDate.AddHours(number);
                    break;

                case "day":
                case "days":
                    reminderDate = reminderDate.AddDays(number);
                    break;

                case "week":
                case "weeks":
                    reminderDate = reminderDate.AddDays(number * 7);
                    break;
            }

            _lastAddedTask.ReminderDate = reminderDate;
            _activityLogger.Log($"Reminder set for task '{_lastAddedTask.Title}' on {reminderDate:g}");
            _lastPrompt = null;
            return $"Got it! I'll remind you in {number} {unit}.";
        }


        // Utility methods
        private string SanitizeName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Friend";
            input = input.Trim();
            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }
        //affirmation 
        private bool IsAffirmative(string input)
        {
            string lower = input.ToLowerInvariant();
            return CyberData.AffirmativeKeywords.Any(keyword => lower.Contains(keyword));
        }
        //Activity log
        private bool IsActivityLogRequest(string input)
        {
            string lower = input.ToLowerInvariant();
            return CyberData.ActivityLogCommands.Any(phrase => lower.Contains(phrase));
        }
        //return menu on help prompt
        private bool IsHelpRequest(string input)
        {
            string lower = input.ToLowerInvariant();
            return lower.Contains("help") || lower.Contains("menu") || lower == "?";
        }

        private string TryGetGreetingResponse(string input)
        {
            string lower = input.ToLowerInvariant();
            foreach (var key in CyberData.BotStatusResponses.Keys)
            {
                if (lower.Contains(key))
                    return GetRandomResponse(CyberData.BotStatusResponses[key]);
            }
            return null;
        }
        //sentiment detection
        private string TryGetSentimentResponse(string input)
        {
            string lower = input.ToLowerInvariant();
            foreach (var sentimentKey in CyberData.SentimentKeywords.Keys)
            {
                if (lower.Contains(sentimentKey))
                    return CyberData.SentimentKeywords[sentimentKey];
            }
            return null;
        }
        //key word recognition
        private KeyValuePair<string, (string Pattern, string[] Responses)>? TryMatchTopic(string input)
        {
            foreach (var kvp in CyberData.RegexResponses)
            {
                if (Regex.IsMatch(input, kvp.Value.Pattern, RegexOptions.IgnoreCase))
                    return kvp;
            }
            return null;
        }
        //use random replies CyberData
        private string GetRandomResponse(string[] responses)
        {
            if (responses == null || responses.Length == 0) return "";
            return responses[_random.Next(responses.Length)];
        }
    }
}















