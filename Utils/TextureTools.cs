using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DepotToolkit.CommonCode
{
    #region License and Information

    /* * * * *
     * The "TextureTools" class provides two extension methods for the Texture2D class
     * of Unity. They allow to scale a texture to an arbitrary resolution. It uses
     * bilinear filtering for resampling. To compensate for possible aspect ratio
     * differences there are two different versions:
     *
     *  - ResampleAndCrop will scale the image so the image will always fill the entire
     *    target image. It will center and crop the image to fit into the target area.
     *  - ResampleAndLetterbox will scale the image so the entire image will fit into
     *    the target image. It will center the image and add letterbox space either at
     *    top and bottom or left and right
     *
     *
     * The MIT License (MIT)
     *
     * Copyright (c) 2012-2017 Markus G�bel
     *
     * Permission is hereby granted, free of charge, to any person obtaining a copy
     * of this software and associated documentation files (the "Software"), to deal
     * in the Software without restriction, including without limitation the rights
     * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
     * copies of the Software, and to permit persons to whom the Software is
     * furnished to do so, subject to the following conditions:
     *
     * The above copyright notice and this permission notice shall be included in all
     * copies or substantial portions of the Software.
     *
     * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
     * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
     * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
     * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
     * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
     * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
     * SOFTWARE.
     *
     * * * * */

    #endregion License and Information

    public static class TextureTools
    {
        public static Color DefaultBgColor { get; set; } = new Color(0, 0, 0, .5f);

        [Serializable]
        public class EditableFont
        {
            public Font font;
            public Texture2D fontImage;
        }

        public static Texture2D DuplicateTexture(this Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        public static Texture2D DrawTextInABox(this Texture2D source, string text)
        {
            return DrawTextInABox(source, text, null);
        }

        public static Texture2D DrawTextInABox(this Texture2D source, string text, EditableFont font)
        {
            return DrawTextInABox(source, new Rect(), null, text, font, DefaultBgColor);
        }

        public static Texture2D DrawTextInABox(this Texture2D source, Vector2 offset, RectOffset rectOffset, string text)
        {
            return DrawTextInABox(source, offset, rectOffset, text, null);
        }

        public static Texture2D DrawTextInABox(this Texture2D source, Vector2 offset, string text, EditableFont font)
        {
            return DrawTextInABox(source, offset, null, text, font);
        }

        public static Texture2D DrawTextInABox(this Texture2D source, Vector2 offset, RectOffset rectOffset, string text, EditableFont font)
        {
            return DrawTextInABox(source, offset, new Rect(), rectOffset, text, font, DefaultBgColor);
        }

        public static Texture2D DrawTextInABox(this Texture2D source, Rect rect, RectOffset rectOffset, string text)
        {
            return DrawTextInABox(source, rect, rectOffset, text, DefaultBgColor);
        }

        public static Texture2D DrawTextInABox(this Texture2D source, Rect rect, RectOffset rectOffset, string text, Color bgColor)
        {
            return DrawTextInABox(source, rect, rectOffset, text, null, bgColor);
        }

        public static Texture2D DrawTextInABox(this Texture2D source, Rect rect, RectOffset rectOffset, string text, EditableFont font, Color bgColor)
        {
            return DrawTextInABox(source, new Vector2(), rect, rectOffset, text, font, bgColor);
        }

        public static Texture2D DrawTextInABox(this Texture2D source, Vector2 offset, Rect rect, RectOffset rectOffset, string text, EditableFont editableFont, Color bgColor)
        {
            return DrawTextInABox(source, offset, rect, rectOffset, text, editableFont, 12, bgColor);
        }

        public static Texture2D DrawTextInABox(this Texture2D source, Vector2 offset, Rect rect, RectOffset rectOffset, string text, EditableFont editableFont, int fontSize, Color bgColor)
        {
            if (editableFont == null) throw new ArgumentNullException(nameof(editableFont)); // TODO: load it from Resources

            if (rect == default)
            {
                var size = GUI.skin.label.CalcSize(new GUIContent(text));
                rect = new Rect(rectOffset?.top ?? 0, rectOffset?.left ?? 0,
                    size.x, size.y);
            }

            int tx = (int)(offset.x + rect.xMin);
            int ty = (int)(offset.y + rect.yMin);

            int x = (int)rect.xMin;
            int y = (int)rect.yMin;

            // Draw font
            source.DrawBox(x, y, (int)rect.width, (int)rect.height, bgColor);

            // Draw text
            source.DrawText(editableFont.font, text, tx, ty, fontSize);

            return source;
        }

        public static Texture2D DrawText(this Texture2D source, Font font, string text, int x, int y, int fontSize, float lineSpacing = 0.75f)
        {
            return DrawText(source, font, text, x, y, out _, fontSize, lineSpacing);
        }

        public static Texture2D DrawText(this Texture2D source, Font font, string text, int x, int y, out TextToTexture text2Tex, int fontSize, float lineSpacing = 0.75f)
        {
            text2Tex = new TextToTexture(font, 10, 10, false);
            return text2Tex.CreateTextToTexture(source, text, x, y, fontSize / 38f, lineSpacing);
        }

        public static Texture2D DrawBox(this Texture2D source, int x, int y, int width, int height, Color c)
        {
            if (width < 0 || height < 0 || width >= source.width || height >= source.height) throw new ArgumentOutOfRangeException();
            var colors = new Color[width * height];
            var tColors = source.GetPixels(x, y, width, height);
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.Lerp(tColors[i], c, .5f);
            source.SetPixels(x, y, width, height, colors);
            source.Apply();
            return source;
        }

        public static Texture2D ResampleAndCrop(this Texture2D source, int targetWidth, int targetHeight)
        {
            int sourceWidth = source.width;
            int sourceHeight = source.height;
            float sourceAspect = (float)sourceWidth / sourceHeight;
            float targetAspect = (float)targetWidth / targetHeight;
            int xOffset = 0;
            int yOffset = 0;
            float factor = 1;
            if (sourceAspect > targetAspect)
            { // crop width
                factor = (float)targetHeight / sourceHeight;
                xOffset = (int)((sourceWidth - sourceHeight * targetAspect) * 0.5f);
            }
            else
            { // crop height
                factor = (float)targetWidth / sourceWidth;
                yOffset = (int)((sourceHeight - sourceWidth / targetAspect) * 0.5f);
            }
            Color32[] data = source.GetPixels32();
            Color32[] data2 = new Color32[targetWidth * targetHeight];
            for (int y = 0; y < targetHeight; y++)
            {
                float yPos = y / factor + yOffset;
                int y1 = (int)yPos;
                if (y1 >= sourceHeight)
                {
                    y1 = sourceHeight - 1;
                    yPos = y1;
                }

                int y2 = y1 + 1;
                if (y2 >= sourceHeight)
                    y2 = sourceHeight - 1;
                float fy = yPos - y1;
                y1 *= sourceWidth;
                y2 *= sourceWidth;
                for (int x = 0; x < targetWidth; x++)
                {
                    float xPos = x / factor + xOffset;
                    int x1 = (int)xPos;
                    if (x1 >= sourceWidth)
                    {
                        x1 = sourceWidth - 1;
                        xPos = x1;
                    }
                    int x2 = x1 + 1;
                    if (x2 >= sourceWidth)
                        x2 = sourceWidth - 1;
                    float fx = xPos - x1;
                    var c11 = data[x1 + y1];
                    var c12 = data[x1 + y2];
                    var c21 = data[x2 + y1];
                    var c22 = data[x2 + y2];
                    float f11 = (1 - fx) * (1 - fy);
                    float f12 = (1 - fx) * fy;
                    float f21 = fx * (1 - fy);
                    float f22 = fx * fy;
                    float r = c11.r * f11 + c12.r * f12 + c21.r * f21 + c22.r * f22;
                    float g = c11.g * f11 + c12.g * f12 + c21.g * f21 + c22.g * f22;
                    float b = c11.b * f11 + c12.b * f12 + c21.b * f21 + c22.b * f22;
                    float a = c11.a * f11 + c12.a * f12 + c21.a * f21 + c22.a * f22;
                    int index = x + y * targetWidth;

                    data2[index].r = (byte)r;
                    data2[index].g = (byte)g;
                    data2[index].b = (byte)b;
                    data2[index].a = (byte)a;
                }
            }

            var tex = new Texture2D(targetWidth, targetHeight);
            tex.SetPixels32(data2);
            tex.Apply(true);
            return tex;
        }

        public static Texture2D ResampleAndLetterbox(this Texture2D source, int targetWidth, int targetHeight, Color aBackground)
        {
            int sourceWidth = source.width;
            int sourceHeight = source.height;
            float sourceAspect = (float)sourceWidth / sourceHeight;
            float targetAspect = (float)targetWidth / targetHeight;
            float xOffset = 0;
            float yOffset = 0;
            float factor = 1;
            if (sourceAspect > targetAspect)
            { // letterbox height
                factor = (float)targetWidth / sourceWidth;
                yOffset = (sourceHeight - sourceWidth / targetAspect) * 0.5f;
            }
            else
            { //letterbox width
                factor = (float)targetHeight / sourceHeight;
                xOffset = (sourceWidth - sourceHeight * targetAspect) * 0.5f;
            }
            Color32[] data = source.GetPixels32();
            Color32[] data2 = new Color32[targetWidth * targetHeight];
            Color32 backCol = aBackground;
            for (int y = 0; y < targetHeight; y++)
            {
                float yPos = y / factor + yOffset;
                int y1 = (int)yPos;
                if (y1 >= sourceHeight || y1 < 0)
                {
                    int index = y * targetWidth;
                    for (int x = 0; x < targetWidth; x++)
                    {
                        data2[index + x] = backCol;
                    }
                    continue;
                }
                int y2 = y1 + 1;
                float fy = yPos - y1;
                if (y2 >= sourceHeight)
                    y2 = sourceHeight - 1;
                y1 *= sourceWidth;
                y2 *= sourceWidth;

                for (int x = 0; x < targetWidth; x++)
                {
                    int index = x + y * targetWidth;

                    float xPos = x / factor + xOffset;
                    int x1 = (int)xPos;
                    if (x1 >= sourceWidth || x1 < 0)
                    {
                        data2[index] = backCol;
                        continue;
                    }

                    int x2 = x1 + 1;
                    if (x2 >= sourceWidth)
                        x2 = sourceWidth - 1;

                    float fx = xPos - x1;

                    var c11 = data[x1 + y1];
                    var c12 = data[x1 + y2];
                    var c21 = data[x2 + y1];
                    var c22 = data[x2 + y2];
                    float f11 = (1 - fx) * (1 - fy);
                    float f12 = (1 - fx) * fy;
                    float f21 = fx * (1 - fy);
                    float f22 = fx * fy;
                    float r = c11.r * f11 + c12.r * f12 + c21.r * f21 + c22.r * f22;
                    float g = c11.g * f11 + c12.g * f12 + c21.g * f21 + c22.g * f22;
                    float b = c11.b * f11 + c12.b * f12 + c21.b * f21 + c22.b * f22;
                    float a = c11.a * f11 + c12.a * f12 + c21.a * f21 + c22.a * f22;

                    data2[index].r = (byte)r;
                    data2[index].g = (byte)g;
                    data2[index].b = (byte)b;
                    data2[index].a = (byte)a;
                }
            }

            var tex = new Texture2D(targetWidth, targetHeight);
            tex.SetPixels32(data2);
            tex.Apply(true);
            return tex;
        }
    }
}