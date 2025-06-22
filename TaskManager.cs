using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyberSecurityBotGUI.TaskLogic
{
    public class TaskItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }

        public override string ToString()
        {
            string status = IsCompleted ? "[✓ Completed]" : "[ ]";
            string reminderText = ReminderDate.HasValue ? $" (Remind: {ReminderDate.Value.ToShortDateString()})" : "";
            return $"{status} {Title}: {Description}{reminderText}";
        }
    }

    public class TaskManager
    {
        private readonly List<TaskItem> tasks = new List<TaskItem>();

        public string AddTask(string title, string description = null, DateTime? reminder = null)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                description = title;
            }

            tasks.Add(new TaskItem
            {
                Title = title,
                Description = description,
                ReminderDate = reminder,
                IsCompleted = false
            });

            if (reminder.HasValue)
                return $"Task '{title}' added with a reminder set for {reminder.Value.ToShortDateString()}.";
            else
                return $"Task '{title}' added without a reminder.";
        }


        public string ViewTasks()
        {
            if (tasks.Count == 0)
                return "You don't have any tasks at the moment.";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Here are your tasks:\n");

            int count = 1;
            foreach (var task in tasks)
            {
                sb.AppendLine($"{count}. {task}");
                count++;
            }

            return sb.ToString();
        }

        public string CompleteTask(int taskIndex)
        {
            if (taskIndex < 1 || taskIndex > tasks.Count)
                return "Invalid task number.";

            tasks[taskIndex - 1].IsCompleted = true;
            return $"Task '{tasks[taskIndex - 1].Title}' marked as completed.";
        }

        public string DeleteTask(int taskIndex)
        {
            if (taskIndex < 1 || taskIndex > tasks.Count)
                return "Invalid task number.";

            string title = tasks[taskIndex - 1].Title;
            tasks.RemoveAt(taskIndex - 1);
            return $"Task '{title}' has been deleted.";
        }
        public TaskItem GetLastTask()
        {
            return tasks.LastOrDefault();
        }

    }
}
