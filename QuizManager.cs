using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CyberSecurityBotGUI.Data;
using CyberSecurityBotGUI.Logic;

namespace CyberSecurityBotGUI.QuizLogic
{
    public class QuizManager
    {
        private readonly List<QuizQuestion> _questions;
        private int _currentQuestionIndex;
        private int _score;
        private bool _isQuizActive;
        public int Score => _score;
        public int TotalQuestions => _questions.Count;

        public QuizManager()
        {
            _questions = Questions.GetAllQuestions();
            _currentQuestionIndex = 0;
            _score = 0;
            _isQuizActive = false;
        }

        public string StartQuiz()
        {
            _currentQuestionIndex = 0;
            _score = 0;
            _isQuizActive = true;
            return $"Starting Cybersecurity Quiz! You will be asked {_questions.Count} questions.\n\n" + GetCurrentQuestionText();
        }

        public bool IsQuizActive => _isQuizActive;

        public string GetCurrentQuestionText()
        {
            if (_currentQuestionIndex >= _questions.Count)
            {
                _isQuizActive = false;
                return GetFinalScore();
            }

            var q = _questions[_currentQuestionIndex];
            var sb = new StringBuilder();
            sb.AppendLine($"Question {_currentQuestionIndex + 1}: {q.QuestionText}");

            if (!q.IsTrueFalse)
            {
                foreach (var option in q.Options)
                {
                    sb.AppendLine(option);
                }
            }
            else
            {
                sb.AppendLine("Please answer 'True' or 'False'.");
            }

            return sb.ToString();
        }

        public string SubmitAnswer(string userAnswer)
        {
            if (_currentQuestionIndex >= _questions.Count)
            {
                return "📝 The quiz is already over.";
            }

            var question = _questions[_currentQuestionIndex];
            string normalizedUserAnswer = userAnswer.Trim().ToLowerInvariant();
            string normalizedCorrectAnswer = question.CorrectAnswer.Trim().ToLowerInvariant();

            string feedback;
            string cleanExplanation = Regex.Replace(question.Explanation, @"^(Correct|Exactly|Good job|Right)[!:\s-]*", "", RegexOptions.IgnoreCase).Trim();

            if (normalizedUserAnswer == normalizedCorrectAnswer)
            {
                _score++;
                feedback = $"✅ Correct! {question.Explanation}";
            }
            else
            {
                feedback = $"❌ Incorrect. The correct answer was '{question.CorrectAnswer}'.\n💡 Tip: {cleanExplanation}";
            }

            _currentQuestionIndex++;

            if (_currentQuestionIndex < _questions.Count)
            {
                return feedback + "\n\n" + GetCurrentQuestionText();
            }
            else
            {
                return feedback + "\n\n" + GetFinalScore();
            }
        }



        private string GetFinalScore()
        {
            _isQuizActive = false; // Reset quiz state

            string feedback;

            if (_score == _questions.Count)
            {
                feedback = "🎉 Perfect score! You're a cybersecurity pro!";
            }
            else if (_score >= _questions.Count * 0.7)
            {
                feedback = "👍 Great job! You have a solid understanding of cybersecurity.";
            }
            else
            {
                feedback = "📘 Keep learning to stay safe online. Practice makes perfect!";
            }

            return $"🏁 Quiz Complete!\nYou scored {_score} out of {_questions.Count}.\n\n{feedback}\n\n" +
                   "Would you like to do something else?\n" + CyberData.MenuText;
        }


    }
}
