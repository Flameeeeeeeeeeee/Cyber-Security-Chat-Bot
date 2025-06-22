using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CyberSecurityBotGUI;
using CyberSecurityBotGUI.TaskLogic;

namespace CyberSecurityBotGUI.Logic
{
    public class ChatBotLogic
    {
        private readonly Random _random = new Random();

        private string _userName;
        private string _currentTopicKey;

        private readonly Dictionary<string, int> _topicRequestCounts =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private readonly TaskManager _taskManager = new TaskManager();

        public bool HasUserName => !string.IsNullOrWhiteSpace(_userName);

        public string StartConversation()
        {
            return
                "👋 Hello! Welcome to CyberSecurityBot.\n" +
                "Before we get started, what's your name?";
        }

        public string ProcessUserInput(string input)
        {
            input = input?.Trim() ?? "";

            if (!HasUserName)
            {
                _userName = SanitizeName(input);
                return $"Nice to meet you {_userName}! How can I help you stay safe online today?\n" +
                        CyberData.MenuText;
            }

            // --- Task Assistant Commands ---
            //add task
             if (input.StartsWith("add task", StringComparison.OrdinalIgnoreCase))
            {
                string taskText = input.Substring("add task".Length).Trim();

                if (string.IsNullOrWhiteSpace(taskText))
                    return "Please provide a task title after 'add task'.";

                // You can optionally split the title and description more smartly here
                string title = taskText;
                string description = $"Task: {taskText}";

                return _taskManager.AddTask(title, description, null);
            }

            //view tasks
            else if (input.Equals("view tasks", StringComparison.OrdinalIgnoreCase))
            {
                return _taskManager.ViewTasks();
            }
            //complete task
            else if (input.StartsWith("complete task", StringComparison.OrdinalIgnoreCase))
            {
                var indexStr = input.Substring("complete task".Length).Trim();
                int index;
                if (int.TryParse(indexStr, out index))
                    return _taskManager.CompleteTask(index - 1); // -1 if user counts from 1
                else
                    return "Please provide a valid task number to complete.";
            }
            //Delete Task
            else if (input.StartsWith("delete task", StringComparison.OrdinalIgnoreCase))
            {
                var indexStr = input.Substring("delete task".Length).Trim();
                int index;
                if (int.TryParse(indexStr, out index))
                    return _taskManager.DeleteTask(index - 1); // Assuming task list is 1-indexed for user
                else
                    return "Please provide a valid task number to delete.";
            }


            // --- Greetings ---
            string greetingResponse = TryGetGreetingResponse(input);
            if (greetingResponse != null)
            {
                return $"{_userName}, {greetingResponse}";
            }

            // --- Help/Menu ---
            if (IsHelpRequest(input))
            {
                return CyberData.MenuText;
            }

            // --- Sentiment Detection ---
            string sentimentResponse = TryGetSentimentResponse(input);
            if (sentimentResponse != null)
            {
                return $"{_userName}, {sentimentResponse}";
            }

            // --- Cybersecurity Topic Matching ---
            var topicMatch = TryMatchTopic(input);
            if (topicMatch != null)
            {
                _currentTopicKey = topicMatch.Value.Key;

                if (_topicRequestCounts.ContainsKey(_currentTopicKey))
                    _topicRequestCounts[_currentTopicKey]++;
                else
                    _topicRequestCounts[_currentTopicKey] = 1;

                if (_topicRequestCounts[_currentTopicKey] > 1 &&
                    CyberData.PersistentInterestResponses.ContainsKey(_currentTopicKey))
                {
                    return $"{GetRandomResponse(CyberData.PersistentInterestResponses[_currentTopicKey])}\n" +
                           GetRandomResponse(CyberData.RegexResponses[_currentTopicKey].Responses);
                }

                return $"{GetRandomResponse(topicMatch.Value.Value.Responses)}";
            }

            // --- Fallback to Last Topic ---
            if (!string.IsNullOrEmpty(_currentTopicKey))
            {
                return $"{_userName}, {GetRandomResponse(CyberData.RegexResponses[_currentTopicKey].Responses)}";
            }

            // --- Default Fallback ---
            return $"Sorry {_userName}, I’m not sure I understand. Could you try rephrasing or ask about topics like passwords, phishing, or privacy?";
        }

        // Utility Methods
        private string SanitizeName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Friend";

            input = input.Trim();
            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
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
                {
                    var responses = CyberData.BotStatusResponses[key];
                    return GetRandomResponse(responses);
                }
            }
            return null;
        }

        private string TryGetSentimentResponse(string input)
        {
            string lower = input.ToLowerInvariant();
            foreach (var sentimentKey in CyberData.SentimentKeywords.Keys)
            {
                if (lower.Contains(sentimentKey))
                {
                    return CyberData.SentimentKeywords[sentimentKey];
                }
            }
            return null;
        }

        private KeyValuePair<string, (string Pattern, string[] Responses)>? TryMatchTopic(string input)
        {
            foreach (var kvp in CyberData.RegexResponses)
            {
                var regex = new Regex(kvp.Value.Pattern, RegexOptions.IgnoreCase);
                if (regex.IsMatch(input))
                {
                    return kvp;
                }
            }
            return null;
        }

        private string GetRandomResponse(string[] responses)
        {
            if (responses == null || responses.Length == 0) return "";
            int idx = _random.Next(responses.Length);
            return responses[idx];
        }
    }
}
