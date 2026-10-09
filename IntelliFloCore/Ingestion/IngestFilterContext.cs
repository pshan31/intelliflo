using IntelliFloCore.Database;
using LiteDB;
using System.Collections.Generic;
using System.Linq;

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
