using System;
using System.Windows;
using CyberSecurityBotGUI; // Ensure your ChatBot.cs is under this namespace
using CyberSecurityBotGUI.Logic;
using CyberSecurityBotGUI.StartUpServices;


namespace CyberSecurityBotGUI
{
    public partial class MainWindow : Window
    {
        private ChatBotLogic chatBot;

        public MainWindow()
        {
            InitializeComponent();
            // Show ASCII art at the top of the chat
            AddBotMessage(StartupService.GetAsciiArt());

            // Play voice greeting
            StartupService.PlayVoiceGreeting();//FileName.ClassName

            // Initialize chatbot logic
            chatBot = new ChatBotLogic();

            // Optional: Add a welcome message from the bot
            AddBotMessage("Welcome! How can I assist you with cybersecurity today?");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string userInput = UserInputTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(userInput))
            {
                AddUserMessage(userInput); // Show user's input in chat box

                string botResponse = chatBot.ProcessUserInput(userInput);// Get response from ChatBot.cs
                AddBotMessage(botResponse); // Show bot response

                UserInputTextBox.Clear(); // Clear input after sending
            }
        }

        // Adds user's message to the ChatDisplayTextBox
        private void AddUserMessage(string message)
        {
            ChatDisplayTextBox.AppendText($"You: {message}\n");
            ChatDisplayTextBox.ScrollToEnd();
        }

        // Adds bot's response to the ChatDisplayTextBox
        private void AddBotMessage(string message)
        {
            ChatDisplayTextBox.AppendText($"Bot: {message}\n");
            ChatDisplayTextBox.ScrollToEnd();
        }
    }
}
