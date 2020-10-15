using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.Core;
using UnityEngine.Extensions;
using UnityStandardAssets.Characters.FirstPerson;
using uzSurfaceMapper.Core.Attrs;
using uzSurfaceMapper.Core.Attrs.CodeAnalysis;
using uzSurfaceMapper.Core.Workers;
using uzSurfaceMapper.Extensions;
using uzSurfaceMapper.Model;
using uzSurfaceMapper.Utils.Benchmarks.Impl;
using Color = UnityEngine.Color;
using Debug = UnityEngine.Debug;
using F = uzSurfaceMapper.Extensions.F;

// ReSharper disable HeuristicUnreachableCode

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace uzSurfaceMapper.Core.Generators
{
    public abstract partial class MapGenerator : MonoSingleton<MapGenerator>, IInvoke
    {
        protected static readonly Thread mainThread = Thread.CurrentThread;

        #region "Constant fields"

        public const int updateInterlockedEvery = 100000;

        public const string resourcePathConst = "Textures/map";
        public const string debugPathConst = "Textures/debugBuild";

        #endregion "Constant fields"

        #region "Static fields"

        public const bool forceReady = true;

        private static bool isReady;

        public static bool IsReady
        {
            get => (TextureWorkerBase.WorkersCollection.IsNullOrEmpty() ||
                    TextureWorkerBase.WorkersCollection.All(x => x.IsReady)) && isReady || forceReady;
            set => isReady = value;
        }

        /// <summary>
        ///     The current index
        /// </summary>
        public static int currentIndex;

        /// <summary>
        ///     The total indexes
        /// </summary>
        public static int totalIndexes;

        #endregion "Static fields"

        /// <summary>
        ///     The resource path
        /// </summary>
        [HideInInspector] [SerializeField] public string resourcePath = resourcePathConst;

        /// <summary>
        ///     Gets the resource path getter.
        /// </summary>
        /// <value>
        ///     The resource path getter.
        /// </value>
        protected static string resourcePathGetter => string.IsNullOrEmpty(Instance.resourcePath)
            ? !Instance.debugging ? resourcePathConst : debugPathConst
            : Instance.resourcePath;

        /// <summary>
        ///     The width of the current loaded texture
        /// </summary>
        public static int mapWidth = -1;

        /// <summary>
        ///     The height of the current loaded texture
        /// </summary>
        public static int mapHeight = -1;

        /// <summary>
        ///     The camera flags
        /// </summary>
        private CameraClearFlags cameraFlags;

        /// <summary>
        ///     The camera color
        /// </summary>
        private Color cameraColor;

        /// <summary>
        ///     The euler angles
        /// </summary>
        private Vector3 eulerAngles;

        /// <summary>
        ///     Force terrain heightmap generation although city was already generated
        ///     Change this to false in case you want to test Terrain Generation
        /// </summary>
        [WIP] public static bool forceTerrainGen = false;

        /// <summary>
        ///     Debug the current instance?
        /// </summary>
        [HideInInspector] [SerializeField] public bool debugging;

        /// <summary>
        ///     The player object
        /// </summary>
        [HideInInspector] [SerializeField] public GameObject playerObject;

        /// <summary>
        ///     The city
        /// </summary>
        [HideInInspector] public City city;

        /// <summary>
        ///     Continue creating builds when finish map texture iteration
        /// </summary>
        [HideInInspector] [SerializeField] public bool continueWhenFinish;

        public static Texture2D MapTexture { get; private set; }

        /// <summary>
        ///     The map colors
        /// </summary>
        public static Color[] mapColors;

        public static string RoadJSONPath => GetOutputSavePath("road");

        /// <summary>
        ///     Gets a value indicating whether this instance is debugging.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is debugging; otherwise, <c>false</c>.
        /// </value>
        public static bool IsDebugging
        {
            get
            {
                if (Instance == null)
                    return false;

                return Instance.debugging;
            }
        }

        //public ObservableCollection<Action> DelegateFuncs { get; } = new ObservableCollection<Action>();

        //private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    foreach (var item in e.NewItems)
        //    {
        //        var action = (Action)item;
        //        action?.Invoke();
        //    }
        //}

        private static bool AlreadyExecuted { get; set; }

        [InvokeAtAwake]
        public virtual void InvokeAtAwake()
        {
            if (!IS_DEMO)
#pragma warning disable 162
                return;
#pragma warning restore 162

            if (AlreadyExecuted)
                return;

            AlreadyExecuted = true;

            TextureBenchmarkData.StartBenchmark(TextureBenchmark.ResourcesLoad);

            // We will need to load it because of minimap
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (!debugging && (MapController.Instance.showMinimap || forceTerrainGen))
            {
                // TODO: Implement async
                MapTexture = Resources.Load<Texture2D>(resourcePathGetter);

                mapWidth = MapTexture.width;
                mapHeight = MapTexture.height;

                // TODO: Implement async read
                mapColors = MapTexture.GetPixels();

                TextureWorkerBase.SetReference(mapColors);

                //AsyncHelper.RunAsync(
                //    () =>
                //    {
                //        // TODO: Implement here all calls
                //        VoronoiTextureWorker.PrepareVoronoiCells();
                //    }); // ,
                //() => IsReady = true);

                // @TODO: Implement offset polygon view here

                //if (testGridPerfomance)
                //{
                //    for (int i = 1; i <= 5; i++)
                //    {
                //        var _ = mapColors.GridCheck(new Point(mapWidth / 2, mapHeight / 2), mapWidth, mapHeight, i).ToArray();
                //    }
                //}

                // TODO: This is working, but not needed on Demo. Remove and refactor.
                //if (testVoronoi)
                //{
                //    VoronoiWorker = AsyncHelper.RunAsync(
                //        () => DoVoronoi(mapColors, mapWidth, mapHeight, false, false, false), result =>
                //        {
                //            var debugStr = result.Item1;

                //            if (result.Item2 == null)
                //            {
                //                Debug.LogError($"{nameof(VoronoiWorker)} cancelled!");
                //                Debug.Log(debugStr);
                //                return;
                //            }

                //            Debug.Log(debugStr);

                //            var watch = Stopwatch.StartNew();
                //            var map = result.Item2.CastBack().ToArray();
                //            var filePath = Path.Combine(Environment.CurrentDirectory, "voronoi-test.png");

                //            F.SaveAndClear(filePath, map, mapWidth, mapHeight);
                //            watch.Stop();

                //            Debug.Log($"Done in {watch.ElapsedMilliseconds} ms!");
                //        });
                //    VoronoiWorker.WorkerSupportsCancellation = true;
                //}
            }

            TextureBenchmarkData.StopBenchmark(TextureBenchmark.ResourcesLoad);
        }

        /// <summary>
        ///     Toggles the player.
        /// </summary>
        /// <param name="active">if set to <c>true</c> [active].</param>
        protected void TogglePlayer(bool active)
        {
            playerObject.GetComponent<CharacterController>().enabled = active;
            playerObject.GetComponent<AudioSource>().enabled = active;
            playerObject.GetComponent<FirstPersonController>().enabled = active;
        }

        /// <summary>
        ///     Toggles the camera.
        /// </summary>
        /// <param name="active">if set to <c>true</c> [active].</param>
        protected void ToggleCamera(bool active)
        {
            if (!active)
            {
                cameraFlags = Camera.main.clearFlags;
                cameraColor = Camera.main.backgroundColor;
                eulerAngles = Camera.main.transform.eulerAngles;
            }

            Camera.main.clearFlags = active ? cameraFlags : CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = active ? cameraColor : UnityEngine.Color.black;
            Camera.main.transform.eulerAngles = active ? eulerAngles : Vector3.left * 90;
        }

        /// <summary>
        ///     Gets the save path.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <returns></returns>
        protected static string GetOutputSavePath(string name = "city", bool isDemo = IS_DEMO)
        {
            string file = Path.Combine(GetStreamingAssetsPath(), isDemo ? "ExampleData" : "Generated Output", $"{name}.json"),
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

#if IS_DEMO
        protected const bool IS_DEMO = true;
#else
        protected const bool IS_DEMO = false;
#endif
    }
}