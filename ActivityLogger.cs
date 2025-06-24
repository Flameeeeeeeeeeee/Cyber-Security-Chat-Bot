using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CyberSecurityBotGUI.Logic
{
    public class ActivityLogger
    {
        private readonly List<string> _log = new List<string>();
        private const int MaxEntries = 10;

        
        // Adds a new entry to the activity log with a timestamp.
        // Keeps only the latest 10 entries for clarity.
       
        public void Log(string message)
        {
            string timestamped = $"{DateTime.Now:HH:mm:ss} - {message}";
            if (_log.Count >= MaxEntries)
                _log.RemoveAt(0);
            _log.Add(timestamped);
        }


        // Returns the recent activity log as a string.

        public string GetRecentLog()
        {
            if (_log.Count == 0)
                return "📝 No recent activities logged yet.";

            return "📜 Here's a summary of recent actions:\n" + string.Join("\n", _log);
        }

        
        // Clears all log entries. Useful for resets or testing.
        
        public void Clear()
        {
            _log.Clear();
        }
    }
}

