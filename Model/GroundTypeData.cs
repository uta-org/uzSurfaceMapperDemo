using uzSurfaceMapper.Model;
using uzSurfaceMapper.Model.Enums;

namespace uzSurfaceMapper.Model
{
    /// <summary>
    ///     Ground Type Data
    /// </summary>
    public class GroundTypeData
    {
        /// <summary>
        ///     Prevents a default instance of the <see cref="GroundTypeData" /> class from being created.
        /// </summary>
        private GroundTypeData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroundTypeData" /> class.
        /// </summary>
        /// <param name="meanHeight">Height of the mean.</param>
        /// <param name="amplitude">The amplitude.</param>
        /// <param name="color">The color.</param>
        /// <param name="groundType">Type of the ground.</param>
        /// <param name="isAllowed">if set to <c>true</c> [is allowed].</param>
        public GroundTypeData(float meanHeight, float amplitude, Color color, GroundType groundType,
            bool isAllowed = false)
        {
            MeanHeight = meanHeight;
            Amplitude = amplitude;
            Color = color;
            GroundType = groundType;
            IsAllowed = isAllowed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroundTypeData"/> class.
        /// </summary>
        /// <param name="meanHeight">Height of the mean.</param>
        /// <param name="amplitude">The amplitude.</param>
        /// <param name="color">The color.</param>
        /// <param name="groundType">Type of the ground.</param>
        /// <param name="behaviour">The behaviour.</param>
        /// <param name="isAllowed">if set to <c>true</c> [is allowed].</param>
        public GroundTypeData(float meanHeight, float amplitude, Color color, GroundType groundType, GroundBehaviour behaviour,
            bool isAllowed = false)
        {
            MeanHeight = meanHeight;
            Amplitude = amplitude;
            Color = color;
            GroundType = groundType;
            Behaviour = behaviour;
            IsAllowed = isAllowed;
        }

        /// <summary>
        ///     Gets the height of the mean.
        /// </summary>
        /// <value>
        ///     The height of the mean.
        /// </value>
        public float MeanHeight { get; }

        /// <summary>
        ///     Gets the amplitude.
        /// </summary>
        /// <value>
        ///     The amplitude.
        /// </value>
        public float Amplitude { get; }

        /// <summary>
        ///     Gets the color.
        /// </summary>
        /// <value>
        ///     The color.
        /// </value>
        public Color Color { get; }

        /// <summary>
        ///     Gets the type of the ground.
        /// </summary>
        /// <value>
        ///     The type of the ground.
        /// </value>
        public GroundType GroundType { get; }

        /// <summary>
        /// Gets the behaviour.
        /// </summary>
        /// <value>
        /// The behaviour.
        /// </value>
        public GroundBehaviour Behaviour { get; }

        /// <summary>
        ///     Gets a value indicating whether this instance is allowed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is allowed; otherwise, <c>false</c>.
        /// </value>
        public bool IsAllowed { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is allowed special case.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is allowed special case; otherwise, <c>false</c>.
        /// </value>
        public bool IsAllowedSpecialCase => IsAllowed || Behaviour == GroundBehaviour.Sea;

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Type: {GroundType}; IsAllowed?: {IsAllowed}";
        }
    }
}