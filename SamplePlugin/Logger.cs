using System;
using System.IO;

namespace MentorRouletteCounter
{
    internal static class Logger
    {
        private static readonly string LogPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\MentorRoulette\\Log.txt";

        static Logger()
        {
            PathHelper.EnsurePathExists(LogPath);
        }

        public static void Log(string message)
        {
            PathHelper.EnsurePathExists(LogPath);
            using StreamWriter writer = File.AppendText(LogPath);
            writer.WriteLine($"[{DateTime.Now:yyyy-dd-MM HH:mm:ss}] {message}");
        }
    }
}
