using System.Collections.Generic;

namespace CyberSecurityBotGUI.QuizLogic
{
    public class QuizQuestion
    {
        public string QuestionText { get; set; }
        public List<string> Options { get; set; }  // For multiple-choice questions
        public string CorrectAnswer { get; set; }  // Should match one of the options or "True"/"False"
        public string Explanation { get; set; }
        public bool IsTrueFalse { get; set; }  // Flag if it's a True/False question

        public QuizQuestion(string questionText, List<string> options, string correctAnswer, string explanation, bool isTrueFalse = false)
        {
            QuestionText = questionText;
            Options = options;
            CorrectAnswer = correctAnswer;
            Explanation = explanation;
            IsTrueFalse = isTrueFalse;
        }
    }

    public static class Questions
    {
        public static List<QuizQuestion> GetAllQuestions()
        {
            return new List<QuizQuestion>
            {
                //1
                new QuizQuestion(
                    "What should you do if you receive an email asking for your password?",
                    new List<string> { "A) Reply with your password", "B) Delete the email", "C) Report the email as phishing", "D) Ignore it" },
                    "C",
                    "Correct! Reporting phishing emails helps prevent scams."
                ),
                //2
                new QuizQuestion(
                    "True or False: Using the same password for multiple sites is safe.",
                    new List<string> { "True", "False" },
                    "False",
                    "Correct! Using the same password across sites increases your security risk.",
                    isTrueFalse: true
                ),

                
                //3
                new QuizQuestion(
                    "What is phishing?",
                    new List<string> { "A) A type of social engineering attack", "B) A secure way to share passwords", "C) A method of encrypting data", "D) None of the above" },
                    "A",
                    "Right! Phishing is a social engineering technique to trick people into revealing sensitive info."
                ),
                //4
                new QuizQuestion(
                    "True or False: You should install software updates promptly.",
                    new List<string> { "True", "False" },
                    "True",
                    "Correct! Updates often patch security vulnerabilities."
                ),
                //5
               new QuizQuestion(
                    "What does two-factor authentication (2FA) provide?",
                    new List<string> { "A) Faster login", "B) An extra layer of security", "C) Password recovery", "D) Automatic updates" },
                    "B",
                    "Correct! 2FA adds an extra step to verify your identity, increasing security."
            ),

                new QuizQuestion(
                    "Which of the following is a strong password?",
                    new List<string> { "A) Password123", "B) qwerty", "C) 123456", "D) P@55w0rd!2025" },
                    "D",
                    "Right! Strong passwords include letters, numbers, and special characters."
                ),

                new QuizQuestion(
                    "What should you do before clicking on a link in an email?",
                    new List<string> { "A) Check the sender's email address", "B) Click immediately", "C) Forward to friends", "D) Reply asking for verification" },
                    "A",
                    "Correct! Always verify the sender to avoid phishing scams."
                ),

                new QuizQuestion(
                    "What is the purpose of a firewall?",
                    new List<string> { "A) To cool down your computer", "B) To prevent unauthorized access", "C) To speed up internet", "D) To store passwords" },
                    "B",
                    "Exactly! Firewalls help block unauthorized access to your network."
                ),

                new QuizQuestion(
                    "Which of these is NOT a sign of a phishing attempt?",
                    new List<string> { "A) Urgent requests for personal info", "B) Poor grammar and spelling", "C) Email from known contacts", "D) Suspicious links" },
                    "C",
                    "Right! Emails from known contacts are usually safe, but always stay cautious."
                ),

                new QuizQuestion(
                    "How often should you update your passwords?",
                    new List<string> { "A) Never", "B) Every 1-3 months", "C) Once a year", "D) Only when hacked" },
                    "B",
                    "Good job! Regularly updating passwords reduces the risk of unauthorized access."
                ),

            };
        }
    }
}
