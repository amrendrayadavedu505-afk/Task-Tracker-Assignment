using System;
using System.Collections.Generic;
using Task_Tracker.Domain;

namespace Task_Tracker.Application
{

    public static class SimpleSearch
    {
    
        public static List<TaskItem> ByTitle(List<TaskItem> items, string text)
        {
            var result = new List<TaskItem>();
            if (string.IsNullOrWhiteSpace(text)) return result;

            text = text.Trim();
            for (int i = 0; i < items.Count; i++)
            {
                var t = items[i];
                if (!string.IsNullOrEmpty(t.Title))
                {
                    if (t.Title.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                        result.Add(t);
                }
            }
            return result;
        }

        public static List<TaskItem> ByIdPrefix(List<TaskItem> items, string idPart)
        {
            var result = new List<TaskItem>();
            if (string.IsNullOrWhiteSpace(idPart)) return result;

            idPart = idPart.Trim();
            for (int i = 0; i < items.Count; i++)
            {
                var t = items[i];
                if (!string.IsNullOrEmpty(t.Id))
                {
                    if (t.Id.Equals(idPart, StringComparison.OrdinalIgnoreCase) ||
                        t.Id.StartsWith(idPart, StringComparison.OrdinalIgnoreCase))
                        result.Add(t);
                }
            }
            return result;
        }

        public static List<TaskItem> ByAssignee(List<TaskItem> items, string who)
        {
            var result = new List<TaskItem>();
            if (string.IsNullOrWhiteSpace(who)) return result;

            who = who.Trim();
            for (int i = 0; i < items.Count; i++)
            {
                var t = items[i];
                if (!string.IsNullOrEmpty(t.Assignee))
                {
                    if (t.Assignee.IndexOf(who, StringComparison.OrdinalIgnoreCase) >= 0)
                        result.Add(t);
                }
            }
            return result;
        }
        public static List<TaskItem> ByDueOn(List<TaskItem> items, DateTime date)
        {
            var result = new List<TaskItem>();
            for (int i = 0; i < items.Count; i++)
            {
                var t = items[i];
                if (t.DueDate.Date == date.Date) result.Add(t);
            }
            return result;
        }

        public static List<TaskItem> ByDueBefore(List<TaskItem> items, DateTime date)
        {
            var result = new List<TaskItem>();
            for (int i = 0; i < items.Count; i++)
            {
                var t = items[i];
                if (t.DueDate.Date < date.Date) result.Add(t);
            }
            return result;
        }

        public static List<TaskItem> ByDueAfter(List<TaskItem> items, DateTime date)
        {
            var result = new List<TaskItem>();
            for (int i = 0; i < items.Count; i++)
            {
                var t = items[i];
                if (t.DueDate.Date > date.Date) result.Add(t);
            }
            return result;
        }
        public static List<TaskItem> ByPriority(List<TaskItem> items, Priority priority)
        {
            var result = new List<TaskItem>();
            for (int i = 0; i < items.Count; i++)
            {
                var t = items[i];
                if (t.Priority == priority) result.Add(t);
            }
            return result;
        }
    }
}
