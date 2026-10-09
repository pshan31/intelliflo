using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliFloCore.Querying
{
    public class QueryRequestModel
    {
        public string query { get; set; }
        public RequestSearchMode requestSearchMode { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string fixedStr { get; set; }
        public DateSearchCriteria dateSearchCriteria { get; set; }
    }

    public enum RequestSearchMode
    {
        VERBOSE, FAST
    }

    public enum DateSearchCriteria
    {
        FIXED, CUSTOM
    }
}
