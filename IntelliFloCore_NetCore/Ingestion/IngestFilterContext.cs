using IntelliFloCore.Database;
using LiteDB;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace IntelliFloCore.Ingestion
{
    public class IngestFilterContext
    {
        private static IngestFilterContext _context;
        private static readonly object _lock = new object();
        public static IngestFilterContext Context
        {
            get
            {
                if (_context == null)
                {
                    lock (_lock)
                    {
                        if (_context == null)
                        {
                            _context = new IngestFilterContext();
                        }
                    }
                }
                return _context;
            }
        }

        private IngestFilterContext()
        {
            var db = LiteDbContext.Database;
            var filtersDbCol = db.GetCollection<IngestFilters>();
            if(filtersDbCol.Query().Count() == 0)
            {
                filtersDbCol.Insert(new IngestFilters() { Name = "Except HealthCheck Logs", Filters = new List<IFDetail> { new IFDetail() { Pattern = "HEALTHCHECK" } } });
                filtersDbCol.Insert(new IngestFilters()
                {
                    Name = "Only Couchbase Specific",
                    Allow = true,
                    Filters = new List<IFDetail>
                    {
                        new IFDetail()
                        {
                            Pattern = "couchbase"
                        },
                        new IFDetail()
                        {
                            Pattern = "customerentity"
                        },
                        new IFDetail()
                        {
                            Pattern = "customerdata"
                        },
                        new IFDetail()
                        {
                            Pattern = "accountentity"
                        },
                        new IFDetail()
                        {
                            Pattern = "accountdata"
                        },
                        new IFDetail()
                        {
                            Pattern = "sessiondata"
                        },
                        new IFDetail()
                        {
                            Pattern = "sessionentity"
                        },
                        new IFDetail()
                        {
                            Pattern = "neuron"
                        },
                        new IFDetail()
                        {
                            Pattern = "jsonreq"
                        },
                        new IFDetail()
                        {
                            Pattern = "jsonres"
                        },
                        new IFDetail()
                        {
                            Pattern = "Message=FOOTPRINT Event="
                        },
                        new IFDetail()
                        {
                            Pattern = "Rules call failed at SEVERITY"
                        }
                    }
                }) ;
            }
            GetEnabledFilter();
        }

        public IngestFilters EnabledFilter { get; set; }
        public List<IngestFilters> GetAllFilter()
        {
            var db = LiteDbContext.Database;
            var filtersDbCol = db.GetCollection<IngestFilters>();

            return filtersDbCol.Query().ToList();
        }

        public bool SetEnabled(int id)
        {
            var db = LiteDbContext.Database;
            var filtersDbCol = db.GetCollection<IngestFilters>();
            var tempFilterObj = filtersDbCol.Query().Where(x => x.Id == id).FirstOrDefault();

            if (tempFilterObj == null)
                return false;

            EnabledFilter = tempFilterObj;

            var enabledFilterDbCol = db.GetCollection<EnabledFilter>();
            EnabledFilter enabledFilter = enabledFilterDbCol.FindAll().FirstOrDefault();
            if (enabledFilter != null)
            {
                enabledFilter.EnabledId = id;
                enabledFilterDbCol.Update(enabledFilter);
            }
            else
            {
                enabledFilter = new EnabledFilter();
                enabledFilter.EnabledId = id;
                enabledFilterDbCol.Insert(enabledFilter);
            }           

            return true;
        }

        private void GetEnabledFilter()
        {
            var db = LiteDbContext.Database;

            var enabledFilterDbCol = db.GetCollection<EnabledFilter>();
            EnabledFilter enabledFilter = enabledFilterDbCol.FindAll().FirstOrDefault();
            if (enabledFilter != null)
            {
                var filtersDbCol = db.GetCollection<IngestFilters>();
                var tempFilterObj = filtersDbCol.Query().Where(x => x.Id == enabledFilter.EnabledId).FirstOrDefault();

                EnabledFilter = tempFilterObj;
            }
        }
    }
}
