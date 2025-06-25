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
        private string _favoriteTopicKey;
        private readonly Dictionary<string, int> _topicRequestCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly TaskManager _taskManager = new TaskManager();
        private TaskItem _lastAddedTask = null;
        private string _lastPrompt = null;
        private readonly QuizManager _quizManager = new QuizManager();
        private readonly ActivityLogger _activityLogger = new ActivityLogger();

        private readonly Dictionary<string, string> _menuOptions = new Dictionary<string, string>
        {
            { "1", "password" },
            { "2", "phishing" },
            { "3", "browsing" },
            { "4", "vpn" },
            { "5", "privacy" },
            { "6", "quiz" }
        };

        public bool HasUserName => !string.IsNullOrWhiteSpace(_userName);

        public string StartConversation()
        {
            return "👋 Hello! Welcome to CyberSecurityBot.\nWhat's your name?";
        }

        public string ProcessUserInput(string input)
        {
            input = input?.Trim() ?? "";
            string lower = input.ToLowerInvariant();

            if (!HasUserName)
            {
                _userName = SanitizeName(input);
                return $"Nice to meet you, {_userName}! How can I assist you with cybersecurity today?\n{CyberData.MenuText}";
            }
            if (IsThanks(input))
            {
                return CyberData.ThanksResponses[_random.Next(CyberData.ThanksResponses.Length)];
            }
            {
                if (InputValidator.InputValidator.IsQuitCommand(input))
                    return "exit"; // Special keyword to trigger shutdown 
            }

            // Favorite topic detection
            foreach (var key in CyberData.RegexResponses.Keys)
            {
                if ((lower.Contains("favorite") || lower.Contains("interested in")) &&
                    Regex.IsMatch(lower, CyberData.RegexResponses[key].Pattern, RegexOptions.IgnoreCase))
                {
                    _favoriteTopicKey = key;
                    return $"{_userName}, got it! I’ll remember that you’re especially interested in {key}. 😊";
                }
            }

            // Quiz handling
            if (input.Equals("start quiz", StringComparison.OrdinalIgnoreCase))
            {
                return _quizManager.StartQuiz();
            }

            if (_quizManager.IsQuizActive)
            {
                string response = _quizManager.SubmitAnswer(input);
                if (!_quizManager.IsQuizActive)
                {
                    _activityLogger.LogQuizResult(_quizManager.Score, _quizManager.TotalQuestions);
                }
                return response;
            }

            if (!_quizManager.IsQuizActive && lower.Contains("quiz"))
            {
                return "The quiz has ended. If you'd like to try it again, just say 'start quiz'.\n\n" + CyberData.MenuText;
            }

            // Task handling
            if (IsAddTaskRequest(input)) return HandleAddTask(input);
            if (_lastAddedTask != null && !_lastAddedTask.ReminderDate.HasValue)
            {
                if (_lastPrompt != "waiting-for-reminder-time")
                {
                    if (IsAffirmative(input))
                        return PromptForReminder();

                    if (IsNegativeResponse(input))
                    {
                        _lastPrompt = null;
                        _lastAddedTask = null;
                        return "No problem! Let me know if you need anything else.";
                    }
                }
                else if (_lastPrompt == "waiting-for-reminder-time")
                {
                    return TryParseReminder(input);
                }
            }

            string normalizedInput = input.Trim().ToLowerInvariant();

            if (normalizedInput == "view tasks" ||
                normalizedInput == "show tasks" ||
                normalizedInput == "see tasks" ||
                normalizedInput == "view task" ||
                normalizedInput == "see tasks" ||
                normalizedInput == "show me the tasks" ||
                  normalizedInput == "tasks" ||
                normalizedInput == "list tasks")
            {
                return _taskManager.ViewTasks();
            }

            if (IsCompleteTaskRequest(input, out int completeIndex))
                return HandleCompleteTask(completeIndex);

            if (IsDeleteTaskRequest(input, out int deleteIndex))
                return HandleDeleteTask(deleteIndex);


            // Activity log
            if (IsActivityLogRequest(input))
                return _activityLogger.GetRecentLog();

            // Greetings
            string greetingResponse = TryGetGreetingResponse(input);
            if (greetingResponse != null)
                return $"{_userName}, {greetingResponse}";

            // Help/Menu
            if (IsHelpRequest(input))
            {
                string menu = CyberData.MenuText;
                if (!string.IsNullOrEmpty(_favoriteTopicKey))
                    menu += $"\n⭐ I’ll keep in mind your favorite topic is {_favoriteTopicKey}.";
                return menu;
            }

            // Sentiment detection
            string sentimentResponse = TryGetSentimentResponse(input);

            // Topic matching
            var topicMatch = TryMatchTopic(input);
            if (topicMatch != null)
                return GenerateTopicResponse(topicMatch.Value, sentimentResponse);

            // Numeric menu options
            string numericInput = Regex.Replace(lower, @"[^\d]", "");
            if (_menuOptions.TryGetValue(numericInput, out string selectedTopicKey))
            {
                if (selectedTopicKey == "quiz")
                    return _quizManager.StartQuiz();

                _currentTopicKey = selectedTopicKey;
                return $"{_userName}, {GetRandomResponse(CyberData.RegexResponses[selectedTopicKey].Responses)}";
            }

            // Follow-up requests
            if (lower.Contains("tell me more") || lower.Contains("what else"))
            {
                string followUpKey = _currentTopicKey ?? _favoriteTopicKey;
                if (followUpKey != null && CyberData.RegexResponses.TryGetValue(followUpKey, out var responseData))
                {
                    return $"{_userName}, here's more on {followUpKey}:\n{GetRandomResponse(responseData.Responses)}";
                }
                return $"{_userName}, could you remind me which topic you’d like to continue with?";
            }

            // Continue from last topic
            if (!string.IsNullOrEmpty(_currentTopicKey))
                return $"{_userName}, {GetRandomResponse(CyberData.RegexResponses[_currentTopicKey].Responses)}";

            return $"Sorry {_userName}, I’m not sure I understand. Try asking about topics like passwords, phishing, or privacy.";
        }

        // --- Task Helpers ---
        private bool IsAddTaskRequest(string input) =>
            input.StartsWith("add task", StringComparison.OrdinalIgnoreCase);

        private string HandleAddTask(string input)
        {
            string taskText = input.Substring("add task".Length).Trim();
            if (string.IsNullOrWhiteSpace(taskText))
                return "Please provide a task title after 'add task'.";

            string title = taskText;
            string description = $"Review or perform task: {taskText}";

            string response = _taskManager.AddTask(title, description, null);
            _lastAddedTask = _taskManager.GetLastTask();
            _activityLogger.Log($"Task added: '{title}'");
            return $"{response} Would you like a reminder?";
        }
        //no reminder helper 
        private bool IsNegativeResponse(string input)
        {
            string lower = input.ToLowerInvariant();
            return lower.Contains("no") || lower.Contains("nah") || lower.Contains("not really") || lower.Contains("nope") || lower.Contains("not now");
        }

        private bool IsCompleteTaskRequest(string input, out int index)
        {
            index = 0;
            if (!input.StartsWith("complete task", StringComparison.OrdinalIgnoreCase))
                return false;
            var indexStr = input.Substring("complete task".Length).Trim();
            return int.TryParse(indexStr, out index);
        }

        private string HandleCompleteTask(int index)
        {
            _activityLogger.Log($"Task marked as completed: #{index}");
            return _taskManager.CompleteTask(index);
        }

        private bool IsDeleteTaskRequest(string input, out int index)
        {
            index = 0;
            if (!input.StartsWith("delete task", StringComparison.OrdinalIgnoreCase))
                return false;
            var indexStr = input.Substring("delete task".Length).Trim();
            return int.TryParse(indexStr, out index);
        }

        private string HandleDeleteTask(int index)
        {
            _activityLogger.Log($"Task deleted: #{index}");
            return _taskManager.DeleteTask(index);
        }

        // Reminder Helpers
        private string PromptForReminder()
        {
            _lastPrompt = "waiting-for-reminder-time";
            return "Great! Please tell me how long — e.g., 'Remind me in 3 days.'";
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
            _activityLogger.AppendToLast($"(Reminder set for {reminderDate:g})");
            _lastPrompt = null;
            return $"Got it! I'll remind you in {number} {unit}.";
        }

        // --- Other Helpers ---
        private string SanitizeName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Friend";
            input = input.Trim();
            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }

        private bool IsAffirmative(string input)
        {
            string lower = input.ToLowerInvariant();
            return CyberData.AffirmativeKeywords.Any(keyword => lower.Contains(keyword));
        }

        private bool IsActivityLogRequest(string input)
        {
            string lower = input.ToLowerInvariant();
            return CyberData.ActivityLogCommands.Any(phrase => lower.Contains(phrase));
        }

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

        private KeyValuePair<string, (string Pattern, string[] Responses)>? TryMatchTopic(string input)
        {
            foreach (var kvp in CyberData.RegexResponses)
            {
                if (Regex.IsMatch(input, kvp.Value.Pattern, RegexOptions.IgnoreCase))
                    return kvp;
            }
            return null;
        }

        private string GetRandomResponse(string[] responses)
        {
            if (responses == null || responses.Length == 0) return "";
            return responses[_random.Next(responses.Length)];
        }

        private string GenerateTopicResponse(KeyValuePair<string, (string Pattern, string[] Responses)> topicMatch, string sentiment = null)
        {
            _currentTopicKey = topicMatch.Key;

            int count = 0;
            if (_topicRequestCounts.TryGetValue(_currentTopicKey, out int existingCount))
                count = existingCount;

            _topicRequestCounts[_currentTopicKey] = count + 1;

            string baseResponse = GetRandomResponse(topicMatch.Value.Responses);
            string intro = "";

            if (_topicRequestCounts[_currentTopicKey] >= 3 &&
                CyberData.PersistentInterestResponses.ContainsKey(_currentTopicKey))
            {
                intro = GetRandomResponse(CyberData.PersistentInterestResponses[_currentTopicKey]) + "\n";
            }

            if (_favoriteTopicKey == _currentTopicKey)
            {
                intro += "And since this is your favorite topic, here's something extra valuable:\n";
            }

            return sentiment != null
                ? $"{_userName}, {sentiment}\n\n{intro}{baseResponse}"
                : $"{intro}{baseResponse}";
        }
        private bool IsThanks(string input)
        {
            string lower = input.ToLowerInvariant();
            return lower == "thanks" ||
                   lower == "thank you" ||
                   lower == "thank you very much" ||
                   lower == "thx" ||
                   lower == "ty" ||
                   lower == "thanks a lot" ||
                   lower.StartsWith("thanks ") ||  // e.g. "thanks for your help"
                   lower.StartsWith("thank you");
        }

    }
}


