using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using uzSurfaceMapper.Extensions;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace uzSurfaceMapper.Utils.Benchmarks
{
    public interface IBenchmarkeable
    {
        bool _doBenchmark { get; }
    }

    public interface IBenchmarkBehaviour
    {
        void BehaviourAwake();
    }

    public class BenchmarkData // : MonoBehaviour
    {
        public static ConcurrentDictionary<string, BenchmarkData> benchmarks =
            new ConcurrentDictionary<string, BenchmarkData>();

        public Stopwatch _sw;
        public ConcurrentDictionary<string, BData> data = new ConcurrentDictionary<string, BData>();
        [HideInInspector] public string type;

        private BenchmarkData()
        {
        }

        public BenchmarkData(string type)
        {
            this.type = type;
            AddThis(type);
        }

        protected void AddThis(string type)
        {
            Debug.LogFormat("Added Benchmark of type {0}", type);
            benchmarks.TryAdd(type, this);
        }

        public static string GetReports(BenchmarkReportOrder order, string type, bool toMs)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("Getting report from {0}", type);

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(F.GetSeparator());
            sb.AppendLine();

            var d = GetData(type);
            // DebugReport(d.data);

            var totalTime = d.data.Select(x => GetVal(x.Value.ticks, x.Value.countFirst ? 1 : x.Value.times, true))
                .Sum();

            sb.Append(GetCompleteReport(ref d.data, order, totalTime, toMs));

            var totalCalls = d.data.Sum(x => x.Value.countFirst ? 1 : x.Value.times);

            sb.AppendFormat("Total time: {0} s of {1} s (~{2}%) Avg: {3} | Calls: {4} | Avg / per call: {5}",
                (totalTime / 1000f).ToString("F2"),
                Time.realtimeSinceStartup.ToString("F2"),
                (totalTime / 1000f / Time.realtimeSinceStartup * 100).ToString("F2"),
                GetFormattedString(GetAvg(d.data.Count, totalTime), false),
                totalCalls,
                GetFormattedString(GetAvg(d.data.Count, totalTime, totalCalls), false));

            return sb.ToString();
        }

        private static void DebugReport(ConcurrentDictionary<string, BData> report)
        {
            Debug.Log(string.Join(Environment.NewLine + Environment.NewLine,
                report.Select(x => string.Format("{0}:{2}{1}", x.Key, x.Value.ToString(), Environment.NewLine))
                    .ToArray()));
        }

        private static string GetCompleteReport(ref ConcurrentDictionary<string, BData> report,
            BenchmarkReportOrder order, float totalTime, bool toMs)
        {
            var sb = new StringBuilder();

            if (order == BenchmarkReportOrder.AZ || order == BenchmarkReportOrder.All)
            {
                sb.AppendLine(GetReportHead(BenchmarkReportOrder.AZ));
                report = GetReport(report, BenchmarkReportOrder.AZ);
                sb.AppendLine(GetIndividualReport(report, totalTime, toMs));
            }

            if (order == BenchmarkReportOrder.Desdescing || order == BenchmarkReportOrder.All)
            {
                sb.AppendLine(GetReportHead(BenchmarkReportOrder.Desdescing));
                report = GetReport(report, BenchmarkReportOrder.Desdescing);
                sb.AppendLine(GetIndividualReport(report, totalTime, toMs));
            }

            return sb.ToString();
        }

        private static string GetReportHead(BenchmarkReportOrder order)
        {
            return string.Format("Ordered by {0}:{1}", order.ToString(), Environment.NewLine);
        }

        private static ConcurrentDictionary<string, BData> GetReport(ConcurrentDictionary<string, BData> report,
            BenchmarkReportOrder order)
        {
            if (order == BenchmarkReportOrder.Desdescing || order == BenchmarkReportOrder.All)
                return report.OrderBy(x => x.Value.ticks * (x.Value.countFirst ? 1 : x.Value.times))
                    .ToConcurrentDictionary();

            if (order == BenchmarkReportOrder.AZ || order == BenchmarkReportOrder.All)
                return report.OrderByDescending(x => x.Key).ToConcurrentDictionary();

            return report;
        }

        private static string GetIndividualReport(ConcurrentDictionary<string, BData> report, float totalTime,
            bool toMs)
        {
            var sb = new StringBuilder();

            foreach (var kv in report)
            {
                float time = 0;
                var data = kv.Value;

                sb.AppendFormat("{0}: {1} * {2} = {3} ({4}){5}",
                    kv.Key,
                    GetFormattedString(data.ticks, 1, toMs, false),
                    data.times,
                    GetFormattedString(data.ticks, data.countFirst ? 1 : data.times, toMs, true, out time),
                    GetPerc(time, totalTime),
                    data.countFirst ? " (Only First)" : "");

                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine(F.GetSeparator());

            return sb.ToString();
        }

        private static float GetVal(float ms, int times, bool toMs)
        {
            if (!toMs)
                return ms * times;
            ms /= TimeSpan.TicksPerMillisecond;

            return ms * times;
        }

        private static string GetFormattedString(float ms, int times, bool toMs, bool showGreater)
        {
            float val = 0;
            return GetFormattedString(ms, times, toMs, showGreater, out val);
        }

        private static string GetFormattedString(float ms, int times, bool toMs, bool showGreater, out float val)
        {
            if (!toMs)
            {
                var ticks = ms;
                val = ticks;
                return string.Format("{0} ticks", ticks);
            }

            ms /= TimeSpan.TicksPerMillisecond;

            val = ms * times;
            return GetFormattedString(val, showGreater, toMs);
        }

        private static string GetFormattedString(float ms, bool showGreater, bool toMs = true)
        {
            var ticks = toMs ? (long) ms : (long) ms * TimeSpan.TicksPerMillisecond;
            var isGreater = Mathf.Ceil(ms) > Mathf.Round(ms) && ms < 1000 && showGreater;

            var s = "{0}{1} {2}";
            var isSecs = ms >= 1000;

            return string.Format(s, isGreater ? string.Format("({0} ticks) >", ticks) : "",
                (isSecs ? ms / 1000f : ms).ToString("F2"), isSecs ? "s" : "ms");
        }

        private static string GetPerc(float time, float totalTime)
        {
            // Debug.LogFormat("Time: {0} | Total Time: {1}", time, totalTime);
            return string.Format("~{0} %", (time * 100 / totalTime).ToString("F3"));
        }

        private static float GetAvg(int num, float totalTime, int calls = 1)
        {
            return totalTime / calls / num;
        }

        public static BenchmarkData GetData(string type)
        {
            //Debug.LogFormat("Type: {0} | Available types: {1}", type, string.Join(", ", benchmarks.Keys));
            //Debug.Break();

            if (benchmarks.ContainsKey(type))
                return benchmarks[type];

            return null;
        }

        public void Awake()
        {
            // WIP: Usar reflection para todas las clases que heredan a esta
            // TextureBenchmarkData TextureBenchmarkData = new TextureBenchmarkData();
            // TextureBenchmarkData.BehaviourAwake();
        }

        public class BData
        {
            public bool isBenchmarked, countFirst;
            public float ticks;
            public int times;

            public override string ToString()
            {
                return string.Format("Ticks: {0}{2}Times: {1}", ticks, times, Environment.NewLine);
            }
        }
    }

    public class BenchmarkData<T> : BenchmarkData where T : struct, IConvertible
    {
        protected BenchmarkData()
            : base(typeof(T).GetGenericTypeName())
        {
        }

        public static void StartBenchmark(T method, bool onlyFirst = false) //, string[] childs)
        {
            var id = method.ToString();

            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            var type = typeof(T).ToString();
            var o = GetData(type);
            var obj = o == null ? new BenchmarkData(type) : o;

            if (!obj.data.ContainsKey(id))
            {
                if (obj._sw != null) obj._sw.Reset();
                obj._sw = Stopwatch.StartNew();

                if (obj.data == null)
                    obj.data = new ConcurrentDictionary<string, BData>();

                obj.data.TryAdd(id, new BData());
            }

            obj.data[id].countFirst = onlyFirst;

            if (Monitor.IsEntered(obj.data[id].times))
                ++obj.data[id].times;
            else
                Interlocked.Increment(ref obj.data[id].times);
        }

        public static void PauseBenchmark(T method)
        {
            var sw = GetStopwatch(method);

            if (sw != null)
                sw.Stop();
        }

        public static void ResumeBenchmark(T method)
        {
            var sw = GetStopwatch(method);

            if (sw != null)
                sw.Start();
        }

        public static void StopBenchmark(T method)
        {
            var id = method.ToString();

            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            var type = typeof(T).ToString();
            var obj = GetBenchmarkData(type);

            if (ExistsBenchmark(method) && obj.data[id].isBenchmarked)
                return;

            var sw = GetStopwatch(type);

            if (sw != null)
                sw.Stop();

            var ticks = sw != null ? sw.ElapsedTicks : 0;

            if (ExistsBenchmark(method))
            {
                obj.data[id].ticks = ticks;
                obj.data[id].isBenchmarked = true;
            }
        }

        private static void ShowException(string id, string type, BenchmarkData obj, Exception exception)
        {
            Debug.LogErrorFormat("[{0} -> {1}] Id: {2} | Available ids: {3}\n{4}",
                type,
                benchmarks.Count,
                id,
                string.Join(", ", obj.data.Keys.ToArray()),
                exception);
            Debug.Break();
        }

        public static bool HasBeenBenchmarked(T method)
        {
            var id = method.ToString();
            var type = typeof(T).ToString();
            var obj = GetBenchmarkData(type);

            if (ExistsBenchmark(method))
                return obj.data[id].isBenchmarked;

            return false;
        }

        public static void AddIteration(T method)
        {
            var id = method.ToString();
            var type = typeof(T).ToString();
            var obj = GetBenchmarkData(type);

            try
            {
                if (ExistsBenchmark(method))
                    if (Monitor.IsEntered(obj.data[id].times))
                        ++obj.data[id].times;
                    else
                        Interlocked.Increment(ref obj.data[id].times);
            }
            catch (Exception ex)
            {
                ShowException(id, type, obj, ex);
            }
        }

        public static void AddIteration(T method, int num)
        {
            var id = method.ToString();
            var type = typeof(T).ToString();
            var obj = GetBenchmarkData(type);

            try
            {
                if (ExistsBenchmark(method))
                    if (Monitor.IsEntered(obj.data[id].times))
                        obj.data[id].times += num;
                    else
                        Interlocked.Add(ref obj.data[id].times, num);
            }
            catch (Exception ex)
            {
                ShowException(id, type, obj, ex);
            }
        }

        public static bool ExistsBenchmark(T method)
        {
            var id = method.ToString();

            var type = typeof(T).ToString();
            var obj = GetBenchmarkData(type);

            return obj != null && obj.data.ContainsKey(id);
        }

        public static bool IsBenchmarkNull(T method)
        {
            var id = method.ToString();

            var type = typeof(T).ToString();
            var obj = GetBenchmarkData(type);

            return ExistsBenchmark(method) && obj.data[id] == null;
        }

        private static BenchmarkData GetBenchmarkData(string type)
        {
            return GetData(type);
        }

        private static Stopwatch GetStopwatch(T method)
        {
            return GetStopwatch(method.GetType().Name);
        }

        private static Stopwatch GetStopwatch(string type)
        {
            var sw = GetBenchmarkData(type);

            if (sw != null)
                return sw._sw;

            return null;
        }
    }
}