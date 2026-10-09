using System.Collections.Generic;

namespace IntelliFloCore.Ingestion
{
    public class IngestFilters
    {
        public int Id { get; set; }
        public bool Allow { get; set; }
        public string Name { get; set; }
        public List<IFDetail> Filters { get; set; }
    }

    public class IFDetail
    {
        public string Pattern { get; set; }
    }
}
