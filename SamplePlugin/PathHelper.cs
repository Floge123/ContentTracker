using System.IO;

namespace MentorRouletteCounter
{
    internal static class PathHelper
    {
        public static void EnsurePathExists(string path)
        {
            string directoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Check if the file exists, create it if it doesn't.
            if (!File.Exists(path))
            {
                using (File.Create(path)) { } // Create the file and immediately close it.
            }
        }
    }
}
