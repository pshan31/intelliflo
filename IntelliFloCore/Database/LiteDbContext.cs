using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
                            _database = new LiteDatabase("Filename=C:\\Users\\316940\\source\\repos\\IntelliFlo\\IntelliFlo\\IntelliFlo.db; mode=Exclusive;");
                            //if (!Directory.Exists("E:\\IntelliFlo"))
                            //    Directory.CreateDirectory("E:\\IntelliFlo");

                            //_database = new LiteDatabase("Filename=E:\\IntelliFlo\\IntelliFlo.db; mode=Exclusive;");
                        }
                    }
                }
                return _database;
            }
        }

    }
}
