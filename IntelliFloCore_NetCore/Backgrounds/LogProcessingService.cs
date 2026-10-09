using IntelliFloCore.Database;
using IntelliFloCore.Ingestion;
using LiteQueue;
using Microsoft.Extensions.Hosting;

namespace IntelliFloCore_NetCore.Backgrounds
{
    public class LogProcessingService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private Timer? _timer = null;
        private Task _task;

        public LogProcessingService()
        {
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _task = DoWork(stoppingToken);
            return Task.CompletedTask;

        }


        private async Task DoWork(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                int i = 1;
                bool checkpoint = true;
                List<Task> tasks = new List<Task>();

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (tasks.Count == LiteDbContext.TaskSize)
                        {
                            await Task.WhenAny(tasks);
                        }


                        if (LiteDbContext.MyQueue.TryDequeue(out string payload))
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                FastLogIngestor logIngestor = new FastLogIngestor();
                                logIngestor.ParseAndIngestLog(payload);
                            }));
                        }
                        else
                        {
                            Thread.Sleep(200);
                        }

                        ////if (i % 200 == 0 && checkpoint)
                        ////{
                        ////    checkpoint = false;
                        ////    LiteDbContext.Database.Checkpoint();
                        ////}
                        //if ((entry = LiteDbContext.Queue.Dequeue()) != null)
                        //{

                        //    try
                        //    {
                        //        LogIngestor logIngestor = new LogIngestor();
                        //        logIngestor.ParseAndIngestLog(entry.Payload);
                        //        LiteDbContext.Queue.Commit(entry);
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        File.AppendAllText("E:\\Logs\\B.txt", ex.Message + ex.StackTrace + "\n\n");
                        //        LiteDbContext.Queue.Commit(entry);
                        //    }
                        //    i++;
                        //    //checkpoint = true;
                        //}
                        //else
                        //{
                        //    //LiteDbContext.Database.Dispose();
                        //    //LiteDbContext.Database = null;
                        //    Thread.Sleep(1000);
                        //}

                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText("E:\\Logs\\B.txt", ex.Message + ex.StackTrace + "\n\n");
                    }
                }
            });
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
