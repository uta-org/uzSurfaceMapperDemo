namespace uzSurfaceMapper.Model.Enums
{
    /// <summary>
    ///     Position-in-Polygon problem result
    /// </summary>
    public enum PIPResult
    {
        /// <summary>
        ///     The segments are null
        /// </summary>
        IsNull,

        /// <summary>
        ///     The point is inside
        /// </summary>
        IsInside,

        /// <summary>
        ///     The point is outside
        /// </summary>
        IsOutside
    }
}