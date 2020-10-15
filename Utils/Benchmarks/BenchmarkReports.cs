using System.Collections;
using UnityEngine;

namespace uzSurfaceMapper.Utils.Benchmarks
{
    // [RequireComponent(typeof(BenchmarkData))]

    public enum BenchmarkReportOrder
    {
        All,
        AZ,
        Desdescing
    }

    public class BenchmarkReports : MonoBehaviour
    {
        public static BenchmarkReports Instance;

        public bool doTextureBenchmark = true,
            doCityBenchmark = true;

        private void Awake()
        {
            Instance = this;
        }

        public void OnLoaderFinished()
        {
            StartCoroutine(GetReport());
        }

        private IEnumerator GetReport()
        {
            yield return new WaitForSeconds(2);

            GetActualReport(BenchmarkReportOrder.All);
        }

        public void GetActualReport(BenchmarkReportOrder order)
        {
            // Debug.LogFormat("Reports num: {0}", BenchmarkData.benchmarks.Count);

            foreach (var report in BenchmarkData.benchmarks)
                Debug.Log(BenchmarkData.GetReports(order, report.Value.type, true));
        }
    }
}