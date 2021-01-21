#if UNITY_2020 || UNITY_2019 || UNITY_2018 || UNITY_2017 || UNITY_5

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

//using uzLib.Lite.Extensions;

namespace UnityEngine.Extensions
{
    /// <summary>
    /// The MathHelper class
    /// </summary>
    public static class MathHelper
    {
        /// <summary>Get the multiples the of.</summary>
        /// <param name="value">The value.</param>
        /// <param name="multipleOf">The multiple to round off.</param>
        /// <returns></returns>
        public static float MultipleOf(this float value, float multipleOf)
        {
            return Mathf.Round(value / multipleOf) * multipleOf;
        }
    }
}

#endif