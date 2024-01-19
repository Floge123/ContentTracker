using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.IO;
using System.Threading;

namespace MentorRouletteCounter
{
    internal class GilTracker
    {
        private static readonly string ExportPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\MentorRoulette\\GilTrack.txt";
        private Timer m_timer;
        private TimeSpan m_interval;
        private long m_lastGilCount = 0;

        public GilTracker(TimeSpan interval)
        {
            m_interval = interval;                    
        }

        public void Start()
        {
            m_timer = new Timer(Track, null, TimeSpan.Zero, m_interval);
        }

        public void Stop()
        {
            m_timer.Dispose();
        }


        private void Track(object? state)
        {
            try
            {
                long currentGil = GetRetainerGil() + GetCharGil();
                if (currentGil != m_lastGilCount)
                {
                    ExportCurrentGil(currentGil);
                    m_lastGilCount = currentGil;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

        private static void ExportCurrentGil(long gil)
        {
            PathHelper.EnsurePathExists(ExportPath);

            using StreamWriter writer = new StreamWriter(ExportPath, true);
            writer.WriteLine($"{DateTime.Now},{gil}");
        }

        private unsafe long GetRetainerGil()
        {
            RetainerManager manager = *RetainerManager.Instance();
            var retainerCount = manager.GetRetainerCount();
            long gil = 0;
            for (uint i = 0; i < retainerCount; i++)
            {
                var retainer = *manager.GetRetainerBySortedIndex(i);
                gil += retainer.Gil;
            }
            return gil;
        }

        private unsafe long GetCharGil()
        {
            return InventoryManager.Instance()->GetGil();
        }
    }
}
