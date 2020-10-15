using System;
using uzSurfaceMapper.Core.Generators;
using UnityEngine;
using uzLib.Lite.Core;

namespace uzSurfaceMapper.Core.Func
{
    /// <summary>
    ///     This will contain all the needed conversion to pass from texture units (7000x5000) to scene units (4200x3000)
    /// </summary>
    public class SceneConversion : Singleton<SceneConversion>
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="SceneConversion"/> class from being created.
        /// </summary>
        private SceneConversion()
        {
            Instance = this;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SceneConversion" /> class.
        /// </summary>
        /// <param name="realScaleZoom">The real scale zoom.</param>
        /// <param name="singlePlaneSize">Size of the single plane.</param>
        /// <param name="conversionFactor">The conversion factor.</param>
        /// <param name="mapSize">Size of the map.</param>
        public SceneConversion(float realScaleZoom, float singlePlaneSize, float conversionFactor, Vector2 mapSize)
            : this()
        {
            RealScaleZoom = realScaleZoom;
            SinglePlaneSize = singlePlaneSize;
            ConversionFactor = conversionFactor;
            MapSize = mapSize;

            ParametersSet = true;
        }

        /// <summary>
        ///     Gets or sets the real scale zoom. (By default this must be equal to 6f)
        /// </summary>
        /// <value>
        ///     The real scale zoom.
        /// </value>
        public float RealScaleZoom { get; set; } = -1;

        /// <summary>
        ///     Gets or sets the size of the single plane. (By default this must be equal to 333f)
        /// </summary>
        /// <value>
        ///     The size of the single plane.
        /// </value>
        public float SinglePlaneSize { get; set; } = -1;

        /// <summary>
        /// Gets the zoomed map offset.
        /// </summary>
        /// <value>
        /// The zoomed map offset.
        /// </value>
        public Vector2 ZoomedMapOffset => MapSize * RealScaleZoom / 2;

        /// <summary>
        /// Gets the size of the map.
        /// </summary>
        /// <value>
        /// The size of the map.
        /// </value>
        public Vector2 MapSize { get; }

        /// <summary>
        ///     Gets or sets the conversion factor. (By default this must be equal to 0.1f)
        /// </summary>
        /// <value>
        ///     The conversion factor.
        /// </value>
        public float ConversionFactor { get; }

        public static bool ParametersSet { get; private set; }

        /// <summary>
        ///     Get the passed coordinated multiplied by the scaleZoom * conversionFactor
        /// </summary>
        /// <param name="coord">The coord.</param>
        /// <param name="useSinglePlaneSize">if set to <c>true</c> [use single plane size].</param>
        /// <returns></returns>
        public float GetScaleMult(float coord, bool useSinglePlaneSize = true)
        {
            return coord * RealScaleZoom * ConversionFactor / (useSinglePlaneSize ? SinglePlaneSize : 1);
        }

        /// <summary>
        ///     Get the passed coordinated divided by the scaleZoom * conversionFactor
        /// </summary>
        /// <param name="coord">The coord.</param>
        /// <returns></returns>
        public float GetScaleDiv(float coord)
        {
            return coord / (RealScaleZoom * ConversionFactor); // * ((1f / RealScaleZoom) / ConversionFactor);
        }

        /// <summary>
        /// Gets the real position on map.
        /// </summary>
        /// <param name="texturePosition">The texture position.</param>
        /// <returns></returns>
        public Vector2 GetRealPositionOnMap(Vector3 texturePosition)
        {
            return GetRealPositionOnMap(new Vector2(texturePosition.x, texturePosition.z));
        }

        /// <summary>
        /// Gets the real position on map.
        /// </summary>
        /// <param name="position">The texture position.</param>
        /// <returns></returns>
        public Vector2 GetRealPositionOnMap(Vector2 position)
        {
            return position * RealScaleZoom - ZoomedMapOffset;
        }

        /// <summary>
        ///     Gets the real position for texture.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public Vector2 GetRealPositionForTexture(Vector3 position)
        {
            return GetRealPositionForTexture(new Vector2(position.x, position.z));
        }

        /// <summary>
        ///     Gets the real position for texture.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public Vector2 GetRealPositionForTexture(Vector2 position)
        {
            return (position + ZoomedMapOffset) / RealScaleZoom;
        }

        /// <summary>
        /// Gets the real position for texture.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="isXAxis">if set to <c>true</c> [is x axis].</param>
        /// <returns></returns>
        public int GetRealPositionForTexture(float value, bool isXAxis)
        {
            return Mathf.RoundToInt((value + (isXAxis ? ZoomedMapOffset.x : ZoomedMapOffset.y)) / RealScaleZoom);
        }

        /// <summary>
        /// Gets the scaled vector.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Vector2 GetScaledVector(Vector3 value)
        {
            return new Vector2(GetScaleDiv(value.x), GetScaleDiv(value.z));
        }

        /// <summary>
        /// Gets the scaled vector.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Vector2 GetScaledVector(Vector2 value)
        {
            return new Vector2(GetScaleDiv(value.x), GetScaleDiv(value.y));
        }

        /// <summary>
        /// Gets the scaled vector.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="z">The z.</param>
        /// <returns></returns>
        public Vector2 GetScaledVector(float x, float z)
        {
            return new Vector2(GetScaleDiv(x), GetScaleDiv(z));
        }

        /// <summary>
        ///     Converts the rect.
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <returns></returns>
        public Rect ConvertRect(float x1, float y1, float x2, float y2)
        {
            // return new Rect(GetScaleMult(x1, false), GetScaleMult(y1, false), GetScaleMult(x2, false), GetScaleMult(y2, false));
            return new Rect(GetScaleDiv(x1) + MapGenerator.mapWidth / 2, GetScaleDiv(y1) + MapGenerator.mapHeight / 2,
                GetScaleDiv(x2), GetScaleDiv(y2));
        }

        /// <summary>
        ///     Converts the vector back to world position.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        public Vector2 ConvertVector(Vector2 v, bool useSinglePlaneSize = false)
        {
            return new Vector2(GetScaleMult(v.x, useSinglePlaneSize), GetScaleMult(v.y, useSinglePlaneSize));
        }

        public override string ToString()
        {
            return $"Map Size: {MapSize}\n" +
                   $"Conversion Factor: {ConversionFactor}\n" +
                   $"Zoomed Map Offset: {ZoomedMapOffset}\n" +
                   $"Single Plane size: {SinglePlaneSize}\n" +
                   $"Real Scale Zoom: {RealScaleZoom}";
        }
    }
}