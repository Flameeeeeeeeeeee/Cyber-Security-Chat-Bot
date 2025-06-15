namespace CyberSecurityBotGUI.InputValidator
{
    public static class InputValidator
    {
        // Normalize input safely for all checks
        private static string Normalize(string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? string.Empty
                : input.Trim().ToLowerInvariant();
        }

        // Check if the input is a quit command
        public static bool IsQuitCommand(string input)
        {
            input = Normalize(input);
            return input == "exit" || input == "quit" || input == "bye";
        }

        // Check if the input is a help command
        public static bool IsHelpCommand(string input)
        {
            input = Normalize(input);
            return input == "help" || input == "menu" || input == "?";
        }

        // Check if the input is a valid menu number (1 to 5)
        public static bool TryGetMenuChoice(string input, out int choice)
        {
            input = Normalize(input);

            if (int.TryParse(input, out choice))
            {
                if (choice >= 1 && choice <= 5)
                    return true;
            }

            choice = -1;
            return false;
        }

        // Check for gibberish or meaningless input
        public static bool IsGibberish(string input)
        {
            input = Normalize(input);

            return input.Length < 3 ||
                   input == "asdf" ||
                   input == "123" ||
                   input == "..." ||
                   input.Contains("???");
        }
    }
}
