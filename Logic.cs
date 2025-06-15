using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CyberSecurityBotGUI;


namespace CyberSecurityBotGUI.Logic
{
    public class ChatBotLogic
    {
        private readonly Random _random = new Random();

        // User state
        private string _userName;
        private string _currentTopicKey;

        // Track how many times each topic was asked
        private readonly Dictionary<string, int> _topicRequestCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public bool HasUserName => !string.IsNullOrWhiteSpace(_userName);

        /// <summary>
        /// Call this to start the conversation, returns a welcome message with ASCII art.
        /// </summary>
        public string StartConversation()
        {
            return
                "👋 Hello! Welcome to CyberSecurityBot.\n" +
                "Before we get started, what's your name?";
        }

        /// <summary>
        /// Main entry for processing user input and returning chatbot responses.
        /// </summary>
        public string ProcessUserInput(string input)
        {
            input = input?.Trim() ?? "";

            // If we don't have user name yet, treat first input as the name
            if (!HasUserName)
            {
                _userName = SanitizeName(input);
                return $"Nice to meet you, {_userName}! How can I help you stay safe online today?\n" +
                        CyberData.MenuText;
            }

            // Check for greeting / status questions first
            string greetingResponse = TryGetGreetingResponse(input);
            if (greetingResponse != null)
            {
                return $"{_userName}, {greetingResponse}";
            }

            // Check if user is asking for menu/help
            if (IsHelpRequest(input))
            {
                return CyberData.MenuText;
            }

            // Detect sentiment & respond if found
            string sentimentResponse = TryGetSentimentResponse(input);
            if (sentimentResponse != null)
            {
                return $"{_userName}, {sentimentResponse}";
            }

            // Detect cybersecurity topics by keywords (regex)
            var topicMatch = TryMatchTopic(input);
            if (topicMatch != null)
            {
                _currentTopicKey = topicMatch.Value.Key;

                // Update topic request count
                if (_topicRequestCounts.ContainsKey(_currentTopicKey))
                    _topicRequestCounts[_currentTopicKey]++;
                else
                    _topicRequestCounts[_currentTopicKey] = 1;

                // If user has asked about this topic repeatedly, use PersistentInterestResponses
                if (_topicRequestCounts[_currentTopicKey] > 1 &&
                    CyberData.PersistentInterestResponses.ContainsKey(_currentTopicKey))
                {
                    return $"{_userName}, {GetRandomResponse(CyberData.PersistentInterestResponses[_currentTopicKey])}\n" +
                           GetRandomResponse(CyberData.RegexResponses[_currentTopicKey].Responses);
                }

                return $"{_userName}, {GetRandomResponse(topicMatch.Value.Value.Responses)}";
            }

            // If user is asking follow-up question related to last topic, continue it
            if (!string.IsNullOrEmpty(_currentTopicKey))
            {
                return $"{_userName}, {GetRandomResponse(CyberData.RegexResponses[_currentTopicKey].Responses)}";
            }

            // If no known input matched, provide default fallback
            return $"Sorry {_userName}, I’m not sure I understand. Could you try rephrasing or ask about topics like passwords, phishing, or privacy?";
        }

        private string SanitizeName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Friend";
            // Simple: capitalize first letter only, trim whitespace
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
