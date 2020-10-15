using uzSurfaceMapper.Core.Attrs.CodeAnalysis;
using uzSurfaceMapper.Model;
using UnityEngine;
using UnityEngine.Core;
using UnityEngine.Extensions;
using SConvert = uzSurfaceMapper.Core.Func.SceneConversion;
using Player = UnityStandardAssets.Characters.FirstPerson.FirstPersonController;

//using uzSurfaceMapper.Utils.UI;

namespace uzSurfaceMapper.Core.Generators
{
    /*

        Dev Suggestion: If you have to access a MapGenerator property or field do it static in MapGenerator class due to polymorphism.

         */

    /// <summary>
    ///     Controls everything about generation.
    /// </summary>
    /// <seealso cref="MonoBehaviour" />
    public sealed class MapController : MonoSingleton<MapController>
    {
        #region "Public fields"

        /// <summary>
        ///     The current percecentage
        /// </summary>
        [HideInInspector] public float currentPerc;

        public GameObject IngameDebugConsole;

        /// <summary>
        ///     The NSFW
        /// </summary>
        [Tooltip("In case this is false, then Game View && Scene View tabs are closed.")]
        public bool nsfw;

        /// <summary>
        /// Forces terrain generation.
        /// </summary>
        [Tooltip("Force terrain heightmap generation although city was already generated.\nChange this to false in case you want to test Terrain Generation.")]
        public bool forceTerrainGeneration;

        /// <summary>
        ///     Toggle to show the minimap
        /// </summary>
        public bool showMinimap = true;

        /// <summary>
        ///     The sea pixel
        /// </summary>
        public Color32 seaPixel = new Color32(127, 167, 200, 255);

        /// <summary>
        ///     The nav icon
        /// </summary>
        public Texture2D navIcon;

        /// <summary>
        ///     The minimap size
        /// </summary>
        public Vector2 minimapSize;

        #endregion "Public fields"

        #region "Private fields"

        /// <summary>
        ///     The sea texture
        /// </summary>
        private Texture2D seaTexture;

        /// <summary>
        ///     The sea rect
        /// </summary>
        private Rect seaRect;

        /// <summary>
        ///     The nav rect
        /// </summary>
        private Rect navRect;

        /// <summary>
        ///     The nav point
        /// </summary>
        private Vector2 navPoint;

        #endregion "Private fields"

        #region "Unity methods"

        private void Awake()
        {
            MapGenerator.forceTerrainGen = forceTerrainGeneration;

            if (minimapSize == default)
                minimapSize = new Vector2(256, 256);

            seaTexture = seaPixel.ToTexture();

            seaRect = new Rect(Vector2.zero, minimapSize);

            navRect = new Rect(minimapSize / 2 - Vector2.one * 8, new Vector2(16, -16));

            navPoint = new Vector2(navRect.xMin + navRect.width * 0.5f, navRect.yMin + navRect.height * 0.5f);
        }

        // Use this for initialization
        private void Start()
        {
#if UNITY_EDITOR
            if (IngameDebugConsole != null)
                IngameDebugConsole.SetActive(false);
#endif
        }

        // Update is called once per frame
        private void Update()
        {
            currentPerc = MapGenerator.currentIndex * MapGenerator.updateInterlockedEvery /
                          (float)MapGenerator.totalIndexes;
        }

        [WIP] // This must be done in a different class
        private void OnGUI()
        {
            // showMinimap
            if (!CityGenerator.DoesCityFileExists || MapGenerator.MapTexture == null)
                return;

            // This needs to be here because of screen resolution updates
            var minimapArea = new Rect(new Vector2(Screen.width - minimapSize.x - 5, 5), minimapSize);

            GUILayout.BeginArea(minimapArea);

            GUI.DrawTexture(seaRect, seaTexture);

            if (City.IsMapPlaneSizeSet)
            {
                GUILayout.BeginArea(new Rect(
                    -MapGenerator.mapWidth / 2f,
                    -MapGenerator.mapHeight / 2f,
                    MapGenerator.mapWidth, MapGenerator.mapHeight));

                if (SConvert.ParametersSet)
                    GUI.DrawTexture(new Rect(
                        minimapArea.width / 2 - SConvert.Instance.GetScaleDiv(Player.Pos.x),
                        minimapArea.height / 2 + navRect.height - SConvert.Instance.GetScaleDiv(Player.Pos.z),
                        MapGenerator.mapWidth,
                        MapGenerator.mapHeight), MapGenerator.MapTexture);

                GUILayout.EndArea();

                if (navIcon != null)
                {
                    var matrixBackup = GUI.matrix;
                    {
                        GUIUtility.RotateAroundPivot(-Player.Euler.y, navPoint);
                        GUI.DrawTexture(navRect, navIcon);
                    }
                    GUI.matrix = matrixBackup;
                }
            }

            GUILayout.EndArea();
        }

        #endregion "Unity methods"
    }
}