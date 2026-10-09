using IntelliFloCore.Common;
using IntelliFloCore.Database;
using IntelliFloCore_NetCore.Ingestion;
using LiteDB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliFloCore.Querying
{
    public class Query
    {
        public Query() { }

        public QueryMModel GetFastLogs(QueryRequestModel request)
        {
            QueryMModel model = new QueryMModel();
            List<QueryModel> lstQM = new List<QueryModel>();
            QueryParser queryParser = new QueryParser();
            queryParser.FastParse(request.query.Replace("\n", " "));

            ConcurrentDictionary<string, ConcurrentDictionary<DateTime, List<double>>> timechartHosts = new ConcurrentDictionary<string, ConcurrentDictionary<DateTime, List<double>>>();
            //var startDate = DateTime.Now.AddDays(-15).ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            //var endDate = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            long startDate = 0;
            long endDate = 0;
            if (request.dateSearchCriteria == DateSearchCriteria.FIXED)
            {
                if (request.fixedStr.EndsWith("m"))
                {
                    var val = Convert.ToInt32(request.fixedStr.Replace("m", ""));
                    startDate = DateTime.Now.ToUniversalTime().AddMinutes(-1 * val).Ticks;
                    endDate = DateTime.Now.ToUniversalTime().Ticks;
                }

                else if (request.fixedStr.EndsWith("s"))
                {
                    var val = Convert.ToInt32(request.fixedStr.Replace("s", ""));
                    startDate = DateTime.Now.ToUniversalTime().AddSeconds(-1 * val).Ticks;
                    endDate = DateTime.Now.ToUniversalTime().Ticks;
                }

                else if (request.fixedStr.EndsWith("h"))
                {
                    var val = Convert.ToInt32(request.fixedStr.Replace("h", ""));
                    startDate = DateTime.Now.ToUniversalTime().AddHours(-1 * val).Ticks;
                    endDate = DateTime.Now.ToUniversalTime().Ticks;
                }
                else return model;
            }
            else
            {
                startDate = request.startDate.Ticks;
                endDate = request.endDate.Ticks;
            }

            var searchData = FastIngestCommon.SearchData(request.query, startDate, endDate, queryParser.model != null);

            try
            {
                Parallel.ForEach(searchData, new ParallelOptions() { MaxDegreeOfParallelism = 100000 }, item =>
                {
                    try
                    {
                        QueryModel queryModel = new QueryModel();
                        queryModel.RawMessage = item.Get("_rawmessage");
                        queryModel.Date = item.Get("datetime");
                        queryModel.Tags = new List<Tag>();


                        if (queryParser.model != null)
                        {
                            IDictionary<DateTime, List<double>> timechart = null;
                            if (!string.IsNullOrEmpty(queryParser.model.Field2))
                            {
                                var field2Value = item.Get(queryParser.model.Field2);

                                if (timechartHosts.ContainsKey(field2Value))
                                {
                                    timechart = timechartHosts[field2Value];
                                }
                                else
                                {

                                    timechartHosts.TryAdd(field2Value, new ConcurrentDictionary<DateTime, List<double>>());
                                    timechart = timechartHosts[field2Value];
                                }
                            }
                            else
                            {
                                if (timechartHosts.ContainsKey(queryParser.model.Function + "(" + queryParser.model.Field1 + ")"))
                                {
                                    timechart = timechartHosts[queryParser.model.Function + "(" + queryParser.model.Field1 + ")"];
                                }
                                else
                                {
                                    timechartHosts.TryAdd(queryParser.model.Function + "(" + queryParser.model.Field1 + ")", new ConcurrentDictionary<DateTime, List<double>>());
                                    timechart = timechartHosts[queryParser.model.Function + "(" + queryParser.model.Field1 + ")"];
                                }
                            }

                            var datetime = new DateTime(long.Parse(item.Get("Timestamp")));

                            var tO = timechart.ContainsKey(datetime) ? timechart[datetime] : null;
                            if (tO == null)
                            {
                                timechart[datetime] = new List<double>();
                                tO = timechart[datetime];
                            }
                            tO.Add(double.Parse(item.Get(queryParser.model.Field1)));
                        }

                        foreach (var i in item.Fields)
                        {
                            if (i.Name != "_rawmessage" && i.Name != "datetime" && i.Name != "_id")
                            {
                                Tag tag = new Tag();
                                tag.Key = i.Name.ToString().Replace("\"", "");
                                tag.Value = item.Get(i.Name).ToString().Replace("\"", "");
                                queryModel.Tags.Add(tag);
                            }
                        }

                        lstQM.Add(queryModel);

                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
            catch (Exception ex)
            {

            }
            TimeChartQuery timeChartQuery = new TimeChartQuery();

            if (queryParser.model != null)
            {
                model.Visualization = timeChartQuery.Parse(timechartHosts, queryParser.model);

                ConcurrentBag<DateTime> list = new ConcurrentBag<DateTime>();
                Parallel.ForEach(model.Visualization, new ParallelOptions() { MaxDegreeOfParallelism = 100000 }, item =>
                {
                    foreach (var i2 in item.Value)
                    {
                        if (!list.Contains(i2.Key))
                            list.Add(i2.Key);
                    }
                });

                var finalList = list.OrderBy(b => b).Distinct().ToList();
                model.Labels = finalList;
            }
            else if (request.requestSearchMode == RequestSearchMode.VERBOSE)
                model.LstQM = lstQM;

            return model;
        }

        public QueryMModel GetLogs(QueryRequestModel request)
        {
            QueryMModel model = new QueryMModel();
            List<QueryModel> lstQM = new List<QueryModel>();
            QueryParser queryParser = new QueryParser();
            queryParser.Parse(request.query.Replace("\n", " "));
            var db = LiteDbContext.Database;
            var collection = db.GetCollection<BsonDocument>("Log");
            try
            {
                collection.EnsureIndex("$._rawmessage");
            }
            catch (Exception ex)
            {

            }
            collection.EnsureIndex("$.datetime");
            collection.EnsureIndex("$.Type");
            var dbQuery = collection.Query();
            ConcurrentDictionary<string, ConcurrentDictionary<DateTime, List<double>>> timechartHosts = new ConcurrentDictionary<string, ConcurrentDictionary<DateTime, List<double>>>();

            var str = new StringBuilder();

            /*  if (queryParser.PlainSearches.Count > 0)
              {
                  int i = 0;
                  foreach (var ps in queryParser.PlainSearches)
                  {
                      if (i > 0)
                          str.Append(" AND ");
                      str.Append($"$._rawmessage LIKE '%{ps}%'");
                      i++;
                  }
              }
            */
            if (queryParser.Equals.Count > 0)
            {
                int i = 0;
                foreach (var ps in queryParser.Equals)
                {
                    if (i > 0)
                        str.Append(" AND ");

                    if (ps.Value.Contains("*"))
                    {
                        var val = ps.Value.Replace("*", "%");
                        str.Append($"$.{ps.Key} LIKE '{val}'");
                    }
                    else if (ps.Value.IsNumeric())
                        str.Append($"$.{ps.Key}={ps.Value}");
                    else
                        str.Append($"$.{ps.Key}='{ps.Value}'");
                    i++;
                }
            }

            //var startDate = DateTime.Now.AddDays(-15).ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            //var endDate = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            var startDate = "";
            var endDate = "";
            if (request.dateSearchCriteria == DateSearchCriteria.FIXED)
            {
                if (request.fixedStr.EndsWith("m"))
                {
                    var val = Convert.ToInt32(request.fixedStr.Replace("m", ""));
                    startDate = DateTime.Now.ToUniversalTime().AddMinutes(-1 * val).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
                    endDate = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
                }

                else if (request.fixedStr.EndsWith("s"))
                {
                    var val = Convert.ToInt32(request.fixedStr.Replace("s", ""));
                    startDate = DateTime.Now.ToUniversalTime().AddSeconds(-1 * val).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
                    endDate = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
                }

                else if (request.fixedStr.EndsWith("h"))
                {
                    var val = Convert.ToInt32(request.fixedStr.Replace("h", ""));
                    startDate = DateTime.Now.ToUniversalTime().AddHours(-1 * val).AddDays(-4).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
                    endDate = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
                }
                else return model;
            }
            else
            {
                startDate = request.startDate.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
                endDate = request.endDate.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            }
            //str.Append(" AND $.datetime>={$date:'" + startDate + "'} AND $.datetime<={$date:'" + endDate + "'}");
            str.Append("$.datetime>={$date:'" + startDate + "'} AND $.datetime<={$date:'" + endDate + "'}");

            var d = dbQuery.Where(str.ToString()).Limit(100000000).ToEnumerable(); //.ToEnumerable();

            try
            {
                Parallel.ForEach(d, new ParallelOptions() { MaxDegreeOfParallelism = 100000 }, item =>
                {
                    try
                    {
                        QueryModel queryModel = new QueryModel();
                        queryModel.RawMessage = item["_rawmessage"].AsString;
                        queryModel.Date = item["datetime"].AsDateTime.ToString();
                        queryModel.Tags = new List<Tag>();
                        if ((queryModel.RawMessage.Contains("NeuronPut", StringComparison.InvariantCultureIgnoreCase) || queryModel.RawMessage.Contains("NeuronGet", StringComparison.InvariantCultureIgnoreCase)) && (!queryModel.RawMessage.Contains("AuthHandlerAbstract", StringComparison.InvariantCultureIgnoreCase) && !queryModel.RawMessage.Contains("HttpAuthHandler", StringComparison.InvariantCultureIgnoreCase)) && queryModel.RawMessage.Contains("CallStatus=Completed", StringComparison.InvariantCultureIgnoreCase) && queryModel.RawMessage.Contains("RespInMs=", StringComparison.InvariantCultureIgnoreCase))
                        //if (queryModel.RawMessage.Contains(" type=", StringComparison.InvariantCultureIgnoreCase) && queryModel.RawMessage.Contains("RiskCacheManager", StringComparison.InvariantCultureIgnoreCase) && queryModel.RawMessage.Contains("ResponseInMS", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (queryParser.model != null)
                            {
                                IDictionary<DateTime, List<double>> timechart = null;
                                if (!string.IsNullOrEmpty(queryParser.model.Field2))
                                {
                                    var field2Value = item[queryParser.model.Field2].AsString;

                                    if (timechartHosts.ContainsKey(field2Value))
                                    {
                                        timechart = timechartHosts[field2Value];
                                    }
                                    else
                                    {

                                        timechartHosts.TryAdd(field2Value, new ConcurrentDictionary<DateTime, List<double>>());
                                        timechart = timechartHosts[field2Value];
                                    }
                                }
                                else
                                {
                                    if (timechartHosts.ContainsKey(queryParser.model.Function + "(" + queryParser.model.Field1 + ")"))
                                    {
                                        timechart = timechartHosts[queryParser.model.Function + "(" + queryParser.model.Field1 + ")"];
                                    }
                                    else
                                    {
                                        timechartHosts.TryAdd(queryParser.model.Function + "(" + queryParser.model.Field1 + ")", new ConcurrentDictionary<DateTime, List<double>>());
                                        timechart = timechartHosts[queryParser.model.Function + "(" + queryParser.model.Field1 + ")"];
                                    }
                                }

                                var datetime = item["datetime"].AsDateTime;

                                var tO = timechart.ContainsKey(datetime) ? timechart[datetime] : null;
                                if (tO == null)
                                {
                                    timechart[datetime] = new List<double>();
                                    tO = timechart[datetime];
                                }
                                tO.Add(item[queryParser.model.Field1].AsDouble);
                            }

                            foreach (var i in item)
                            {
                                if (i.Key != "_rawmessage" && i.Key != "datetime" && i.Key != "_id")
                                {
                                    Tag tag = new Tag();
                                    tag.Key = i.Key.ToString().Replace("\"", "");
                                    tag.Value = i.Value.ToString().Replace("\"", "");
                                    queryModel.Tags.Add(tag);
                                }
                            }

                            lstQM.Add(queryModel);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }
            catch (Exception ex)
            {

            }
            TimeChartQuery timeChartQuery = new TimeChartQuery();

            if (request.requestSearchMode != RequestSearchMode.VERBOSE)
                model.LstQM = lstQM;


            if (queryParser.model != null)
            {
                model.Visualization = timeChartQuery.Parse(timechartHosts, queryParser.model);

                ConcurrentBag<DateTime> list = new ConcurrentBag<DateTime>();
                Parallel.ForEach(model.Visualization, new ParallelOptions() { MaxDegreeOfParallelism = 100000 }, item =>
                {
                    foreach (var i2 in item.Value)
                    {
                        if (!list.Contains(i2.Key))
                            list.Add(i2.Key);
                    }
                });

                var finalList = list.OrderBy(b => b).Distinct().ToList();
                model.Labels = finalList;
            }

            return model;
        }

        private static bool WhereClause(QueryParser qp, string logMsg)
        {
            bool flag = true;
            logMsg = logMsg.ToLower();
            foreach (var item in qp.PlainSearches)
            {
                if (!logMsg.Contains(item.ToLower()))
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }
    }
}
