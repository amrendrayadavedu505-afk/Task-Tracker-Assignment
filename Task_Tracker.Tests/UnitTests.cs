using System;
using System.Collections.Generic;
using System.IO;
using Task_Tracker.Domain;
using Task_Tracker.Application;
using Xunit;

public class BasicTests
{
    private TaskItem Make(string title, DateTime start, DateTime due, Priority p, Status s = Status.Todo, string who = "nikhil")
    {
        return new TaskItem
        {
            Title = title,
            StartDate = start.Date,
            DueDate = due.Date,
            Priority = p,
            Status = s,
            Assignee = who
        };
    }

    public void Search_ByTitle_FindsSubstring()
    {
        var list = new List<TaskItem>
        {
            Make("Write Report", DateTime.UtcNow, DateTime.UtcNow.AddDays(3), Priority.Medium),
            Make("report summary", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), Priority.Low),
            Make("Nothing", DateTime.UtcNow, DateTime.UtcNow.AddDays(2), Priority.High),
        };

        var found = SimpleSearch.ByTitle(list, "REPORT");
        Assert.Equal(2, found.Count);
    }

    public void Search_Overdue_CorrectFilter()
    {
        var today = DateTime.UtcNow.Date;

        var list = new List<TaskItem>
        {
            Make("past not done", today.AddDays(-10), today.AddDays(-1), Priority.Low, Status.Todo), 
            Make("past done",    today.AddDays(-9),  today.AddDays(-2), Priority.Low, Status.Done),    
            Make("future",       today,              today.AddDays(2),  Priority.High, Status.Todo),     
        };

        var od = SimpleSearch.Overdue(list);
        Assert.Single(od);
        Assert.Equal("past not done", od[0].Title);
    }


    public void Sort_ByDueDateAscending_Works()
    {
        var today = DateTime.UtcNow.Date;
        var list = new List<TaskItem>
        {
            Make("b", today, today.AddDays(5), Priority.Medium),
            Make("a", today, today.AddDays(1), Priority.Medium),
            Make("c", today, today.AddDays(3), Priority.Medium),
        };

        var sorted = SimpleSort.ByDueDateAscending(list);
        Assert.Equal(new[] { "a", "c", "b" }, new[] { sorted[0].Title, sorted[1].Title, sorted[2].Title });
    }
    public void Sort_ByPriorityDesc_Works()
    {
        var list = new List<TaskItem>
        {
            Make("low",  DateTime.UtcNow, DateTime.UtcNow.AddDays(1), Priority.Low),
            Make("high", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), Priority.High),
            Make("crit", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), Priority.Critical),
            Make("med",  DateTime.UtcNow, DateTime.UtcNow.AddDays(1), Priority.Medium),
        };

        var sorted = SimpleSort.ByPriorityDesc(list);
        Assert.Equal(new[] { "crit", "high", "med", "low" },
                     new[] { sorted[0].Title, sorted[1].Title, sorted[2].Title, sorted[3].Title });
    }
    public void Csv_OverdueToCsv_CreatesFile()
    {
        // Arrange: make one overdue and one not overdue
        var today = DateTime.UtcNow.Date;
        var overdue = new List<TaskItem>
        {
            Make("od1", today.AddDays(-5), today.AddDays(-1), Priority.Medium),
        };

        var path = CsvExport.OverdueToCsv(overdue);
        
        Assert.True(File.Exists(path));

        var text = File.ReadAllText(path);
        Assert.Contains("Id,Title,Assignee,Status,Priority,StartDate,DueDate", text);
        Assert.Contains("od1", text);
    }
}
