using System.IO;
using System.Reflection;
using System.Threading;
using UnityEngine;
using uzSurfaceMapper.Core.Func;
using uzSurfaceMapper.Core.Attrs.CodeAnalysis;

namespace uzSurfaceMapper.Core.Generators
{
    public sealed partial class CityGenerator : MapGenerator
    {
        /// <summary>
        ///     The real scale zoom (if the map texture is 7000x5000 this will generate a plane of 42kmx30km)
        /// </summary>
        public float realScaleZoom = 1;

        /// <summary>
        ///     The conversion factor
        /// </summary>
        public float conversionFactor = 1f; // This must be 1

        /// <summary>
        ///     The single plane size
        /// </summary>
        private float singlePlaneSize = -1;

        public static SceneConversion SConv { get; private set; }

        public static bool DoesCityFileExists { get; private set; }

        /// <summary>
        ///     Gets the city json path.
        /// </summary>
        /// <value>
        ///     The city json path.
        /// </value>
        protected static string CityJSONPath => GetOutputSavePath();

        public void OnEnable()
        {
            DelegateFuncs.Add(() =>
            {
                // The first thing we will do, is fill the fields of city class (to allow it to have what it need)
                if (SConv == null)
                    SConv = new SceneConversion(realScaleZoom, singlePlaneSize, conversionFactor, new Vector2(mapWidth, mapHeight));

                DoesCityFileExists = File.Exists(CityJSONPath);
            });
        }

        /// <summary>
        ///     Gets the save path.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <returns></returns>
        protected static string GetOutputSavePath(string name = "city")
        {
            string file = Path.Combine(GetStreamingAssetsPath(), "Generated Output", $"{name}.json"),
                folder = Path.GetDirectoryName(file);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return file.Replace(@"\", "/");
        }

        /// <summary>
        ///     Gets the streaming assets path.
        /// </summary>
        /// <returns></returns>
        [MustBeReviewed]
        private static string GetStreamingAssetsPath()
        {
            string streamingAssets;
            if (Thread.CurrentThread != mainThread)
            {
                var assLocation =
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));

#if UNITY_EDITOR
                streamingAssets = Path.Combine(assLocation, "Assets", "StreamingAssets");
#else
                streamingAssets =
 Path.Combine(assLocation, new DirectoryInfo(assLocation).GetDirectories("*_Data*", SearchOption.TopDirectoryOnly).First().Name, "StreamingAssets");
#endif
            }
            else
            {
                streamingAssets = Application.streamingAssetsPath;
            }

            return streamingAssets;
        }
    }
}