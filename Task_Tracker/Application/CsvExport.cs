using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Task_Tracker.Domain;

namespace Task_Tracker.Application
{
    public static class CsvExport
    {
        // Export a list of tasks to a CSV file in ./Reports/
        public static string OverdueToCsv(List<TaskItem> items)
        {
            var reportsDir = Path.Combine(AppContext.BaseDirectory, "Reports");
            Directory.CreateDirectory(reportsDir);

            var fileName = "overdue_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".csv";
            var fullPath = Path.Combine(reportsDir, fileName);

            var sb = new StringBuilder();

            // header
            sb.AppendLine("Id,Title,Assignee,Status,Priority,StartDate,DueDate");

            // rows
            for (int i = 0; i < items.Count; i++)
            {
                var t = items[i];
                var row = string.Join(",",
                    Csv(t.Id),
                    Csv(t.Title),
                    Csv(t.Assignee),
                    Csv(t.Status.ToString()),
                    Csv(t.Priority.ToString()),
                    Csv(t.StartDate.ToString("yyyy-MM-dd")),
                    Csv(t.DueDate.ToString("yyyy-MM-dd"))
                );
                sb.AppendLine(row);
            }

            File.WriteAllText(fullPath, sb.ToString(), Encoding.UTF8);
            return fullPath; 
        }
        private static string Csv(string? value)
        {
            value ??= "";
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
