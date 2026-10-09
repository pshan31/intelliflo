using System;
using System.Collections.Generic;

namespace IntelliFloCore.Querying
{
    public class QueryMModel
    {
        public List<QueryModel> LstQM { get; set; }
        public IDictionary<string, IEnumerable<KeyValuePair<DateTime, double>>> Visualization { get; set; }
    }

    public class QueryModel
    {
        public string RawMessage { get; set; }
        public string Date { get; set; }
        public List<Tag> Tags { get; set; }
    }

    public class Tag
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
