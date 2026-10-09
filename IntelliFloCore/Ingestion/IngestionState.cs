using System.Timers;
using System;

namespace IntelliFloCore.Ingestion
{
    static class IngestionState
    {
        public static bool IsActive = false;
        static IngestionState()
        {
            Timer aTimer = new Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 60000;
            aTimer.Enabled = true;
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            IsActive = false;
        }
    }
}
