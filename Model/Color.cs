using System;
using System.Collections.Generic;
using UnityEngine;

namespace uzSurfaceMapper.Model
{
    /// <summary>
    ///     Struct Color
    /// </summary>
    /// <seealso cref="ICloneable" />
    [Serializable]
    public struct Color : ICloneable, IEquatable<Color>
    {
        /// <summary>
        ///     Clones this instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        ///     The r
        /// </summary>
        public byte r, g, b, a;

        /// <summary>
        ///     Gets the white.
        /// </summary>
        /// <value>The white.</value>
        public static Color white => new Color(255, 255, 255);

        /// <summary>
        ///     Gets the red.
        /// </summary>
        /// <value>The red.</value>
        public static Color red => new Color(255, 0, 0);

        /// <summary>
        ///     Gets the green.
        /// </summary>
        /// <value>The green.</value>
        public static Color green => new Color(0, 255, 0);

        /// <summary>
        ///     Gets the blue.
        /// </summary>
        /// <value>The blue.</value>
        public static Color blue => new Color(0, 0, 255);

        /// <summary>
        ///     Gets the yellow.
        /// </summary>
        /// <value>The yellow.</value>
        public static Color yellow => new Color(255, 255, 0);

        /// <summary>
        ///     Gets the gray.
        /// </summary>
        /// <value>The gray.</value>
        public static Color gray => new Color(128, 128, 128);

        /// <summary>
        ///     Gets the black.
        /// </summary>
        /// <value>The black.</value>
        public static Color black => new Color(0, 0, 0);

        /// <summary>
        ///     Gets the transparent.
        /// </summary>
        /// <value>The transparent.</value>
        public static Color transparent => new Color(0, 0, 0, 0);

        /// <summary>
        /// Gets the purple.
        /// </summary>
        /// <value>
        /// The purple.
        /// </value>
        public static Color purple => new Color(255, 0, 255, 255);

        public static Color cyan => new Color(0, 255, 255, 255);
        public static Color orange => new Color(255, 128, 0, 255);

        /// <summary>
        ///     Initializes a new instance of the <see cref="Color" /> struct.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        public Color(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            a = byte.MaxValue;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Color" /> struct.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <param name="a">a.</param>
        public Color(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        /// <summary>
        ///     Implements the ==.
        /// </summary>
        /// <param name="c1">The c1.</param>
        /// <param name="c2">The c2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Color c1, Color c2)
        {
            return c1.r == c2.r && c1.g == c2.g && c1.b == c2.b && c1.a == c2.a;
        }

        /// <summary>
        ///     Implements the !=.
        /// </summary>
        /// <param name="c1">The c1.</param>
        /// <param name="c2">The c2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Color c1, Color c2)
        {
            return !(c1 == c2);
        }

        /// <summary>
        ///     Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        // Thanks to: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type#example-1
        public bool Equals(Color other)
        {
            return this == other;
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        ///     Implements the -.
        /// </summary>
        /// <param name="c1">The c1.</param>
        /// <param name="c2">The c2.</param>
        /// <returns>The result of the operator.</returns>
        public static Color operator -(Color c1, Color c2)
        {
            return new Color(
                (byte)Mathf.Clamp(c1.r - c2.r, 0, 255),
                (byte)Mathf.Clamp(c2.g - c2.g, 0, 255),
                (byte)Mathf.Clamp(c2.b - c2.b, 0, 255));
        }

        /// <summary>
        ///     Implements the +.
        /// </summary>
        /// <param name="c1">The c1.</param>
        /// <param name="c2">The c2.</param>
        /// <returns>The result of the operator.</returns>
        public static Color operator +(Color c1, Color c2)
        {
            return new Color(
                (byte)Mathf.Clamp(c1.r + c2.r, 0, 255),
                (byte)Mathf.Clamp(c2.g + c2.g, 0, 255),
                (byte)Mathf.Clamp(c2.b + c2.b, 0, 255));
        }

        /// <summary>
        ///     Lerps the specified c2.
        /// </summary>
        /// <param name="c2">The c2.</param>
        /// <param name="t">The t.</param>
        /// <returns>Color.</returns>
        public Color Lerp(Color c2, float t)
        {
            return new Color(
                (byte)Mathf.Lerp(r, c2.r, t),
                (byte)Mathf.Lerp(g, c2.g, t),
                (byte)Mathf.Lerp(b, c2.b, t));
        }

        /// <summary>
        ///     Lerps the specified c2.
        /// </summary>
        /// <param name="c1">The c1.</param>
        /// <param name="c2">The c2.</param>
        /// <param name="t">The t.</param>
        /// <returns>
        ///     Color.
        /// </returns>
        public static Color Lerp(Color c1, Color c2, float t)
        {
            return new Color(
                (byte)Mathf.Lerp(c1.r, c2.r, t),
                (byte)Mathf.Lerp(c1.g, c2.g, t),
                (byte)Mathf.Lerp(c1.b, c2.b, t));
        }

        /// <summary>
        ///     Inverts this instance.
        /// </summary>
        /// <returns>Color.</returns>
        public Color Invert()
        {
            return new Color(
                (byte)Mathf.Clamp(byte.MaxValue - r, 0, 255),
                (byte)Mathf.Clamp(byte.MaxValue - g, 0, 255),
                (byte)Mathf.Clamp(byte.MaxValue - b, 0, 255));
        }

        // This conversion operator must be explicit to avoid memory leaks on implementations

        /// <summary>
        ///     Performs an explicit conversion from <see cref="UnityEngine.Color" /> to <see cref="Color" />.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Color(UnityEngine.Color c)
        {
            return new Color((byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255), (byte)(c.a * 255));
        }

        /// <summary>
        ///     Performs an explicit conversion from <see cref="Color" /> to <see cref="UnityEngine.Color" />.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator UnityEngine.Color(Color c)
        {
            return new UnityEngine.Color(c.r / 255f, c.g / 255f, c.b / 255f, c.a / 255f);
        }

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            if (this == white)
                return "white";
            if (this == transparent)
                return "transparent";
            if (this == red)
                return "red";
            if (this == blue)
                return "blue";
            if (this == black)
                return "black";
            if (this == green)
                return "green";
            if (this == yellow)
                return "yellow";

            return $"({r}, {g}, {b}, {a})";
        }

        /// <summary>
        ///     Fills the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>Color[].</returns>
        public static IEnumerable<Color> Fill(int x, int y)
        {
            for (var i = 0; i < x * y; ++i)
                yield return black;
        }
    }
}