using System;
using System.Collections.Generic;
using Task_Tracker.Domain;

namespace Task_Tracker.Application
{
    // two basic sorts:
    // 1) insertion sort by due date (earliest first)
    // 2) selection sort by priority (Critical -> High -> Medium -> Low)
    public static class SimpleSort
    {
        public static List<TaskItem> ByDueDateAscending(List<TaskItem> items)
        {
            var a = new List<TaskItem>(items);

            for (int i = 1; i < a.Count; i++)
            {
                var key = a[i];
                int j = i - 1;

                while (j >= 0 && a[j].DueDate > key.DueDate)
                {
                    a[j + 1] = a[j];
                    j = j - 1;
                }

                a[j + 1] = key;
            }

            return a;
        }

        public static List<TaskItem> ByPriorityDesc(List<TaskItem> items)
        {
            var a = new List<TaskItem>(items);

            for (int i = 0; i < a.Count; i++)
            {
                int best = i;

                for (int j = i + 1; j < a.Count; j++)
                {
                    if (PriorityValue(a[j].Priority) > PriorityValue(a[best].Priority))
                    {
                        best = j;
                    }
                }

                // swap
                var temp = a[i];
                a[i] = a[best];
                a[best] = temp;
            }

            return a;
        }

        private static int PriorityValue(Priority p)
        {
            // higher number means higher priority
            if (p == Priority.Critical) return 4;
            if (p == Priority.High)     return 3;
            if (p == Priority.Medium)   return 2;
            return 1; // Low
        }
    }
}
