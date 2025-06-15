using System;
using System.IO;
using System.Media;
using System.Windows;

namespace CyberSecurityBotGUI.StartUpServices
{
    public static class StartupService
    {
        private const string AudioFilePath = @"Resources\\ttsmaker-file-2025-4-19-1-24-16.wav";

        /// <summary>
        /// Plays a WAV file for a voice greeting.
        /// </summary>
        public static void PlayVoiceGreeting()
        {
            try
            {
                if (File.Exists(AudioFilePath))
                {
                    using (var player = new SoundPlayer(AudioFilePath))
                    {
                        player.PlaySync();
                    }
                }
                else
                {
                    MessageBox.Show("Audio file not found.", "Audio Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not play audio: " + ex.Message, "Playback Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Returns an ASCII banner string (to be displayed in the UI).
        /// </summary>
        public static string GetAsciiArt()
        {
            return
@"╔════════════════════════════════════════════════════╗
║   YOUR PASSWORD SUCKS, BUT THAT'S FINE             ║
║   I'm your Cybersecurity Wingman                   ║
╚════════════════════════════════════════════════════╝";
        }

        /// <summary>
        /// Returns a divider string (e.g., for TextBlocks or Logs).
        /// </summary>
        public static string GetDivider()
        {
            return new string('-', 55);
        }
    }
}
