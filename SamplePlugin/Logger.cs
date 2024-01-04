using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentorRouletteCounter
{
    internal static class Logger
    {
        private const string LogPath = "D:\\Bibliothek\\Dokumente\\MentorRoulette\\Log.txt";

        static Logger()
        {
            PathHelper.EnsurePathExists(LogPath);
        }

        public static void Log(string message)
        {
            PathHelper.EnsurePathExists(LogPath);
            using StreamWriter writer = File.AppendText(LogPath);
            writer.WriteLine($"[{DateTime.Now:yyyy-dd-mm hh:mm:ss}] {message}");
        }
    }
}
