using uzSurfaceMapper.Extensions.Demo;
using UnityEngine;

namespace uzSurfaceMapper.Model
{
    /// <summary>
    ///     Texture Object
    /// </summary>
    public sealed class TextureObject
    {
        /// <summary>
        ///     Prevents a default instance of the <see cref="TextureObject" /> class from being created.
        /// </summary>
        private TextureObject()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TextureObject" /> class.
        /// </summary>
        /// <param name="colors">The colors.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public TextureObject(Color32[] colors, int width, int height)
        {
            Colors = colors;
            Width = width;
            Height = height;
        }

        /// <summary>
        ///     Gets the colors.
        /// </summary>
        /// <value>
        ///     The colors.
        /// </value>
        public Color32[] Colors { get; }

        /// <summary>
        ///     Gets the width.
        /// </summary>
        /// <value>
        ///     The width.
        /// </value>
        public int Width { get; }

        /// <summary>
        ///     Gets the height.
        /// </summary>
        /// <value>
        ///     The height.
        /// </value>
        public int Height { get; }

        /// <summary>
        ///     Gets the length.
        /// </summary>
        /// <value>
        ///     The length.
        /// </value>
        public int Length => Width * Height;

        /// <summary>
        ///     Gets the x.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public int GetX(int index)
        {
            return index % Width;
        }

        /// <summary>
        ///     Gets the y.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public int GetY(int index)
        {
            return index / Width;
        }

        /// <summary>
        ///     Sets the color.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="color">The color.</param>
        public void SetColor(int x, int y, Color32 color)
        {
            Colors[F.P(x, y, Width, Height)] = color;
        }

        /// <summary>
        ///     To the texture.
        /// </summary>
        /// <returns></returns>
        public Texture2D ToTexture()
        {
            var texture = new Texture2D(Width, Height);

            texture.SetPixels32(Colors);
            texture.Apply();

            return texture;
        }
    }
}