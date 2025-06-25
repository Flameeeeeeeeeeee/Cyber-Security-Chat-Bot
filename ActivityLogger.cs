using System;
using System.Collections.Generic;

namespace CyberSecurityBotGUI.Logic
{
    public class ActivityLogger
    {
        private readonly List<string> _log = new List<string>();
        private const int MaxEntries = 10;

        public void Log(string message)
        {
            if (_log.Count >= MaxEntries)
                _log.RemoveAt(0);
            _log.Add(message);
        }

        // Appends additional info to the last log entry
        public void AppendToLast(string extra)
        {
            if (_log.Count == 0) return;

            _log[_log.Count - 1] += $" {extra}";
        }

        public string GetRecentLog()
        {
            if (_log.Count == 0)
                return "📝 No recent activities logged yet.";

            var numberedLog = new List<string>();
            for (int i = 0; i < _log.Count; i++)
            {
                numberedLog.Add($"{i + 1}. {_log[i]}");
            }

            return "📜 Here's a summary of recent actions:\n" + string.Join("\n", numberedLog);
        }

        public void Clear()
        {
            _log.Clear();
        }

        public void LogQuizResult(int score, int totalQuestions)
        {
            string entry = $"Quiz completed: scored {score} out of {totalQuestions}";
            Log(entry);
        }
    }
}
