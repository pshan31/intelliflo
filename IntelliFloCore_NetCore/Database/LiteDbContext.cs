using IntelliFloCore.Ingestion;
using LiteDB;
using LiteQueue;
using System.Collections.Concurrent;

namespace IntelliFloCore.Database
{
    public class LiteDbContext
    {
        private static LiteDatabase _database;
        private static readonly object _lock = new object();
        public static LiteDatabase Database
        {
            get
            {
                if (_database == null)
                {
                    lock (_lock)
                    {
                        if (_database == null)
                        {
                            //_database = new LiteDatabase("C:\\IntelliFlo\\IntelliFlo.db");
                            if (!Directory.Exists("C:\\IntelliFlo"))
                                Directory.CreateDirectory("C:\\IntelliFlo");

                            ConnectionString connectionString = new ConnectionString();
                            connectionString.Connection = ConnectionType.Direct;
                            connectionString.Filename = "C:\\IntelliFlo\\IntelliFlo.db";
                            _database = new LiteDatabase(connectionString);
                        }
                    }
                }
                return _database;
            }
        }

        public static ConcurrentQueue<string> MyQueue = new ConcurrentQueue<string>();
        public static int TaskSize { get; set; } = 2000;

        public static bool Enqueue(string message)
        {
            MyQueue.Enqueue(message);
            return true;            
        }

        private static LiteQueue<string> _queue;
        private static readonly object _lock2 = new object();
        public static LiteQueue<string> Queue
        {
            get
            {
                if (_queue == null)
                {
                    lock (_lock2)
                    {
                        if (_queue == null)
                        {
                            _queue= new LiteQueue<string>(Database, "queuedLogs", true);
                            _queue.ResetOrphans();
                        }
                    }
                }
                return _queue;
            }
        }


        public static bool CleanupLogs()
        {
            if (!IngestionState.IsActive)
            {
                try
                {
                    Database.DropCollection("Log");
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else return false;
        }
    }
}
