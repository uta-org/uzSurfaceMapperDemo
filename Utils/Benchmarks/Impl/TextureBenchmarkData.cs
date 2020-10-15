namespace uzSurfaceMapper.Utils.Benchmarks.Impl
{
    public enum TextureBenchmark
    {
        ResourcesLoad,
        GetPixels,
        CreateArray,
        CastFrom,
        CastBack,
        SetPixels,
        FloodFilling,
        ShowExceptions,
        ShowingIteratedPixels,
        TextureMixing,
        Disposing,
        SaveTexture,
        Destroy,
        Generate,
        PartitionerCreate,
        UnloadAsset,
        TerraGen,
        MapSerializating,
        TexSmoothing,
        FindBridges,
        DebuggingInfo
    }

    public class TextureBenchmarkData : BenchmarkData<TextureBenchmark>, IBenchmarkeable, IBenchmarkBehaviour
    {
        public static TextureBenchmarkData Instance;

        public static bool doBenchmark
        {
            get
            {
                if (Instance == null) Instance = new TextureBenchmarkData();
                return Instance._doBenchmark;
            }
        }

        public void BehaviourAwake()
        {
            //Instance = this;
        }

        public bool _doBenchmark =>
            BenchmarkReports.Instance != null ? BenchmarkReports.Instance.doTextureBenchmark : false;
    }
}