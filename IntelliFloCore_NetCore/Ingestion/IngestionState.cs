using System.Timers;
using System;
using Timer = System.Timers.Timer;

namespace IntelliFloCore.Ingestion
{
    public static class IngestionState
    {
        public static bool IsActive = false;

        static IngestionState()
        {
            Timer aTimer = new System.Timers.Timer();
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
