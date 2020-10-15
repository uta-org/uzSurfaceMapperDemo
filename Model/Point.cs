using System;
using uzSurfaceMapper.Model.Enums;
using Newtonsoft.Json;
using UnityEngine;

namespace uzSurfaceMapper.Model
{
    /// <summary>
    ///     Struct Point
    /// </summary>
    [Serializable]
    public struct Point : ICloneable
    {
        /// <summary>
        ///     The x
        /// </summary>
        public int x, y;

        /// <summary>
        ///     Gets the zero.
        /// </summary>
        /// <value>The zero.</value>
        public static Point zero => new Point(0, 0);

        /// <summary>
        ///     Gets the one.
        /// </summary>
        /// <value>
        ///     The one.
        /// </value>
        public static Point one => new Point(1, 1);

        /// <summary>
        ///     Gets down.
        /// </summary>
        /// <value>Down.</value>
        public static Point down => new Point(0, 1);

        /// <summary>
        ///     Gets the left.
        /// </summary>
        /// <value>The left.</value>
        public static Point left => new Point(-1, 0);

        /// <summary>
        ///     Gets the right.
        /// </summary>
        /// <value>The right.</value>
        public static Point right => new Point(1, 0);

        /// <summary>
        ///     Gets up.
        /// </summary>
        /// <value>Up.</value>
        public static Point up => new Point(0, -1);

        /// <summary>
        ///     Gets the upper left.
        /// </summary>
        /// <value>The upper left.</value>
        public static Point upperLeft => new Point(-1, -1);

        /// <summary>
        ///     Gets the upper right.
        /// </summary>
        /// <value>The upper right.</value>
        public static Point upperRight => new Point(1, -1);

        /// <summary>
        ///     Gets down left.
        /// </summary>
        /// <value>Down left.</value>
        public static Point downLeft => new Point(-1, 1);

        /// <summary>
        ///     Gets down right.
        /// </summary>
        /// <value>Down right.</value>
        public static Point downRight => new Point(1, 1);

        /// <summary>
        ///     Gets the name.
        /// </summary>
        /// <value>The name.</value>
        [JsonIgnore]
        public string name => GetName();

        /// <summary>
        ///     Gets the SQR magnitude.
        /// </summary>
        /// <value>The SQR magnitude.</value>
        [JsonIgnore]
        public float sqrMagnitude => x * x + y * y;

        /// <summary>
        ///     Gets the magnitude.
        /// </summary>
        /// <value>The magnitude.</value>
        [JsonIgnore]
        public float magnitude => Mathf.Sqrt(sqrMagnitude);

        /// <summary>
        ///     Gets the normalized.
        /// </summary>
        /// <value>The normalized.</value>
        [JsonIgnore]
        public Point normalized => new Point((int)Mathf.Clamp01(x), (int)Mathf.Clamp01(y));

        /// <summary>
        ///     Initializes a new instance of the <see cref="Point" /> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Point" /> struct.
        /// </summary>
        /// <param name="vector2">The vector2.</param>
        public Point(Vector2 vector2, PointApproximation approximation = PointApproximation.Round)
        {
            switch (approximation)
            {
                case PointApproximation.Round:
                    x = Mathf.RoundToInt(vector2.x);
                    y = Mathf.RoundToInt(vector2.y);
                    break;

                case PointApproximation.Floor:
                    x = Mathf.FloorToInt(vector2.x);
                    y = Mathf.FloorToInt(vector2.y);
                    break;

                case PointApproximation.Ceil:
                    x = Mathf.CeilToInt(vector2.x);
                    y = Mathf.CeilToInt(vector2.y);
                    break;
            }

            x = (int)vector2.x;
            y = (int)vector2.y;
        }

        /// <summary>
        ///     Implements the +.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static Point operator +(Point a, Point b)
        {
            return new Point(a.x + b.x, a.y + b.y);
        }

        /// <summary>
        ///     Implements the -.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static Point operator -(Point a, Point b)
        {
            return new Point(a.x - b.x, a.y - b.y);
        }

        /// <summary>
        ///     Implements the *.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static Point operator *(Point a, Point b)
        {
            return new Point(a.x * b.x, a.y * b.y);
        }

        /// <summary>
        ///     Implements the /.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static Point operator /(Point a, Point b)
        {
            return new Point(a.x / b.x, a.y / b.y);
        }

        /// <summary>
        ///     Implements the *.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static Point operator *(Point a, int b)
        {
            return new Point(a.x * b, a.y * b);
        }

        /// <summary>
        ///     Implements the /.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>The result of the operator.</returns>
        public static Point operator /(Point a, int b)
        {
            return new Point(a.x / b, a.y / b);
        }

        /// <summary>
        ///     Implements the operator *.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static Point operator *(Point a, float b)
        {
            return new Point((int)(a.x * b), (int)(a.y * b));
        }

        /// <summary>
        ///     Implements the operator /.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static Point operator /(Point a, float b)
        {
            return new Point((int)(a.x / b), (int)(a.y / b));
        }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="Point" /> to <see cref="Vector2" />.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Vector2(Point p)
        {
            return new Vector2(p.x, p.y);
        }

        /// <summary>
        ///     Performs an explicit conversion from <see cref="Vector2" /> to <see cref="Point" />.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Point(Vector2 v)
        {
            return new Point((int)v.x, (int)v.y);
        }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="Point" /> to <see cref="Vector2" />.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Vector3(Point p)
        {
            return new Vector3(p.x, p.y);
        }

        /// <summary>
        ///     Performs an explicit conversion from <see cref="Vector2" /> to <see cref="Point" />.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Point(Vector3 v)
        {
            return new Point((int)v.x, (int)v.y);
        }

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("({0}, {1})", x, y);
        }

        /// <summary>
        ///     Implements the ==.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(Point p1, Point p2)
        {
            return p1.x == p2.x && p1.y == p2.y;
        }

        /// <summary>
        ///     Implements the !=.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(Point p1, Point p2)
        {
            return p1.x != p2.x || p1.y != p2.y;
        }

        /// <summary>
        ///     Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var p = (Point)obj;
            return x == p.x && y == p.y;
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
        ///     Gets the name.
        /// </summary>
        /// <returns></returns>
        private string GetName()
        {
            if (this == upperLeft)
                return "upperLeft";
            if (this == up)
                return "up";
            if (this == upperRight)
                return "upperRight";
            if (this == left)
                return "left";
            if (this == right)
                return "right";
            if (this == downLeft)
                return "downLeft";
            if (this == down)
                return "down";
            if (this == downRight)
                return "downRight";
            if (this == zero)
                return "center";
            return "none";
        }

        /// <summary>
        ///     Gets the point.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <returns>Point.</returns>
        public Point GetPoint(Direction dir)
        {
            switch (dir)
            {
                case Direction.up:
                    return up;

                case Direction.upperRight:
                    return upperRight;

                case Direction.right:
                    return right;

                case Direction.downRight:
                    return downRight;

                case Direction.down:
                    return down;

                case Direction.downLeft:
                    return downLeft;

                case Direction.left:
                    return left;

                case Direction.upperLeft:
                    return upperLeft;
            }

            return zero;
        }

        /// <summary>
        ///     Gets the point.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <returns>Point.</returns>
        public Point GetInnerPoint(InnerDirection dir)
        {
            switch (dir)
            {
                case InnerDirection.up:
                    return up;

                case InnerDirection.right:
                    return right;

                case InnerDirection.down:
                    return down;

                case InnerDirection.left:
                    return left;
            }

            return zero;
        }

        /// <summary>
        ///     News the position.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <returns>Point.</returns>
        public Point NewPos(Direction dir)
        {
            return this + GetPoint(dir);
        }

        /// <summary>
        ///     News the position.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <returns>Point.</returns>
        public Point NewInnerPos(InnerDirection dir)
        {
            return this + GetInnerPoint(dir);
        }

        /// <summary>
        ///     Nexts the direction.
        /// </summary>
        /// <param name="lastDirection">The last direction.</param>
        /// <returns></returns>
        public Point NextDirection(ref InnerDirection lastDirection)
        {
            lastDirection = InnerDirections.GetPerpendicular(lastDirection);
            return GetInnerPoint(lastDirection);
        }

        /// <summary>
        ///     Clones this instance.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Tries the parse.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public static bool TryParse(string str, out Point point)
        {
            try
            {
                point = Parse(str);
                return true;
            }
            catch
            {
                point = default;
                return false;
            }
        }

        /// <summary>
        /// Parses the specified string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static Point Parse(string str)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(nameof(str));

            var coords = str.Split(',');
            return new Point(int.Parse(coords[0]), int.Parse(coords[1]));
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public static string ToString(int x, int y)
        {
            return $"{x},{y}";
        }
    }
}