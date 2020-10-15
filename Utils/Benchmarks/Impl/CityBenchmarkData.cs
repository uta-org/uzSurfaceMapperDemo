namespace uzSurfaceMapper.Utils.Benchmarks.Impl
{
    public enum CityBenchmark
    {
        SavingCity,
        LoadingCity,
        CreateTexture,
        BresenhamTexture,
        CreateBuild,
        LoadingGif,
        SendWebRequest,
        GenerateCity,
        SingleChunkLoop
    }

    public class CityBenchmarkData : BenchmarkData<CityBenchmark>, IBenchmarkeable, IBenchmarkBehaviour
    {
        public static CityBenchmarkData Instance;

        public static bool doBenchmark
        {
            get
            {
                if (Instance == null) Instance = new CityBenchmarkData();
                return Instance._doBenchmark;
            }
        }

        public void BehaviourAwake()
        {
            //Instance = this;
        }

        public bool _doBenchmark =>
            BenchmarkReports.Instance != null ? BenchmarkReports.Instance.doCityBenchmark : false;
    }
}