using Deedle;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IntelliFloCore.Querying.QueryParser;

namespace IntelliFloCore.Querying
{
    public class TimeChartQuery
    {
        public IDictionary<string, IEnumerable<KeyValuePair<DateTime, double>>> Parse(IDictionary<string, IDictionary<DateTime, List<double>>> timeseries_list, TimeChartModel model)
        {
            Dictionary<string, IEnumerable<KeyValuePair<DateTime, double>>> result = new Dictionary<string, IEnumerable<KeyValuePair<DateTime, double>>>();
            foreach (var item in timeseries_list)
            {
                var series = new Series<DateTime, List<double>>(item.Value);
                TimeSpan span = TimeSpan.FromMilliseconds(10);
                if (model.Span.EndsWith("m"))
                {
                    var val = Convert.ToInt32(model.Span.Replace("m", ""));
                    span = TimeSpan.FromMinutes(val);
                }
                else if (model.Span.EndsWith("s"))
                {
                    var val = Convert.ToInt32(model.Span.Replace("s", ""));
                    span = TimeSpan.FromSeconds(val);
                }
                else if (model.Span.EndsWith("h"))
                {
                    var val = Convert.ToInt32(model.Span.Replace("h", ""));
                    span = TimeSpan.FromHours(val);
                }

                if (model.Function.StartsWith("p"))
                {
                    var val = Convert.ToInt32(model.Function.Replace("p", ""));
                    result.Add(item.Key, GroupByIntervalPercentile(series, span, val));
                }

                else if (model.Function == "avg")
                {
                    result.Add(item.Key, GroupByIntervalAverage(series, span));

                }

                else if (model.Function == "count")
                {
                    result.Add(item.Key, GroupByIntervalCount(series, span));

                }

                else if (model.Function == "sum")
                {
                    result.Add(item.Key, GroupByIntervalSum(series, span));
                }

                else if (model.Function == "min")
                {
                    result.Add(item.Key, GroupByIntervalMin(series, span));
                }

                else if (model.Function == "max")
                {
                    result.Add(item.Key, GroupByIntervalMax(series, span));
                }
            }
            return result;
        }


        public static IEnumerable<KeyValuePair<DateTime, double>> GroupByIntervalCount(Series<DateTime, List<double>> data, TimeSpan interval)
        {
            var groupedData = data.GroupBy(kvp => kvp.Key - TimeSpan.FromTicks(kvp.Key.Ticks % interval.Ticks))
                                  .Select(g => (double)g.Value.Observations.Select(x => (double)x.Value.Count()).Sum());
            return groupedData.Observations.ToList();
        }


        public static IEnumerable<KeyValuePair<DateTime, double>> GroupByIntervalSum(Series<DateTime, List<double>> data, TimeSpan interval)
        {
            var groupedData = data.GroupBy(kvp => kvp.Key - TimeSpan.FromTicks(kvp.Key.Ticks % interval.Ticks))
                                  .Select(g => (double)g.Value.Observations.Select(x => (double)x.Value.Sum()).Sum());
            return groupedData.Observations.ToList();
        }


        public static IEnumerable<KeyValuePair<DateTime, double>> GroupByIntervalMin(Series<DateTime, List<double>> data, TimeSpan interval)
        {
            var groupedData = data.GroupBy(kvp => kvp.Key - TimeSpan.FromTicks(kvp.Key.Ticks % interval.Ticks))
                                  .Select(g => (double)g.Value.Observations.Select(x => (double)x.Value.Min()).Min());
            return groupedData.Observations.ToList();
        }


        public static IEnumerable<KeyValuePair<DateTime, double>> GroupByIntervalMax(Series<DateTime, List<double>> data, TimeSpan interval)
        {
            var groupedData = data.GroupBy(kvp => kvp.Key - TimeSpan.FromTicks(kvp.Key.Ticks % interval.Ticks))
                                  .Select(g => (double)g.Value.Observations.Select(x => (double)x.Value.Max()).Max());
            return groupedData.Observations.ToList();
        }

        public static IEnumerable<KeyValuePair<DateTime, double>> GroupByIntervalPercentile(Series<DateTime, List<double>> data, TimeSpan interval, int percentile)
        {
            List<KeyValuePair<DateTime, double>> result = new List<KeyValuePair<DateTime, double>>();

            var groupedData = data.GroupBy(kvp => kvp.Key - TimeSpan.FromTicks(kvp.Key.Ticks % interval.Ticks));

            foreach (var item in groupedData.Observations)
            {
                List<double> doubles = new List<double>();
                foreach (var iItem in item.Value.Observations)
                {
                    doubles.AddRange(iItem.Value);
                }
                result.Add(new KeyValuePair<DateTime, double>(item.Key, Statistics.Percentile(doubles, percentile)));
            }

            return result;
        }

        public static IEnumerable<KeyValuePair<DateTime, double>> GroupByIntervalAverage(Series<DateTime, List<double>> data, TimeSpan interval)
        {
            List<KeyValuePair<DateTime, double>> result = new List<KeyValuePair<DateTime, double>>();

            var groupedData = data.GroupBy(kvp => kvp.Key - TimeSpan.FromTicks(kvp.Key.Ticks % interval.Ticks));

            foreach (var item in groupedData.Observations)
            {
                List<double> doubles = new List<double>();
                foreach (var iItem in item.Value.Observations)
                {
                    doubles.AddRange(iItem.Value);
                }
                result.Add(new KeyValuePair<DateTime, double>(item.Key, doubles.Average()));
            }

            return result;
        }
    }
}
