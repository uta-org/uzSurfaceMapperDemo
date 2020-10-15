using System;
using System.Collections;
using System.IO;
using CielaSpike;
using UnityEngine;
using UnityEngine.Utils.DebugTools;
using UnityGif;
using uzSurfaceMapper.Core.Attrs;
using uzSurfaceMapper.Core.Func;
using uzSurfaceMapper.Extensions;
using uzSurfaceMapper.Model;
using uzSurfaceMapper.Utils.Benchmarks;
using uzSurfaceMapper.Utils.Benchmarks.Impl;

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
        ///     The terrain generator
        /// </summary>
        public GameObject _terrainGenerator;

        /// <summary>
        ///     The loading GIF
        /// </summary>
        public UniGif.GifFile loadingGif;

        /// <summary>
        ///     The hold position
        /// </summary>
        private Vector3 holdPosition;

        /// <summary>
        ///     The character controller
        /// </summary>
        private CharacterController characterController;

        /// <summary>
        ///     The current load progress
        /// </summary>
        private float currentLoadProgress;

        /// <summary>
        ///     The before city loaded
        /// </summary>
        public Func<IEnumerator> beforeCityLoaded;

        /// <summary>
        ///     Occurs when [is city loaded].
        /// </summary>
        public Action<bool> isCityLoaded;

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

            if (!IS_DEMO)
#pragma warning disable 162
                return;
#pragma warning restore 162

            if (singlePlaneSize == -1)
                singlePlaneSize = 12.2f;

            // The first thing we will do, is fill the fields of city class (to allow it to have what it need)
            if (SConv == null)
                SConv = new SceneConversion(realScaleZoom, singlePlaneSize, conversionFactor, new Vector2(mapWidth, mapHeight));

            isCityLoaded = buildCanBeInstanciated =>
            {
                //Debug.Log("City file loaded! Can Build?: " + buildCanBeInstanciated);

                if (!buildCanBeInstanciated)
                {
                    // DO THE MAIN GENETATION: BUY THE ASSET TO SEE THIS CODE.
                    //ThreadedDebug.Log("Starting generation!");

                    //// Disable player
                    //TogglePlayer(false);

                    //// Disable camera
                    //ToggleCamera(false);

                    //// Start to do all generation
                    //this.StartCoroutineAsync(DoAllGeneration(saveTexture));
                }
                else
                {
                    if (forceTerrainGen && !continueWhenFinish)
                        return;

                    ThreadedDebug.Log("Starting building city!");

                    //if (testOneBuild)
                    //    TestOneBuild();
                    StartCoroutine(GenerateCity());
                }
            };

            DoesCityFileExists = File.Exists(CityJSONPath);

            if (DoesCityFileExists)
            {
                ThreadedDebug.Log("Starting loading city from file!");

                if (playerObject != null)
                    holdPosition = playerObject.transform.position;

                StartCoroutine(AsyncReadAllFileText(s =>
                {
                    city = s.Deserialize<City>();
                    Debug.Log($"Deserialized city with {city.BuildingCount} buildings!");

                    IsReady = true;
                }));
            }
            else
            {
                if (characterController != null)
                    characterController.enabled = false;

                isCityLoaded(false);
            }
        }

        /// <summary>
        ///     Asynchronous read all file text.
        /// </summary>
        /// <param name="fin">The fin.</param>
        /// <returns></returns>
        private IEnumerator AsyncReadAllFileText(Action<string> fin)
        {
            if (beforeCityLoaded != null)
                yield return this.StartCoroutineAsync(beforeCityLoaded.Invoke());

            bool isMonoSingleton = GetType().DeclaringType?.FullName?.Contains("MonoSingleton") == true;
            Debug.Log($"Loading gif is null?: {loadingGif == null}; Initialized?: {loadingGif?.IsInitialized}; IsMonoSingleton?: {isMonoSingleton}");

            if (loadingGif != null && loadingGif.IsInitialized && isMonoSingleton)
                yield return new WaitUntil(() => loadingGif.IsReady);

            CityBenchmarkData.StartBenchmark(CityBenchmark.LoadingCity);

            yield return F.AsyncReadFileWithWWW(CityJSONPath, f => currentLoadProgress = f, fin);

            CityBenchmarkData.StopBenchmark(CityBenchmark.LoadingCity);

            isCityLoaded(true);
        }

        internal IEnumerator GenerateCity()
        {
            // If them was disabled, then...

            // ReSharper disable once Unity.NoNullPropagation
            if (Camera.main?.clearFlags == CameraClearFlags.SolidColor)
            {
                // Re-enable player
                TogglePlayer(true);

                // Re-enable camera
                ToggleCamera(true);
            }

            // Starts the benchmark
            CityBenchmarkData.StartBenchmark(CityBenchmark.GenerateCity);

            //// We get the terrain generator script
            //var terraGen = _terrainGenerator.GetComponent<TGen>();

            //// We get the field 'singlePlaneSize' from terraGen script
            //singlePlaneSize = TGen.OriginalPlaneScale;

            ////Debug.Log(singlePlaneSize);

            //// The first thing we will do, is fill the fields of city class (to allow it to have what it need)
            //if (SConv == null)
            //    SConv = new SceneConversion(realScaleZoom, singlePlaneSize, conversionFactor, new Vector2(mapWidth, mapHeight));

            //// Update single plane size depeding on the scale
            //TGen.OriginalPlaneScale = SConv.GetScaleMult(TGen.OriginalPlaneScale, false);

            //// Update instance also
            //SConv.SinglePlaneSize = TGen.OriginalPlaneScale;

            // We will get the number of chunks on each axis
            float xCoord = SConv.GetScaleMult(mapWidth),
                  yCoord = SConv.GetScaleMult(mapHeight);

            // We will get the size of the entire map
            float xMapSize = xCoord * singlePlaneSize,
                yMapSize = yCoord * singlePlaneSize;

            City.mapPlaneSize = new Vector2(xMapSize, yMapSize);
            //City.singlePlaneSize = TGen.OriginalPlaneScale; //SConvert.Instance.GetScaleDiv(terraGen.singlePlaneSize);

            // We activate the terrain generation script
            _terrainGenerator?.SetActive(true);

            // Stops the benchmark
            CityBenchmarkData.StopBenchmark(CityBenchmark.GenerateCity);

            BenchmarkReports.Instance.GetActualReport(BenchmarkReportOrder.All);

            yield break;
        }
    }
}