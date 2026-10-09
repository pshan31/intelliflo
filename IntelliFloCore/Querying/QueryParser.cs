using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IntelliFloCore.Querying
{
    public class QueryParser
    {
        public QueryParser() { }

        public List<string> PlainSearches = new List<string>();
        public List<string> OrPlainSearches = new List<string>();
        public List<string> AndPlainSearches = new List<string>();

        public List<string> LogicalSearches = new List<string>();
        public List<string> OrLogicalSearches = new List<string>();
        public List<string> AndLogicalSearches = new List<string>();

        public Dictionary<string, string> Evaluates = new Dictionary<string, string>();

        public Dictionary<string, string> Equals = new Dictionary<string, string>();
        public Dictionary<string, string> OrEquals = new Dictionary<string, string>();
        public Dictionary<string, string> AndEquals = new Dictionary<string, string>();

        public List<string> TableColumns = new List<string>();
        public TimeChartModel model = null;
        public bool HasDedup;

        public void Parse(string query)
        {
            var splits = query.Split('|');

            var firstQuery = splits[0].Trim();
            var fqSplits = SplitString(firstQuery);

            bool followUpLogical = false;
            for (int i = 0; i < fqSplits.Length; i++)
            {
                var split = fqSplits[i].Trim();
                var ipSplit = i + 1 == fqSplits.Length ? null : fqSplits[i + 1].Trim();

                if (IsLogical(ipSplit) || followUpLogical)
                {
                    followUpLogical = IsLogical(ipSplit);
                    i++;

                    // " "
                    if (IsEnclosed(split))
                    {
                        if (ipSplit == "OR")
                            OrPlainSearches.Add(split.Substring(1, split.Length - 2));
                        else
                            AndPlainSearches.Add(split.Substring(1, split.Length - 2));
                    }

                    // + - < > = <= >=   todo
                    else if (IsArithmatic(split))
                    {
                        if (ipSplit == "OR")
                            OrLogicalSearches.Add(split);
                        else
                            AndLogicalSearches.Add(split);
                    }

                    // =
                    else if (IsEquals(split))
                    {
                        var es = split.Split('=');
                        if (ipSplit == "OR")
                            OrEquals.Add(es[0], es[1]);
                        else
                            AndEquals.Add(es[0], es[1]);
                    }

                    // plain text with enclosing
                    else
                    {
                        if (ipSplit == "OR")
                            OrPlainSearches.Add(split);
                        else
                            AndPlainSearches.Add(split);
                    }
                }
                else
                {
                    if (IsEnclosed(split))
                    {
                        PlainSearches.Add(split.Substring(1, split.Length - 2));
                    }
                    else if (IsArithmatic(split))
                    {
                        LogicalSearches.Add(split);
                    }
                    else if (IsEquals(split))
                    {
                        var es = split.Split('=');
                        Equals.Add(es[0], es[1]);
                    }
                    else
                    {
                        PlainSearches.Add(split);
                    }
                }
            }


            for (int j = 1; j < splits.Length; j++)
            {
                var op = splits[j].Trim();
                if (op.StartsWith("eval"))
                {
                    op = op.Replace("eval ", "");
                    var s = op.Split('=');

                    Evaluates.Add(s[0].Trim(), s[1].Trim());
                }
                else if (op.StartsWith("table"))
                {
                    op = op.Replace("table ", "");
                    var s = op.Split(',');

                    foreach (var item in s)
                    {
                        TableColumns.Add(item.Trim());
                    }
                }
                else if (op.StartsWith("dedup"))
                {
                    HasDedup = true;
                }
                else if (op.StartsWith("timechart"))
                {
                    model = ParseTimeChart(op.Trim());
                }
            }
        }

        public bool IsEnclosed(string val)
        {
            return val.StartsWith("\"") && val.EndsWith("\"");
        }

        public bool IsLogical(string val)
        {
            switch (val)
            {
                case "AND":
                case "OR": return true;
                default: return false;
            }
        }

        public bool IsArithmatic(string val)
        {
            switch (val)
            {
                case "=":
                default: return false;
            }
        }

        public bool IsEquals(string val)
        {
            return val.Contains("=");
        }

        public string[] SplitString(string input)
        {
            string pattern = @"(?:[^\s""]+|""[^""]*"")+";
            return Regex.Matches(input, pattern)
                        .Cast<Match>()
                        .Select(m => m.Value)
                        .ToArray();
        }

        public TimeChartModel ParseTimeChart(string input)
        {
            string pattern = @"^timechart( span=(\d+[dms]))? (p90|p99|min|max|avg|count|sum)\((\w+)\)( by (\w+))?$";
            Match match = Regex.Match(input, pattern);

            if (!match.Success)
            {
                throw new ArgumentException("Invalid timechart format.");
            }

            string function = match.Groups[3].Value;
            string field1 = match.Groups[4].Value;
            string field2 = match.Groups[6].Value;

            // Check if span is provided
            string span = match.Groups.Count == 7 ? match.Groups[2].Value : null;

            return new TimeChartModel
            {
                Function = function,
                Field1 = field1,
                Field2 = field2,
                Span = span
            };
        }


        public class TimeChartModel
        {
            public string Function { get; set; }
            public string Field1 { get; set; }
            public string Field2 { get; set; }
            public string Span { get; set; }
        }
    }
}
