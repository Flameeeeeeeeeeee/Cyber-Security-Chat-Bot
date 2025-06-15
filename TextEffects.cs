using System;
using System.Threading.Tasks;

namespace CyberSecurityBotGUI.TextEffects
{
    public static class TextEffects
    {
        // Typing effect returns the full string letter-by-letter via callback
        public static async Task TypeEffectAsync(string message, Action<char> onCharTyped, int delay = 20)
        {
            if (onCharTyped == null) throw new ArgumentNullException(nameof(onCharTyped));

            foreach (char c in message)
            {
                onCharTyped(c);
                await Task.Delay(delay);
            }
        }

        // Returns a string of repeated '-' characters for dividers
        public static string GetDivider(int width = 55)
        {
            return new string('-', width);
        }
    }
}
