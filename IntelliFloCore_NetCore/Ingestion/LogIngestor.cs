using IntelliFloCore.Database;
using System.Text.RegularExpressions;
using LiteDB;
using IntelliFloCore.Common;
using Newtonsoft.Json.Linq;
using log4net;

namespace IntelliFloCore.Ingestion
{
    public class LogIngestor
    {
        private ILog _log4net;

        static LogIngestor()
        {
        }
        public LogIngestor() {
            _log4net = log4net.LogManager.GetLogger(typeof(LogIngestor));
        }

        private bool IsApplicableLog(string rawMessage)
        {
            if(!rawMessage.Contains("BLZCMUSTANG"))
                return false;

            var filter = IngestFilterContext.Context.EnabledFilter;
            if (filter == null)
                return true;

            if (filter.Allow)
            {
                foreach (var item in filter.Filters)
                {
                    if (rawMessage.Contains(item.Pattern, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
            }
            else
            {
                bool flag = true;
                foreach (var item in filter.Filters)
                {
                    if (rawMessage.Contains(item.Pattern, StringComparison.InvariantCultureIgnoreCase))
                        flag = false;
                }
                return flag;
            }

            return false;
        }

        public bool ParseAndIngestLog(string rawMessage)
        {
            if (!IsApplicableLog(rawMessage))
            {
                return false;
            }

            IngestionState.IsActive = true;
            if (!rawMessage.EndsWith("}"))
                rawMessage += "}";

            var jsonObj = JObject.Parse(rawMessage);

            var eventMsg = jsonObj.SelectToken("event")?.ToString();
            var dateStr = eventMsg.Length > 25 ? eventMsg.Substring(0, 23) : "";

            var date = GetInUTC(dateStr);
            var pairs = ParseLogMessage(rawMessage);

            var db = LiteDbContext.Database;
            {
                var collection = db.GetCollection<BsonDocument>("Log");

                var log = new BsonDocument();
                log["_rawmessage"] = eventMsg;
                if (date != null)
                    log["datetime"] = date;

                if (pairs != null)
                {
                    foreach (var item in pairs)
                    {
                        if (item.Value.IsNumeric(out double converted))
                            log[item.Key] = converted;
                        else
                            log[item.Key] = item.Value;
                    }
                }

                log["host"] = jsonObj.SelectToken("host")?.ToString();
                log["source"] = jsonObj.SelectToken("source")?.ToString();
                log["sourcetype"] = jsonObj.SelectToken("sourcetype")?.ToString();
                log["index"] = jsonObj.SelectToken("index")?.ToString();

                collection.Insert(log);
            }
            return true;
        }



        private Dictionary<string, string> ParseLogMessage(string rawMessage)
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            try
            {
                // Define the regular expression pattern to match key-value pairs
                string pattern = @"(\w+)=(\S+)";

                // Match all key-value pairs in the log message
                MatchCollection matches = Regex.Matches(rawMessage, pattern);

                // Iterate over matches and add them to the dictionary
                foreach (Match match in matches)
                {
                    // Extract key and value from the match
                    string key = match.Groups[1].Value;
                    string value = match.Groups[2].Value;

                    // Add key-value pair to the dictionary
                    keyValuePairs[key] = value;
                }
            }
            catch { }
            return keyValuePairs;
        }

        private DateTime? GetInUTC(string edtDateString)
        {
            try
            {
                DateTime edtDateTime = DateTime.ParseExact(edtDateString, "yyyy-MM-dd HH:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture);

                DateTime utcDateTime = TimeZoneInfo.ConvertTimeToUtc(edtDateTime, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

                return utcDateTime;
            }
            catch
            {
                return null;
            }
        }

        public bool QueueLog(string rawMessage)
        {
            if (!IsApplicableLog(rawMessage))
            {
                return false;
            }
            try
            {
                //_log4net.Info(rawMessage);
                LiteDbContext.MyQueue.Enqueue(rawMessage);
                //LiteDbContext.Queue.Enqueue(rawMessage);
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
    }
}


