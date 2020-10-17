using System;
using System.Collections;
using APIScripts.Utils;
using CielaSpike;
using UnityEngine;
using UnityEngine.Extensions;
using UnityEngine.UI;
using UnityGif;
using UnityStandardAssets.Characters.FirstPerson;
using uzSurfaceMapper.Core.Attrs;
using uzSurfaceMapper.Core.Func;
using uzSurfaceMapper.Extensions;
using uzSurfaceMapper.Model;
using uzSurfaceMapper.Utils.Benchmarks;
using uzSurfaceMapper.Utils.Benchmarks.Impl;
using F = uzSurfaceMapper.Extensions.F;
using UColor = UnityEngine.Color;

#if UNITY_WEBGL

using File = uzSurfaceMapperDemo.Utils.File;

#endif

namespace uzSurfaceMapper.Core.Generators
{
    public sealed partial class CityGenerator : MapGenerator
    {
        /// <summary>
        ///     The real scale zoom (if the map texture is 7000x5000 this will generate a plane of 42kmx30km)
        ///     Default: 4 (old: 6)
        /// </summary>
        public float realScaleZoom = 4;

        /// <summary>
        ///     The conversion factor
        /// </summary>
        public float conversionFactor = 1f; // This must be 1

        /// <summary>
        ///     The single plane size
        ///     Default: 12.2 (old 33.3)
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
        ///     The character controller
        /// </summary>
        private CharacterController characterController; // TODO: remove...

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

        protected static string CityBINPath => GetOutputSavePath("city", "bin");

        private float Progress { get; set; }

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

                    //ThreadedDebug.Log("Starting building city!");  TODO: ThreadedDebug is incompatible
                    Debug.Log("Starting building city!");

                    //if (testOneBuild)
                    //    TestOneBuild();
                    StartCoroutine(GenerateCity());
                }
            };

            string path;

#if !UNITY_WEBGL
            path = CityJSONPath;
#else
            path = CityBINPath;
#endif

            DoesCityFileExists = File.Exists(path);

            characterController = FindObjectOfType<CharacterController>();

            if (DoesCityFileExists)
            {
                //ThreadedDebug.Log("Starting loading city from file!");
                Debug.Log("Starting loading city from file!");

                //if (playerObject != null)
                holdPosition = FirstPersonController.Pos;
                //holdPosition.y = 100;
                OnGenerationFinished += () =>
                {
                    Debug.Log($"Set player position on generation finished! {holdPosition} -> {FirstPersonController.Pos}");
                    FirstPersonController.Pos = holdPosition;
                };

                // playerObject.transform.position;

#if !UNITY_WEBGL
                StartCoroutine(AsyncReadAllFileText(s =>
                {
                    city = s.Deserialize<City>();
                    Debug.Log($"Deserialized city with {city.BuildingCount} buildings!");

                    isCityReady = true;
                }));
#else
                var url = WebRequestUtils.MakeInitialUrl(path);
                Debug.Log($"City: '{path}' -> '{url}'");
                url.ReadDataFromWebAsync(result =>
                {
                    Func<City> cityAsync = () => F.Deserialize<City>(result, evnt =>
                    {
                        Progress = evnt.Progress;
                    });
                    AsyncHelper.RunAsync(cityAsync, cityResult =>
                    {
                        city = cityResult;
                        Debug.Log($"Deserialized city with {city.BuildingCount} buildings!");
                        isCityReady = true;
                    });
                });

                //StartCoroutine(F.AsyncReadFileWithWWW<byte[]>(path, s =>
                //{
                //    city = F.Deserialize<City>(s);
                //    Debug.Log($"Deserialized city with {city.BuildingCount} buildings!");

                //    isCityReady = true;
                //}));
#endif
            }
            else
            {
                if (characterController != null)
                    characterController.enabled = false;

                isCityLoaded(false);
            }

#if UNITY_WEBGL
            //Debug.Log(CityBINPath);
            //Debug.Log($"Exists: {File.Exists(CityBINPath)}\n" +
            //          $"City exists: {DoesCityFileExists}\n" +
            //          $"if: {!File.Exists(CityBINPath) && DoesCityFileExists}");
            if (!File.Exists(CityBINPath) && DoesCityFileExists)
            {
                Debug.Log("Started coroutine!");
                StartCoroutine(SerializeBin());
            }
#endif
        }

        private void OnGUI()
        {
            if (isCityReady)
                return;

            UIUtils.DrawBar(CityProgressRect, Progress, UColor.white, UColor.gray, 1);
            GUI.Label(CityProgressRect, $"City progress: {Progress * 100:F2} %", LabelStyle);
        }

        protected override IEnumerator SerializeBin()
        {
            Debug.Log($"Waiting city to be deserialized in order to serialize to '{CityBINPath}'...");
            yield return new WaitUntil(() => isCityReady);
            Debug.Log($"Serializing '{CityBINPath}'!");

            // ReSharper disable once InvokeAsExtensionMethod
            File.WriteAllBytes(CityBINPath, F.Serialize(city, null));
        }

        [InvokeAtUpdate]
        public override void InvokeAtUpdate()
        {
            base.InvokeAtUpdate();

            if (DoesCityFileExists && !IsReady && characterController != null)
                FirstPersonController.Pos = holdPosition;
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