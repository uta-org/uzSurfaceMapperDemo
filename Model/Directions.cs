using uzSurfaceMapper.Model.Enums;

namespace uzSurfaceMapper.Model
{
    /// <summary>
    ///     Class Directions.
    /// </summary>
    public static class Directions
    {
        private const int dirLength = 8;

        /// <summary>
        ///     Gets the perpendicular.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <returns>Direction.</returns>
        public static Direction GetPerpendicular(Direction dir)
        {
            return GetDir(dir, 2);
        }

        /// <summary>
        ///     Gets the clockwise.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <returns>Direction.</returns>
        public static Direction GetClockwise(Direction dir)
        {
            return GetDir(dir, 1);
        }

        /// <summary>
        ///     Gets the counter clockwise.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <returns>Direction.</returns>
        public static Direction GetCounterClockwise(Direction dir)
        {
            return GetDir(dir, -1);
        }

        /// <summary>
        ///     Gets the dir.
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="n">The n.</param>
        /// <returns></returns>
        private static Direction GetDir(Direction dir, int n)
        {
            var v = (int)dir + n;

            if (v < 0)
                v += dirLength;
            else if (v >= dirLength)
                v -= dirLength;

            return (Direction)v;
        }
    }
}