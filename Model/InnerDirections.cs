using uzSurfaceMapper.Model.Enums;
using uzSurfaceMapper.Extensions.Demo;

namespace uzSurfaceMapper.Model
{
    /// <summary>
    ///     Class Directions.
    /// </summary>
    public static class InnerDirections
    {
        private const int dirLength = 4;

        /// <summary>
        ///     Gets the perpendicular.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <returns>Direction.</returns>
        public static InnerDirection GetPerpendicular(this InnerDirection dir, bool inverse = false)
        {
            // left ==> up
            // down ==> left ...
            return GetDirection(dir, inverse ? -1 : 1);
        }

        /// <summary>
        ///     Gets the inverse.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="inverse">if set to <c>true</c> [inverse].</param>
        /// <returns></returns>
        public static InnerDirection GetInverse(this InnerDirection dir, bool inverse = false)
        {
            // down ==> up
            // right ==> left
            return GetDirection(dir, inverse ? -2 : 2);
        }

        /// <summary>
        ///     Gets the dir.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="n">The n.</param>
        /// <returns></returns>
        private static InnerDirection GetDirection(this InnerDirection dir, int n)
        {
            var v = (int)dir + n;

            if (v < 0)
                v += dirLength;
            else if (v >= dirLength)
                v -= dirLength;

            return (InnerDirection)v;
        }

        public static InnerDirection GetDirection(this Point p1, Point p2)
        {
            int dx = (p1.x - p2.x).ToOne();
            int dy = (p1.y - p2.y).ToOne();

            if (dx < 0)
                return InnerDirection.left;

            if (dx > 0)
                return InnerDirection.right;

            if (dy < 0)
                return InnerDirection.down;

            if (dy > 0)
                return InnerDirection.up;

            return default;
        }
    }
}