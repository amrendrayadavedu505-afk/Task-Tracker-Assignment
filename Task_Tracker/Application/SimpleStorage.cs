using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Task_Tracker.Domain;

namespace Task_Tracker.Application
{
    public static class SimpleStorage
    {
        private static readonly string DataDir  = Path.Combine(AppContext.BaseDirectory, "Data");
        private static readonly string DataPath = Path.Combine(DataDir, "tasks.json");
        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

        public static List<TaskItem> Load()
        {
            try
            {
                Directory.CreateDirectory(DataDir);
                if (!File.Exists(DataPath)) return new List<TaskItem>();
                var json = File.ReadAllText(DataPath);
                return JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
            }
            catch
            {
                return new List<TaskItem>();
            }
        }

        public static void Save(List<TaskItem> items)
        {
            Directory.CreateDirectory(DataDir);
            var tmp = DataPath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(items, JsonOpts));
            File.Move(tmp, DataPath, true);
        }
    }
}
