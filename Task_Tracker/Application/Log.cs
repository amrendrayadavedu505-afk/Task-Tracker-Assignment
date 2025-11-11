using System;
using System.IO;

namespace Task_Tracker.Application
{
    public static class SimpleLog
    {
        private static readonly string LogDir  = Path.Combine(AppContext.BaseDirectory, "Logs");
        private static readonly string LogPath = Path.Combine(LogDir, "log.txt");

        private static void Write(string level, string msg)
        {
            Directory.CreateDirectory(LogDir);
            var line = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {level}: {msg}";
            File.AppendAllText(LogPath, line + Environment.NewLine);
        }

        public static void Info(string msg)  => Write("INFO",  msg);
        public static void Error(string msg) => Write("ERROR", msg);

        // Optional: to quickly see where the file is
        public static string PathToFile() => LogPath;
    }
}
