using System.IO;
using UnityEngine;
using uzSurfaceMapper.Core.Attrs;
using uzSurfaceMapper.Core.Func;

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

        [InvokeAtAwake]
        public override void InvokeAtAwake()
        {
            base.InvokeAtAwake();

            // The first thing we will do, is fill the fields of city class (to allow it to have what it need)
            if (SConv == null)
                SConv = new SceneConversion(realScaleZoom, singlePlaneSize, conversionFactor, new Vector2(mapWidth, mapHeight));

            DoesCityFileExists = File.Exists(CityJSONPath);
        }
    }
}