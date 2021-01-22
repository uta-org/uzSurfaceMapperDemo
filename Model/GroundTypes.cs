using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using uzSurfaceMapper.Core.Attrs.CodeAnalysis;
using uzSurfaceMapper.Model.Enums;
using uzSurfaceMapper.Extensions;
using UnityEngine;
using uzSurfaceMapper.Extensions.Demo;
using Color = uzSurfaceMapper.Model.Color;

//using uzSurfaceMapper.Core.Components;

namespace uzSurfaceMapper.Model
{
    public static class GroundTypes
    {
        [WIP] // There are some ground types missing (see map texture in detail, airports or fences for example)
        private static readonly Dictionary<GroundType, GroundTypeData> mapColors =
            new Dictionary<GroundType, GroundTypeData>
            {
                {
                    GroundType.Building,
                    new GroundTypeData(0, 0, Color.white, GroundType.Building, GroundBehaviour.Building)
                },
                {
                    GroundType.Asphalt,
                    new GroundTypeData(0, 0, Color.black, GroundType.Asphalt, GroundBehaviour.Road)
                },
                {
                    GroundType.Asphalt2,
                    new GroundTypeData(0, 0, new Color(75, 75, 75, 255), GroundType.Asphalt2, GroundBehaviour.Road)
                },
                {
                    GroundType.Asphalt3,
                    new GroundTypeData(0, 0, new Color(18, 19, 19, 255), GroundType.Asphalt3, GroundBehaviour.Road)
                },
                {
                    GroundType.LightPavement1,
                    new GroundTypeData(0, 0, new Color(178, 190, 184, 255), GroundType.LightPavement1,
                        GroundBehaviour.Building)
                },
                {
                    GroundType.LightPavement2,
                    new GroundTypeData(0, 0, new Color(178, 189, 184, 255), GroundType.LightPavement2,
                        GroundBehaviour.Building)
                },
                {
                    GroundType.Pavement,
                    new GroundTypeData(0, 0, new Color(102, 102, 102, 255), GroundType.Pavement,
                        GroundBehaviour.Building)
                },
                {
                    GroundType.Pavement2,
                    new GroundTypeData(0, 0, new Color(77, 81, 79, 255), GroundType.Pavement2,
                        GroundBehaviour.Building)
                },
                {
                    GroundType.Grass1,
                    new GroundTypeData(0, .3f, new Color(56, 130, 39, 255), GroundType.Grass1, true)
                },
                {
                    GroundType.Grass2,
                    new GroundTypeData(0, .3f, new Color(35, 130, 39, 255), GroundType.Grass2, true)
                },
                {
                    GroundType.Grass3,
                    new GroundTypeData(0, .3f, new Color(73, 155, 45, 255), GroundType.Grass3, true)
                },
                {
                    GroundType.DarkGrass1,
                    new GroundTypeData(.1f, .2f, new Color(24, 88, 26, 255), GroundType.DarkGrass1, true)
                },
                {
                    GroundType.DarkGrass2,
                    new GroundTypeData(.1f, .2f, new Color(38, 88, 26, 255), GroundType.DarkGrass2, true)
                },
                {
                    GroundType.DryGrass1,
                    new GroundTypeData(0, .1f, new Color(110, 146, 64, 255), GroundType.DryGrass1, true)
                },
                {
                    GroundType.DryGrass2,
                    new GroundTypeData(0, .1f, new Color(110, 147, 64, 255), GroundType.DryGrass2, true)
                },
                {
                    GroundType.DryGrass3,
                    new GroundTypeData(0, .1f, new Color(109, 149, 122, 255), GroundType.DryGrass3, true)
                },
                {
                    GroundType.DryGrass4,
                    new GroundTypeData(0, .1f, new Color(110, 147, 110, 255), GroundType.DryGrass4, true)
                },
                {
                    GroundType.DryGrass5,
                    new GroundTypeData(0, .1f, new Color(138, 175, 63, 255), GroundType.DryGrass5, true)
                },
                {
                    GroundType.DryGrass6,
                    new GroundTypeData(0, .1f, new Color(55, 63, 49, 255), GroundType.DryGrass6, true)
                },
                {
                    GroundType.HardSand,
                    new GroundTypeData(.2f, .25f, new Color(180, 145, 111, 255), GroundType.HardSand, true)
                },
                {
                    GroundType.Sand,
                    new GroundTypeData(.05f, .1f, new Color(239, 192, 148, 255), GroundType.Sand, true)
                },
                {
                    GroundType.Dirt,
                    new GroundTypeData(0, .4f, new Color(156, 134, 115, 255), GroundType.Dirt, true)
                },
                {
                    GroundType.Mud,
                    new GroundTypeData(0, .5f, new Color(109, 78, 21, 255), GroundType.Mud, GroundBehaviour.Road)
                },
                {
                    GroundType.Rock,
                    new GroundTypeData(.25f, .25f, new Color(153, 153, 153, 255), GroundType.Rock, true)
                },
                {
                    GroundType.Snow,
                    new GroundTypeData(.2f, .1f, new Color(202, 233, 226, 255), GroundType.Snow, true)
                },
                {
                    GroundType.Ice,
                    new GroundTypeData(.25f, .2f, new Color(148, 170, 166, 255), GroundType.Ice, true)
                },
                {
                    GroundType.Water,
                    new GroundTypeData(-.25f, .3f, new Color(109, 150, 188, 255), GroundType.Water, GroundBehaviour.Sea)
                },
                {
                    GroundType.Water2,
                    new GroundTypeData(-.25f, .3f, new Color(102, 139, 173, 255), GroundType.Water2, GroundBehaviour.Sea)
                },
                {
                    GroundType.Rails,
                    new GroundTypeData(0, 0, new Color(128, 3, 4, 255), GroundType.Rails, GroundBehaviour.Road)
                },
                {
                    GroundType.Tunnel,
                    new GroundTypeData(0, 0, new Color(107, 105, 99, 255), GroundType.Tunnel, GroundBehaviour.Road)
                },
                {
                    GroundType.DirtRoad,
                    new GroundTypeData(0, .1f, new Color(128, 64, 4, 255), GroundType.DirtRoad, GroundBehaviour.Road)
                },
                {
                    GroundType.Fence,
                    new GroundTypeData(0, 0, new Color(205, 74, 73, 255), GroundType.Fence, GroundBehaviour.Road)
                }
            };

        /// <summary>
        ///     The ground type map
        /// </summary>
        private static ConcurrentDictionary<Color, GroundType> groundTypeMap;

        /// <summary>
        ///     The ground colors
        /// </summary>
        private static ConcurrentBag<Color> groundColors = new ConcurrentBag<Color>();

        /// <summary>
        ///     Gets the ground type map.
        /// </summary>
        /// <value>
        ///     The ground type map.
        /// </value>
        private static ConcurrentDictionary<Color, GroundType> GroundTypeMap
        {
            get
            {
                if (groundTypeMap == null)
                {
                    //groundTypeMap = mapColors.Values.ToDictionary(x => x.Color, x => x.GroundType);

                    groundTypeMap = new ConcurrentDictionary<Color, GroundType>();

                    foreach (var kv in mapColors)
                    {
                        groundColors.Add(kv.Value.Color);

                        if (!groundTypeMap.TryAdd(kv.Value.Color, kv.Key))
                            throw new Exception();
                    }

                    Debug.Log(string.Join(Environment.NewLine,
                        groundTypeMap.Select(gtm => $"Key: {gtm.Key} | Value: {gtm.Value}")));
                }

                return groundTypeMap;
            }
        }

        /// <summary>
        ///     Gets the ground type data.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        public static GroundTypeData GetGroundTypeData(this Color color, bool force = false)
        {
            try
            {
                return mapColors[GroundTypeMap[color]];
            }
            catch
            {
                if (force)
                {
                    var nearestColor = color.GetSimilarColor(groundColors);
                    return nearestColor.GetGroundTypeData();
                }

                return null;
            }
        }

        /// <summary>
        ///     Gets the ground type data.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static GroundTypeData GetGroundTypeData(this GroundType type)
        {
            return mapColors[type];
        }

        /// <summary>
        ///     Gets the color.
        /// </summary>
        /// <param name="groundType">Type of the ground.</param>
        /// <returns></returns>
        public static Color GetColor(this GroundType groundType)
        {
            return mapColors[groundType].Color;
        }
    }
}