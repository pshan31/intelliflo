namespace IntelliFloCore_NetCore.Ingestion
{
    public static class ServerInsights
    {
        public static ServerUsage GetDetails()
        {
            long size = 0;
            string indexName = "intelliflo_ingest";
            string indexPath = Path.Combine(Environment.CurrentDirectory, indexName);
            if (Directory.Exists(indexName))
            {
                DirectoryInfo dir = new DirectoryInfo(indexPath);
                foreach (FileInfo fi in dir.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    size += fi.Length;
                }
            }

            var freeSpace = new System.IO.DriveInfo("C").AvailableFreeSpace;
            return new ServerUsage()
            {
                FreeSpace = freeSpace,
                Size = size
            };
        }
        /*public static ServerUsage GetDetails()
        {
            long size = 0;
            string indexName = "intelliflo_ingest";
            string indexPath = Path.Combine(Environment.CurrentDirectory, indexName);
            if (Directory.Exists(indexName))
            {
                DirectoryInfo dir = new DirectoryInfo(indexPath);
                foreach (FileInfo fi in dir.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    size += fi.Length;
                }
            }

            var freeSpace = new System.IO.DriveInfo("C").AvailableFreeSpace;
            return new ServerUsage()
            {
                FreeSpace = freeSpace,
                Size = size
            };
        }*/

        public static ServerUsage OldGetDetails()
        {
            //var size = new FileInfo("C:\\IntelliFlo\\IntelliFlo.db").Length;
            var size = new FileInfo("C:\\IntelliFlo\\IntelliFlo.db").Length;
            var freeSpace = new System.IO.DriveInfo("C").AvailableFreeSpace;
            return new ServerUsage()
            {
                FreeSpace = freeSpace,
                Size = size
            };
        }
    }

    public class ServerUsage
    {
        public long Size { get; set; }
        public long FreeSpace { get; set; }
    }
}
