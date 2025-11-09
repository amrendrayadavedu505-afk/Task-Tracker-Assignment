using System;

namespace Task_Tracker.Domain
{
    public enum Status   { Todo, InProgress, Done }
    public enum Priority { Low, Medium, High, Critical }

    public class TaskItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Assignee { get; set; } = "";

        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
        public DateTime DueDate   { get; set; } = DateTime.UtcNow.Date.AddDays(7);

        public Priority Priority { get; set; } = Priority.Medium;
        public Status Status { get; set; } = Status.Todo;
    }
}
