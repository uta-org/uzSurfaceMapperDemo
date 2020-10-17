using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using APIScripts.Utils;
using uzSurfaceMapper.Core.Attrs.CodeAnalysis;
using uzSurfaceMapper.Model;
using uzSurfaceMapper.Model.Enums;
using uzSurfaceMapper.Utils.Threading;
using MetadataExtractor;
using MetadataExtractor.Formats.Png;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Extensions;
using UnityEngine.Networking;

//using uzSurfaceMapper.Utils.Meshs;
//using uzSurfaceMapper.Utils.Noises;
//using uzSurfaceMapper.Utils.Textures;

using static uzSurfaceMapper.Core.Generators.MapGenerator;
using UEColor = UnityEngine.Color;
using Color = uzSurfaceMapper.Model.Color;
using Debug = UnityEngine.Debug;
using Directory = MetadataExtractor.Directory;
using Object = UnityEngine.Object;
using Random = System.Random;
using SConvert = uzSurfaceMapper.Core.Func.SceneConversion;
using Task = System.Threading.Tasks.Task;

#if UNITY_WEBGL

using File = uzSurfaceMapperDemo.Utils.File;

#endif

namespace uzSurfaceMapper.Extensions
{
    public static class F
    {
        ///// <summary>
        /////     Gets the separator.
        ///// </summary>
        ///// <param name="l">The l.</param>
        ///// <returns></returns>
        //public static string GetSeparator(int l = 30)
        //{
        //    return new string('-', l);
        //}

        /// <summary>
        ///     Gets the name of the generic type.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        public static string GetGenericTypeName(this Type x)
        {
            try
            {
                return x.GetGenericArguments()[0].FullName;
            }
            catch
            {
                return x.FullName;
            }
        }

        /// <summary>
        ///     To the color.
        /// </summary>
        /// <param name="lerp">The lerp.</param>
        /// <returns></returns>
        public static Color ToColor(float lerp) //[0..1]
        {
            if (lerp < 0 || lerp > 1)
                lerp = Mathf.Clamp01(lerp);

            var rgb = (int)(lerp * Mathf.Pow(2, 24));

            return new Color(Convert.ToByte((rgb >> 16) & 0xff),
                Convert.ToByte((rgb >> 8) & 0xff),
                Convert.ToByte((rgb >> 0) & 0xff),
                byte.MaxValue);
        }

        /// <summary>
        ///     Saves the data.
        /// </summary>
        /// <param name="FileName">Name of the file.</param>
        /// <param name="Data">The data.</param>
        /// <returns></returns>
        public static bool SaveData(string FileName, byte[] Data)
        {
            BinaryWriter Writer = null;

            try
            {
                if (!File.Exists(FileName))
                    File.Create(FileName).Dispose();

                // Create a new stream to write to the file
                Writer = new BinaryWriter(File.OpenWrite(FileName));

                // Writer raw data
                Writer.Write(Data);
                Writer.Flush();
                Writer.Close();
            }
            catch
            {
                //...
                return false;
            }

            return true;
        }

        /// <summary>
        ///     To the vec.
        /// </summary>
        /// <param name="pts">The PTS.</param>
        /// <returns></returns>
        public static IEnumerable<Vector2> ToVec(this IEnumerable<Point> pts)
        {
            return pts.Select(x => (Vector2)x);
        }

        /// <summary>
        ///     Gets the determinant.
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <returns></returns>
        private static float GetDeterminant(float x1, float y1, float x2, float y2)
        {
            return x1 * y2 - x2 * y1;
        }

        /// <summary>
        ///     Gets the area.
        /// </summary>
        /// <param name="ps">The ps.</param>
        /// <returns></returns>
        public static float GetArea(this IEnumerable<Point> ps)
        {
            var vertices = ps.ToArray();

            if (vertices.Length < 3)
                return 0;

            var area = GetDeterminant(vertices[vertices.Length - 1].x, vertices[vertices.Length - 1].y, vertices[0].x,
                vertices[0].y);

            for (var i = 1; i < vertices.Length; i++)
                area += GetDeterminant(vertices[i - 1].x, vertices[i - 1].y, vertices[i].x, vertices[i].y);

            return Mathf.Abs(area / 2);
        }

        /// <summary>
        ///     Compares the colors.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public static float CompareColors(UEColor a, UEColor b)
        {
            return 100 * (
                       1.0f - (
                           Math.Abs(a.r - b.r) +
                           Math.Abs(a.g - b.g) +
                           Math.Abs(a.b - b.b)
                       ) / (256.0f * 3)
                   );
        }

        /// <summary>
        ///     Sets the parent.
        /// </summary>
        /// <param name="gameObject">The game object.</param>
        /// <param name="object">The object.</param>
        public static void SetParent(this GameObject gameObject, GameObject @object)
        {
            gameObject.transform.parent = @object.transform;
        }

        /// <summary>
        ///     Determines whether [is null or empty].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <returns>
        ///     <c>true</c> if [is null or empty] [the specified list]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return list == null || list != null && list.Count == 0;
        }

        /// <summary>
        ///     Determines whether [is null or empty].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <returns>
        ///     <c>true</c> if [is null or empty] [the specified list]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return list == null || list != null && list.Count == 0;
        }

        /// <summary>
        ///     Determines whether [is null or empty].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="col">The col.</param>
        /// <returns>
        ///     <c>true</c> if [is null or empty] [the specified col]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty<T>(this ICollection<T> col)
        {
            return col == null || col != null && col.Count == 0;
        }

        /// <summary>
        ///     Determines whether [is null or empty].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr">The arr.</param>
        /// <returns>
        ///     <c>true</c> if [is null or empty] [the specified arr]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty<T>(this T[] arr)
        {
            return arr == null || arr != null && arr.Length == 0;
        }

        /// <summary>
        ///     Determines whether [is null or empty].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <returns>
        ///     <c>true</c> if [is null or empty] [the specified list]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            return list == null || list != null && list.Count() == 0;
        }

        /// <summary>
        ///     Concats the specified element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> list, T element)
        {
            return list.Concat(new[] { element });
        }

        /// <summary>
        ///     Inserts the specified index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static IEnumerable<T> Insert<T>(this IEnumerable<T> enumerable, int index, T value)
        {
            return enumerable.SelectMany((x, i) => index == i ? new[] { value, x } : new[] { x });
        }

        /// <summary>
        ///     Removes the specified index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static IEnumerable<T> Remove<T>(this IEnumerable<T> enumerable, int index)
        {
            return enumerable.Where((x, i) => index != i);
        }

        /// <summary>
        ///     Removes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static IEnumerable<T> Remove<T>(this IEnumerable<T> enumerable, T value)
        {
            return enumerable.Where(x => !x.Equals(value));
        }

        /// <summary>
        ///     Clones the specified list to clone.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listToClone">The list to clone.</param>
        /// <returns></returns>
        public static IEnumerable<T> Clone<T>(this IEnumerable<T> listToClone)
            where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone());
        }

        // Never use this with Insert extension of IEnumerable<T>, is a very expensive method
        /// <summary>
        ///     ps the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="w">The w.</param>
        /// <param name="h">The h.</param>
        /// <returns></returns>
        public static int P(int x, int y, int w, int h)
        {
            // (h - y - 1)
            // y
            return x + (h - y - 1) * w;

            // Pl: x + (h - y - 1) * w;
        }

        /// <summary>
        /// ps the safe.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="w">The w.</param>
        /// <param name="h">The h.</param>
        /// <returns></returns>
        public static int PSafe(int x, int y, int w, int h)
        {
            if (x < 0)
                x = 0;
            else if (x >= w)
                x = w - 1;

            if (y < 0)
                y = 0;
            else if (y >= h)
                y = h - 1;

            return x + (h - y - 1) * w;
        }

        /// <summary>
        /// ps the safe.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="w">The w.</param>
        /// <param name="h">The h.</param>
        /// <param name="isOut">if set to <c>true</c> [is out].</param>
        /// <returns></returns>
        public static int PSafe(int x, int y, int w, int h, out bool isOut)
        {
            isOut = x < 0 || x >= w || y < 0 || y >= h;

            if (!isOut)
                return x + (h - y - 1) * w;

            return -1;
        }

        public static int PnSafe(int x, int y, int w, int h)
        {
            return PnSafe(x, y, w, h, out _);
        }

        public static int PnSafe(int x, int y, int w, int h, out bool isOut)
        {
            isOut = x < 0 || x >= w || y < 0 || y >= h;

            if (!isOut)
                return x + y * w;

            return -1;
        }

        /// <summary>
        ///     Pns the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="w">The w.</param>
        /// <returns></returns>
        public static int Pn(int x, int y, int w)
        {
            return x + y * w;
        }

        public static Point nP(int i, int w)
        {
            return new Point(i % w, i / w);
        }

        public static Point nP(int i, int w, int h)
        {
            return new Point(i % w, h - i / w - 1);
        }

        /// <summary>
        ///     Pls the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="w">The w.</param>
        /// <param name="h">The h.</param>
        /// <returns></returns>
        public static long Pl(long x, long y, long w, long h)
        {
            return x + (h - y - 1) * w;
        }

        /// <summary>
        ///     Pfs the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="w">The w.</param>
        /// <returns></returns>
        public static float Pf(float x, float y, float w)
        {
            return x + y * w;
        }

        /// <summary>
        ///     Sets the color of the static.
        /// </summary>
        /// <param name="colorMap">The color map.</param>
        /// <param name="ps">The ps.</param>
        /// <param name="c">The c.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        public static HashSet<Color> SetStaticColor(this Color[] colorMap, IEnumerable<Point> ps, Color c, int width,
            int height)
        {
            foreach (var p in ps)
                colorMap[P(p.x, p.y, width, height)] = Color.red;

            return (HashSet<Color>)colorMap.AsEnumerable();
        }

        /// <summary>
        ///     Casts from.
        /// </summary>
        /// <param name="color32">The color32.</param>
        /// <returns></returns>
        public static IEnumerable<Color> CastFrom(this NativeArray<Color32> color32)
        {
            return color32.Select(x => (Color)(UEColor)x);
        }

        /// <summary>
        ///     Casts from.
        /// </summary>
        /// <param name="color32">The color32.</param>
        /// <returns></returns>
        public static IEnumerable<Color> CastFrom(this Color32[] color32)
        {
            return color32.Select(x => (Color)(UEColor)x);
        }

        /// <summary>
        ///     Casts the back.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        public static IEnumerable<Color32> CastBack(this Color[] color)
        {
            return color.Select(x => (Color32)(UEColor)x);
        }

        /// <summary>
        ///     Casts the color from.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        public static Color CastColorFrom(this Color32 color)
        {
            return (Color)(UEColor)color;
        }

        /// <summary>
        ///     Casts the color back.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        public static Color32 CastColorBack(this Color color)
        {
            return (UEColor)color;
        }

        /// <summary>
        ///     Runs the specified working dir.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="workingDir">The working dir.</param>
        /// <param name="fin">The fin.</param>
        /// <param name="arguments">The arguments.</param>
        public static void Run(this string fileName,
            string workingDir = null, Action fin = null, params string[] arguments)
        {
            using (var p = new Process())
            {
                var args = p.StartInfo;

                args.FileName = fileName;
                args.CreateNoWindow = true;

                if (workingDir != null) args.WorkingDirectory = workingDir;

                if (arguments != null && arguments.Any())
                    args.Arguments = string.Join(" ", arguments).Trim();
                else if (fileName.ToLowerInvariant() == "explorer")
                    args.Arguments = args.WorkingDirectory;

                p.Start();
                p.WaitForExit();
            }

            fin?.Invoke();
        }

        /// <summary>
        ///     Determines whether this instance is symbolic.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///     <c>true</c> if the specified path is symbolic; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSymbolic(this string path)
        {
            var pathInfo = new FileInfo(path);
            return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }

        /// <summary>
        ///     To the arr string.
        /// </summary>
        /// <param name="arr">The arr.</param>
        /// <returns></returns>
        public static string ToArrString(this string[] arr)
        {
            return string.Join(", ", arr);
        }

        /// <summary>
        ///     Gets the neighbors.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        public static IEnumerable<T> GetNeighbors<T>(this T[] source, long x, long y, long width, long height)
        {
            for (long i = -1; i <= 1; ++i)
                for (long j = -1; j <= 1; ++j)
                {
                    if (i == 0 && j == 0) continue;
                    yield return source[Pl(x + i, y + j, width, height)];
                }
        }

        /// <summary>
        ///     Gets the first neighbor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr">The arr.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        public static T GetFirstNeighbor<T>(this IList<T> arr, long x, long y, long width, long height)
        {
            for (long i = -1; i <= 1; ++i)
                for (long j = -1; j <= 1; ++j)
                {
                    if (i == 0 && j == 0) continue;

                    try
                    {
                        var e = arr[(int)Pl(x + i, y + j, width, height)];
                        if (e != null)
                            return e;
                    }
                    catch
                    {
                        return default;
                    }
                }

            return default;
        }

        /// <summary>
        ///     Alls the same.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public static bool AllSame<T>(params T[] list)
        {
            return list.AllSame();
        }

        /// <summary>
        ///     Alls the same.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public static bool AllSame<T>(this IEnumerable<T> list)
        {
            var first = true;
            var comparand = default(T);

            foreach (var i in list)
            {
                if (first) comparand = i;
                else if (!i.Equals(comparand)) return false;
                first = false;
            }

            return true;
        }

        /// <summary>
        ///     Invokes the specified method name.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="args">The arguments.</param>
        public static void Invoke(this Component component, string methodName, params object[] args)
        {
            var method = component.GetType().GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method != null)
                method.Invoke(component, args);
        }

        /// <summary>
        ///     Invokes the exception safe.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="args">The arguments.</param>
        public static void InvokeExceptionSafe(this Component component, string methodName, params object[] args)
        {
            try
            {
                component.Invoke(methodName, args);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        ///     Sends the type of the message to objects of.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msg">The MSG.</param>
        /// <param name="args">The arguments.</param>
        public static void SendMessageToObjectsOfType<T>(string msg, params object[] args) where T : Component
        {
            var objects = Object.FindObjectsOfType<T>();

            foreach (var obj in objects)
                obj.InvokeExceptionSafe(msg, args);
        }

        /// <summary>
        ///     Gets the or add component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go">The go.</param>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            var comp = go.GetComponent<T>();

            if (null == comp)
                comp = go.AddComponent<T>();

            return comp;
        }

        /// <summary>
        ///     Debugs the exceptions.
        /// </summary>
        /// <param name="exceptions">The exceptions.</param>
        public static void DebugExceptions(this ConcurrentQueue<Exception> exceptions)
        {
            try
            {
                if (exceptions != null && exceptions.Count > 0)
                    foreach (var innerException in exceptions.Distinct())
                        Debug.LogException(innerException);
            }
            catch
            {
            }
        }

        /// <summary>
        ///     Floods the fill.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="target">The target.</param>
        /// <param name="replacement">The replacement.</param>
        public static void FloodFill<T>(this T[] source, int x, int y, int width, int height, T target, T replacement)
            where T : IEquatable<T>
        {
            source.FloodFill(x, y, width, height, target, replacement, out var _, null);
        }

        /// <summary>
        ///     Floods the array following Flood Fill algorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="target">The target to replace.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="i">The i.</param>
        /// <param name="fillCallback">The fill callback.</param>
        // This was generic
        public static void FloodFill<T>(this T[] source, int x, int y, int width, int height, T target, T replacement,
            out int i)
            where T : IEquatable<T>
        {
            FloodFill(source, x, y, width, height, target, replacement, out i, null);
        }

        /// <summary>
        ///     Floods the array following Flood Fill algorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="target">The target to replace.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="i">The i.</param>
        /// <param name="fillCallback">The fill callback.</param>
        // This was generic
        public static void FloodFill<T>(this T[] source, int x, int y, int width, int height, T target, T replacement, Action<int> fillCallback)
            where T : IEquatable<T>
        {
            FloodFill(source, x, y, width, height, target, replacement, out _, fillCallback);
        }

        /// <summary>
        ///     Floods the array following Flood Fill algorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="target">The target to replace.</param>
        /// <param name="replacement">The replacement.</param>
        /// <param name="i">The i.</param>
        /// <param name="fillCallback">The fill callback.</param>
        // This was generic
        public static void FloodFill<T>(this T[] source, int x, int y, int width, int height, T target, T replacement,
            out int i, Action<int> fillCallback)
            where T : IEquatable<T>
        {
            i = 0;

            var queue = new HashSet<int>
            {
                P(x, y, width, height)
            };

            do
            {
                // source index, source x, source y
                int si = queue.First(),
                    sx = si % width,
                    sy = si / width;

                queue.Remove(si);

                if (source[si].Equals(target))
                {
                    source[si] = replacement;

                    if (sx + 1 < width && source[Pn(sx + 1, sy, width)].Equals(target))
                        queue.Add(Pn(sx + 1, sy, width));

                    if (sx - 1 >= 0 && source[Pn(sx - 1, sy, width)].Equals(target))
                        queue.Add(Pn(sx - 1, sy, width));

                    if (sy + 1 < height && source[Pn(sx, sy + 1, width)].Equals(target))
                        queue.Add(Pn(sx, sy + 1, width));

                    if (sy - 1 >= 0 && source[Pn(sx, sy - 1, width)].Equals(target))
                        queue.Add(Pn(sx, sy - 1, width));
                }

                fillCallback?.Invoke(si);

                ++i;
            } while (queue.Count > 0);
        }

        /// <summary>
        ///     Adds the or set.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void AddOrSet<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, value);
            else
                dictionary[key] = value;
        }

        /// <summary>
        ///     To the concurrent dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return new ConcurrentDictionary<TKey, TValue>
                (source.ToDictionary(pair => pair.Key, pair => pair.Value));
        }

        /// <summary>
        ///     Gets the funcs.
        /// </summary>
        /// <param name="waitUntil">The wait until.</param>
        /// <returns></returns>
        public static Func<int, bool>[] GetFuncs(params Func<int, bool>[] waitUntil)
        {
            return waitUntil;
        }

        /// <summary>
        ///     Determines whether [is in bounds] [the specified index].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashset">The hashset.</param>
        /// <param name="index">The index.</param>
        /// <returns>
        ///     <c>true</c> if [is in bounds] [the specified index]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInBounds<T>(this HashSet<T> hashset, int index)
        {
            return index >= 0 && index < hashset.Count;
        }

        /// <summary>
        /// Determines whether [is in bounds] [the specified index].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array.</param>
        /// <param name="index">The index.</param>
        /// <returns>
        ///   <c>true</c> if [is in bounds] [the specified index]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInBounds<T>(this T[] array, int index)
        {
            return index >= 0 && index < array.Length;
        }

        /// <summary>
        /// Determines whether [is in bounds] [the specified index].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="index">The index.</param>
        /// <returns>
        ///   <c>true</c> if [is in bounds] [the specified index]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInBounds<T>(this List<T> list, int index)
        {
            return index >= 0 && index < list.Count;
        }

        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> source)
        {
            foreach (var item in source)
                set.Add(item);
        }

        /// <summary>
        ///     Base64s the encode.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns></returns>
        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        ///     Base64s the decode.
        /// </summary>
        /// <param name="base64EncodedData">The base64 encoded data.</param>
        /// <returns></returns>
        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        /// <summary>
        ///     Draws the line.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="c">The c.</param>
        public static void DrawLine(this Texture2D texture, Point p1, Point p2, Color c)
        {
            DrawLine(texture, p1.x, p1.y, p2.x, p2.y, c);
        }

        /// <summary>
        ///     Draws the line.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="x0">The x0.</param>
        /// <param name="y0">The y0.</param>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="c">The c.</param>
        public static void DrawLine(this Texture2D texture, int x0, int y0, int x1, int y1, Color c)
        {
            int sx = 0,
                sy = 0;

            int dx = Mathf.Abs(x1 - x0),
                dy = Mathf.Abs(y1 - y0);

            if (x0 < x1)
                sx = 1;
            else
                sx = -1;
            if (y0 < y1)
                sy = 1;
            else
                sy = -1;

            int err = dx - dy,
                e2 = 0;

            while (true)
            {
                texture.SetPixel(x0, y0, c.AsUnityColor());

                if (x0 == x1 && y0 == y1)
                    break;

                e2 = 2 * err;

                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                }

                if (e2 < dx)
                {
                    err = err + dx;
                    y0 = y0 + sy;
                }
            }

            texture.Apply();
        }

        public static bool DrawLine<T>(this T[] source, Point p1, Point p2, Func<int, int, bool> predicate)
        {
            return DrawLine(source, p1.x, p1.y, p2.x, p2.y, predicate);
        }

        public static bool DrawLine<T>(this T[] source, int x0, int y0, int x1, int y1, Func<int, int, bool> predicate)
        {
            int sx, sy;

            int dx = Mathf.Abs(x1 - x0),
                dy = Mathf.Abs(y1 - y0);

            if (x0 < x1)
                sx = 1;
            else
                sx = -1;
            if (y0 < y1)
                sy = 1;
            else
                sy = -1;

            int err = dx - dy,
                e2;

            while (true)
            {
                if (!predicate(x0, x1))
                    return false;

                if (x0 == x1 && y0 == y1)
                    break;

                e2 = 2 * err;

                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                }

                if (e2 < dx)
                {
                    err = err + dx;
                    y0 = y0 + sy;
                }
            }

            return true;
        }

        /// <summary>
        ///     Draws the line.
        /// </summary>
        /// <param name="colors">The colors.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="c">The c.</param>
        public static void DrawLine(this Color[] colors, Point p1, Point p2, int width, int height, Color c)
        {
            DrawLine(colors, p1.x, p1.y, p2.x, p2.y, width, height, c);
        }

        /// <summary>
        ///     Draws the line.
        /// </summary>
        /// <param name="colors">The colors.</param>
        /// <param name="x0">The x0.</param>
        /// <param name="y0">The y0.</param>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="c">The c.</param>
        public static void DrawLine(this Color[] colors, int x0, int y0, int x1, int y1, int width, int height, Color c)
        {
            int sx = 0,
                sy = 0;

            int dx = Mathf.Abs(x1 - x0),
                dy = Mathf.Abs(y1 - y0);

            if (x0 < x1)
                sx = 1;
            else
                sx = -1;
            if (y0 < y1)
                sy = 1;
            else
                sy = -1;

            int err = dx - dy,
                e2 = 0;

            while (true)
            {
                colors[P(x0, y0, width, height)] = c;

                if (x0 == x1 && y0 == y1)
                    break;

                e2 = 2 * err;

                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                }

                if (e2 < dx)
                {
                    err = err + dx;
                    y0 = y0 + sy;
                }
            }
        }

        /// <summary>
        /// Gets the pixels on line.
        /// </summary>
        /// <param name="x0">The x0.</param>
        /// <param name="y0">The y0.</param>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <returns></returns>
        public static IEnumerable<Point> GetPixelsOnLine(int x0, int y0, int x1, int y1)
        {
            int sx = 0,
                sy = 0;

            int dx = Mathf.Abs(x1 - x0),
                dy = Mathf.Abs(y1 - y0);

            if (x0 < x1)
                sx = 1;
            else
                sx = -1;
            if (y0 < y1)
                sy = 1;
            else
                sy = -1;

            int err = dx - dy,
                e2 = 0;

            while (true)
            {
                // colors[P(x0, y0, width, height)] = c;
                yield return new Point(x0, y0);

                if (x0 == x1 && y0 == y1)
                    break;

                e2 = 2 * err;

                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                }

                if (e2 < dx)
                {
                    err = err + dx;
                    y0 = y0 + sy;
                }
            }
        }

        /// <summary>
        ///     Draws the line.
        /// </summary>
        /// <param name="colors">The colors.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="c">The c.</param>
        public static void DrawLine(this Color32[] colors, Point p1, Point p2, int width, int height,
            UEColor c)
        {
            DrawLine(colors, p1.x, p1.y, p2.x, p2.y, width, height, c);
        }

        /// <summary>
        ///     Draws the line.
        /// </summary>
        /// <param name="colors">The colors.</param>
        /// <param name="x0">The x0.</param>
        /// <param name="y0">The y0.</param>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="c">The c.</param>
        public static void DrawLine(this Color32[] colors, int x0, int y0, int x1, int y1, int width, int height,
            UEColor c)
        {
            int sx = 0,
                sy = 0;

            int dx = Mathf.Abs(x1 - x0),
                dy = Mathf.Abs(y1 - y0);

            if (x0 < x1)
                sx = 1;
            else
                sx = -1;
            if (y0 < y1)
                sy = 1;
            else
                sy = -1;

            int err = dx - dy,
                e2 = 0;

            while (true)
            {
                colors[P(x0, y0, width, height)] = c;

                if (x0 == x1 && y0 == y1)
                    break;

                e2 = 2 * err;

                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                }

                if (e2 < dx)
                {
                    err = err + dx;
                    y0 = y0 + sy;
                }
            }
        }

        /// <summary>
        ///     Converts the z.
        /// </summary>
        /// <param name="p">The v.</param>
        /// <returns></returns>
        public static Vector3 ConvertZ(this Point p)
        {
            return new Vector3(p.x, 0, p.y);
        }

        /// <summary>
        ///     Converts the z.
        /// </summary>
        /// <param name="p">The v.</param>
        /// <returns></returns>
        public static Vector3 ConvertZ(this Vector2 v)
        {
            return new Vector3(v.x, 0, v.y);
        }

        /// <summary>
        ///     Sets the position between2 vertices.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        public static void SetPositionBetween2Points(this Transform transform, Point p1, Point p2)
        {
            Vector3 vE = SConvert.Instance.ConvertVector(p1).ConvertZ(),
                vPE = SConvert.Instance.ConvertVector(p2).ConvertZ();

            transform.position = vE + (vPE - vE) / 2;
            transform.rotation = Quaternion.FromToRotation(Vector3.right, vPE - vE);
            transform.localScale = new Vector3((vE - vPE).magnitude, 1, .2f);
        }

        /// <summary>
        ///     Sets the scale yz.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        public static void SetScaleYZ(this Transform transform, float y, float z = .2f)
        {
            var x = transform.localScale.x;

            transform.localScale = new Vector3(x, y, z);
        }

        /// <summary>
        ///     Gets the height of the build.
        /// </summary>
        /// <param name="animationCurve">The animation curve.</param>
        /// <param name="weight">The weight.</param>
        /// <returns></returns>
        public static float GetBuildHeight(this AnimationCurve animationCurve, float weight)
        {
            if (animationCurve == null)
                return 0;

            float xMin = animationCurve.keys.First().time,
                xMax = animationCurve.keys.Last().time;

            if (weight < xMin || weight > xMax)
                return 3;

            return animationCurve.Evaluate(weight);
        }

#if !UNITY_WEBGL
        /// <summary>
        ///     Percentiles the specified percentile.
        /// </summary>
        /// <param name="seq">The seq.</param>
        /// <param name="percentile">The percentile.</param>
        /// <returns></returns>
        public static float Percentile(this IEnumerable<dynamic> seq, float percentile)
        {
            var elements = seq.ToArray();
            Array.Sort(elements);

            var realIndex = percentile * (elements.Length - 1);
            var index = (int)realIndex;
            var frac = realIndex - index;

            if (index + 1 < elements.Length)
                return elements[index] * (1 - frac) + elements[index + 1] * frac;
            return elements[index];
        }

        /// <summary>
        ///     Ases the dynamic.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts">The ts.</param>
        /// <returns></returns>
        public static IEnumerable<dynamic> AsDynamic<T>(this IEnumerable<T> ts)
        {
            foreach (var t in ts)
                yield return t;
        }
#endif

        // TODO: This not needed anymore. This implementation was used to created a building on the old 2018 way.
        ///// <summary>
        /////     Creates the floor.
        ///// </summary>
        ///// <param name="bd">The bd.</param>
        ///// <param name="parent">The parent.</param>
        ///// <param name="name">The name.</param>
        ///// <returns></returns>
        //public static GameObject CreateBase(this Building bd, Transform parent, Material mat, string name = "Floor")
        //{
        //    var vs = bd.Pol.Edges.Select(x => SConvert.Instance.ConvertVector(x)).ToArray();

        //    var mesh = vs.ExtrudeMesh();

        //    // This must be done when the building is completely generated
        //    var gameObject = GenerateGameObjectWithBasicComponents(name, mesh, mat, parent);

        //    return gameObject;
        //}

        ///// <summary>
        ///// Generates the game object with basic components.
        ///// </summary>
        ///// <param name="name">The name.</param>
        ///// <param name="mesh">The mesh.</param>
        ///// <param name="mat">The mat.</param>
        ///// <param name="parent">The parent.</param>
        ///// <returns></returns>
        //public static GameObject GenerateGameObjectWithBasicComponents(string name, Mesh mesh, Material mat,
        //    Transform parent = null)
        //{
        //    var gameObject = new GameObject(name);

        //    gameObject = gameObject
        //        .GetOrAddComponentThen<MeshFilter>()
        //        .ModifyComponent(filter => { filter.mesh = mesh; })
        //        .GetOrAddComponentThen<MeshRenderer>()
        //        .ModifyComponent(renderer => { renderer.sharedMaterial = mat; })
        //        .GetOrAddComponentThen<MeshCollider>()
        //        .ModifyComponent(col => { col.sharedMesh = mesh; });

        //    if (parent != null)
        //        gameObject.transform.parent = parent;

        //    return gameObject;
        //}

        /// <summary>
        ///     Gets the or add component then.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go">The go.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     go
        ///     or
        ///     component
        /// </exception>
        public static T GetOrAddComponentThen<T>(this GameObject go)
            where T : Component
        {
            if (go == null)
                throw new ArgumentNullException("go");

            return go.GetOrAddComponent<T>();
        }

        /// <summary>
        ///     Modifies the component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component">The component.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     component
        ///     or
        ///     action
        /// </exception>
        public static GameObject ModifyComponent<T>(this T component, Action<T> action)
            where T : Component
        {
            if (component == null)
                throw new ArgumentNullException("component");

            if (action == null)
                throw new ArgumentNullException("action");

            action.Invoke(component);

            return component.gameObject;
        }

        /// <summary>
        ///     Fors the each.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">The source.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> src, Action<T> action)
        {
            return src.Select(i =>
            {
                action(i);
                return i;
            });
        }

        /// <summary>
        ///     Fors the each.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">The source.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> src, Action<T, int> action)
        {
            return src.Select((e, i) =>
            {
                action(e, i);
                return e;
            });
        }

#if UNITY_EDITOR

        /// <summary>
        ///     Draws the string.
        /// </summary>
        /// <param name="worldPos">The world position.</param>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        public static void DrawString(Vector3 worldPos, string text, Color? color = null)
        {
            Handles.BeginGUI();
            if (color.HasValue) GUI.color = color.Value.AsUnityColor();
            var view = SceneView.currentDrawingSceneView;
            var screenPos = view.camera.WorldToScreenPoint(worldPos);
            var size = GUI.skin.label.CalcSize(new GUIContent(text));
            GUI.Label(new Rect(screenPos.x - size.x / 2, -screenPos.y + view.position.height + 4, size.x, size.y),
                text);
            Handles.EndGUI();
        }

#endif

        /// <summary>
        ///     Files the read asynchronous task.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="updatePerc">The update perc.</param>
        /// <param name="finishedReading">The finished reading.</param>
        /// <returns></returns>
        public static Task FileReadAsyncTask(string path, Action<float> updatePerc, Action<string> finishedReading)
        {
            /* Replace Task.Factory with ThreadPool when using .NET <= 3.5
             *
             * ThreadPool.QueueUserWorkItem(state =>
             *
             * */

            var task = Task.Run(() =>
            {
                var fileInfo = new FileInfo(path);
                var contents = "";

                float length = fileInfo.Length;
                var currentLength = 0;

                using (var sr = new StreamReader(path))
                {
                    while (!sr.EndOfStream)
                    {
                        var str = sr.ReadLine();
                        contents += str;

                        // yield return str;

                        //Call on main Thread
                        UnityThread.executeInUpdate(() => { updatePerc(currentLength / length); });

                        currentLength += str.Length;
                        //Interlocked.Add(ref currentLength, str.Length);
                    }

                    //Call on main Thread
                    UnityThread.executeInUpdate(() => { finishedReading(contents); });
                }
            });

            return task;
        }

        /// <summary>
        ///     Files the read asynchronous.
        /// </summary>
        /// <param name="pathOrUrl">The path or URL.</param>
        /// <param name="updatePerc">The update perc.</param>
        /// <param name="finishedReading">The finished reading.</param>
        /// <returns></returns>
        public static IEnumerator FileReadAsync(string pathOrUrl, Action<float> updatePerc,
            Action<string> finishedReading)
        {
            var contents = "";

            var task = FileReadAsyncTask(pathOrUrl, updatePerc, s => contents = s);

            while (!task.IsCompleted)
                yield return null;

            finishedReading?.Invoke(contents);
        }

        // Thanks to: https://stackoverflow.com/questions/41296957/wait-while-file-load-in-unity
        // Thanks to: https://stackoverflow.com/a/34378847/3286975

        /// <summary>
        ///     Asynchronouses the read file with WWW.
        /// </summary>
        /// <param name="pathOrUrl">The path or URL.</param>
        /// <param name="updatePerc">The update perc.</param>
        /// <param name="finishedReading">The finished reading.</param>
        /// <returns></returns>
        [MustBeReviewed]
        public static IEnumerator AsyncReadFileWithWWW<T>(string pathOrUrl)
        {
            return AsyncReadFileWithWWW<T>(pathOrUrl, null);
        }

        /// <summary>
        ///     Asynchronouses the read file with WWW.
        /// </summary>
        /// <param name="pathOrUrl">The path or URL.</param>
        /// <param name="updatePerc">The update perc.</param>
        /// <param name="finishedReading">The finished reading.</param>
        /// <returns></returns>
        [MustBeReviewed]
        public static IEnumerator AsyncReadFileWithWWW<T>(string pathOrUrl,
            Action<T> finishedReading)
        {
            return AsyncReadFileWithWWW(pathOrUrl, null, finishedReading);
        }

        /// <summary>
        ///     Asynchronouses the read file with WWW.
        /// </summary>
        /// <param name="pathOrUrl">The path or URL.</param>
        /// <param name="updatePerc">The update perc.</param>
        /// <param name="finishedReading">The finished reading.</param>
        /// <returns></returns>
        [MustBeReviewed]
        public static IEnumerator AsyncReadFileWithWWW<T>(string pathOrUrl, Action<float> updatePerc,
            Action<T> finishedReading)
        {
            var fileInfo = new FileInfo(pathOrUrl);

#if !UNITY_WEBGL
            float length = fileInfo.Length;

            // Application.isEditor && ??? // Must review
            if (Path.IsPathRooted(pathOrUrl))
                pathOrUrl = "file:///" + pathOrUrl;
#else
            float length = 1;
#endif

            using (var www = new UnityWebRequest(pathOrUrl))
            {
                www.downloadHandler = new DownloadHandlerBuffer();

                www.SendWebRequest();

                while (!www.isDone)
                {
                    updatePerc?.Invoke(www.downloadedBytes / length); // currentLength / length
                    yield return null;
                }

                if (www.isHttpError || www.isNetworkError)
                {
                    Debug.Log("Error while downloading data: " + www.error);
                    finishedReading(default);
                }
                else
                    finishedReading((T)(typeof(T) == typeof(string) ? (object)www.downloadHandler.text : www.downloadHandler.data));
            }
        }

        /// <summary>
        ///     Gets the size of the image.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static Vector2 GetImageSize(string path)
        {
            IEnumerable<Directory> directories = ImageMetadataReader.ReadMetadata(path);

            var directory = directories.OfType<PngDirectory>().FirstOrDefault();

            var width = int.Parse(directory?.Tags.FirstOrDefault(x => x.Name == "Image Width").Description);
            var height = int.Parse(directory?.Tags.FirstOrDefault(x => x.Name == "Image Height").Description);

            return new Vector2(width, height);
        }

        /// <summary>
        ///     Copies the specified source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">source</exception>
        public static List<T> Copy<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return new List<T>(source);
        }

        // Thanks to: http://orbcreation.com/orbcreation/page.orb?1172

        /// <summary>
        ///     Combines the meshes.
        /// </summary>
        /// <param name="aGo">a go.</param>
        /// <returns></returns>
        public static Mesh CombineMeshes(this GameObject aGo)
        {
            var meshRenderers = aGo.GetComponentsInChildren<MeshRenderer>(false);
            var totalVertexCount = 0;
            var totalMeshCount = 0;

            if (meshRenderers != null && meshRenderers.Length > 0)
                foreach (var meshRenderer in meshRenderers)
                {
                    var filter = meshRenderer.gameObject.GetComponent<MeshFilter>();

                    if (filter != null && filter.sharedMesh != null)
                    {
                        totalVertexCount += filter.sharedMesh.vertexCount;
                        totalMeshCount++;
                    }
                }

            if (totalMeshCount == 0)
            {
                Debug.Log("No meshes found in children. There's nothing to combine.");
                return null;
            }

            if (totalMeshCount == 1)
            {
                Debug.Log("Only 1 mesh found in children. There's nothing to combine.");
                return null;
            }

            if (totalVertexCount > 65535)
            {
                Debug.Log("There are too many vertices to combine into 1 mesh (" + totalVertexCount +
                          "). The max. limit is 65535");
                return null;
            }

            var mesh = new Mesh();
            var myTransform = aGo.transform.worldToLocalMatrix;
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uv1s = new List<Vector2>();
            var uv2s = new List<Vector2>();
            var subMeshes = new Dictionary<Material, List<int>>();

            if (meshRenderers != null && meshRenderers.Length > 0)
                foreach (var meshRenderer in meshRenderers)
                {
                    var filter = meshRenderer.gameObject.GetComponent<MeshFilter>();

                    if (filter != null && filter.sharedMesh != null)
                    {
                        MergeMeshInto(filter.sharedMesh, meshRenderer.sharedMaterials,
                            myTransform * filter.transform.localToWorldMatrix, vertices, normals, uv1s, uv2s,
                            subMeshes);

                        if (filter.gameObject != aGo)
                            filter.gameObject.SetActive(false);
                    }
                }

            mesh.vertices = vertices.ToArray();

            if (normals.Count > 0)
                mesh.normals = normals.Fill(mesh.vertices.Length).ToArray();

            if (uv1s.Count > 0)
                mesh.uv = uv1s.Fill(mesh.vertices.Length).ToArray();

            if (uv2s.Count > 0)
                mesh.uv2 = uv2s.Fill(mesh.vertices.Length).ToArray();

            mesh.subMeshCount = subMeshes.Keys.Count;

            var materials = new Material[subMeshes.Keys.Count];
            var mIdx = 0;
            foreach (var m in subMeshes.Keys)
            {
                materials[mIdx] = m;
                mesh.SetTriangles(subMeshes[m].ToArray(), mIdx++);
            }

            if (meshRenderers != null && meshRenderers.Length > 0)
            {
                var meshRend = aGo.GetComponent<MeshRenderer>();

                if (meshRend == null)
                    meshRend = aGo.AddComponent<MeshRenderer>();

                meshRend.sharedMaterials = materials;

                var meshFilter = aGo.GetComponent<MeshFilter>();

                if (meshFilter == null)
                    meshFilter = aGo.AddComponent<MeshFilter>();

                meshFilter.sharedMesh = mesh;
            }

            return mesh;
        }

        /// <summary>
        ///     Merges the mesh into.
        /// </summary>
        /// <param name="meshToMerge">The mesh to merge.</param>
        /// <param name="ms">The ms.</param>
        /// <param name="transformMatrix">The transform matrix.</param>
        /// <param name="vertices">The vertices.</param>
        /// <param name="normals">The normals.</param>
        /// <param name="uv1s">The uv1s.</param>
        /// <param name="uv2s">The uv2s.</param>
        /// <param name="subMeshes">The sub meshes.</param>
        private static void MergeMeshInto(Mesh meshToMerge, Material[] ms, Matrix4x4 transformMatrix,
            List<Vector3> vertices, List<Vector3> normals, List<Vector2> uv1s, List<Vector2> uv2s,
            Dictionary<Material, List<int>> subMeshes)
        {
            if (meshToMerge == null)
                return;

            var vertexOffset = vertices.Count;
            var vs = meshToMerge.vertices;

            for (var i = 0; i < vs.Length; i++)
                vs[i] = transformMatrix.MultiplyPoint3x4(vs[i]);

            vertices.AddRange(vs);

            var rotation = Quaternion.LookRotation(transformMatrix.GetColumn(2), transformMatrix.GetColumn(1));
            var ns = meshToMerge.normals;

            if (ns != null && ns.Length > 0)
            {
                for (var i = 0; i < ns.Length; i++)
                    ns[i] = rotation * ns[i];

                normals.AddRange(ns);
            }

            var uvs = meshToMerge.uv;

            if (uvs != null && uvs.Length > 0)
                uv1s.AddRange(uvs);

            uvs = meshToMerge.uv2;

            if (uvs != null && uvs.Length > 0)
                uv2s.AddRange(uvs);

            for (var i = 0; i < ms.Length; i++)
                if (i < meshToMerge.subMeshCount)
                {
                    var ts = meshToMerge.GetTriangles(i);

                    if (ts.Length > 0)
                    {
                        if (ms[i] != null && !subMeshes.ContainsKey(ms[i]))
                            subMeshes.Add(ms[i], new List<int>());

                        var subMesh = subMeshes[ms[i]];

                        for (var t = 0; t < ts.Length; t++)
                            ts[t] += vertexOffset;

                        subMesh.AddRange(ts);
                    }
                }
        }

        /// <summary>
        ///     Fills the specified length.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="len">The length.</param>
        /// <returns></returns>
        public static IEnumerable<T> Fill<T>(this List<T> list, int len)
        {
            if (list.Count >= len)
                return list.Take(len - 1);

            //Debug.Log($"ListCount: {list.Count} | Length: {len}");

            for (var i = list.Count; i < len; ++i)
                list.Add(default);

            //Debug.Log($"ListCount: {list.Count} | Length: {len}");

            return list;
        }

        /// <summary>
        ///     Gets the color of the similar.
        /// </summary>
        /// <param name="c1">The c1.</param>
        /// <param name="cs">The cs.</param>
        /// <returns></returns>
        public static Color GetSimilarColor(this Color c1, IEnumerable<Color> cs)
        {
            return cs.OrderBy(x => x.ColorThreshold(c1)).FirstOrDefault();
        }

        /// <summary>
        ///     Colors the threshold.
        /// </summary>
        /// <param name="c1">The c1.</param>
        /// <param name="c2">The c2.</param>
        /// <returns></returns>
        public static int ColorThreshold(this Color c1, Color c2)
        {
            return Math.Abs(c1.r - c2.r) + Math.Abs(c1.g - c2.g) + Math.Abs(c1.b - c2.b);
        }

        /// <summary>
        ///     Colors the similary perc.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public static float ColorSimilaryPerc(this Color a, Color b)
        {
            return 1f - a.ColorThreshold(b) / (256f * 3);
        }

        // Voronoi impl
        // See: https://en.wikipedia.org/wiki/Voronoi_diagram#Illustration
        // And: https://codereview.stackexchange.com/questions/139059/order-a-list-of-vertices-by-closest-distance

        /// <summary>
        ///     Pow2s the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        private static double Pow2(double x)
        {
            return x * x;
        }

        /// <summary>
        ///     Pow2s the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        private static float Pow2(float x)
        {
            return x * x;
        }

        /// <summary>
        ///     Pow2s the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        private static int Pow2(int x)
        {
            return x * x;
        }

        /// <summary>
        ///     Distance2s the specified p2.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns></returns>
        public static int Distance2(this Point p1, Point p2)
        {
            return Pow2(p2.x - p1.x) + Pow2(p2.y - p1.y);
        }

        /// <summary>
        ///     Taxicabs the distance.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns></returns>
        public static int TaxicabDistance(this Point p1, Point p2)
        {
            return Mathf.Abs(p2.x - p1.x) + Mathf.Abs(p2.y - p1.y);
        }

        /// <summary>
        ///     Grids the count.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="xGrid">The x grid.</param>
        /// <param name="yGrid">The y grid.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="check">The check.</param>
        /// <returns></returns>
        public static int GridCount<T>(this T[] source, int x, int y, int xGrid, int yGrid, int width, int height,
            T check)
            where T : IEquatable<T>
        {
            var sum = 0;

            for (var _x = -xGrid; _x <= xGrid; ++_x)
                for (var _y = -yGrid; _y <= yGrid; ++_y)
                    if (source[P(x + _x, y + _y, width, height)].Equals(check))
                        ++sum;

            return sum;
        }

        /// <summary>
        ///     Grids the check.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="xGrid">The x grid.</param>
        /// <param name="yGrid">The y grid.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="check">The check.</param>
        /// <returns></returns>
        public static float GridProbability<T>(this T[] source, int x, int y, int xGrid, int yGrid, int width,
            int height, T check)
            where T : IEquatable<T>
        {
            float total = xGrid * yGrid * 4,
                sum = 0;

            for (var _x = -xGrid; _x <= xGrid; ++_x)
                for (var _y = -yGrid; _y <= yGrid; ++_y)
                {
                    if (_x == 0 && _y == 0) continue;
                    sum += source[P(x + _x, y + _y, width, height)].Equals(check) ? 1f : 0f;
                }

            return sum / total;
        }

        /// <summary>
        ///     Simplifieds to string.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        public static string SimplifiedToString(this Vector2 v)
        {
            return $"({v.x.ToString("F2")}, {v.y.ToString("F2")})";
        }

        /// <summary>
        ///     PIP Problem
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool IsInPolygon(this Point[] poly, Point p)
        {
            Point p1, p2;
            var inside = false;

            if (poly.Length < 3)
                return inside;

            var oldPoint = new Point(
                poly[poly.Length - 1].x, poly[poly.Length - 1].y);

            for (var i = 0; i < poly.Length; i++)
            {
                var newPoint = new Point(poly[i].x, poly[i].y);

                if (newPoint.x > oldPoint.x)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if (newPoint.x < p.x == p.x <= oldPoint.x
                    && (p.y - (long)p1.y) * (p2.x - p1.x)
                    < (p2.y - (long)p1.y) * (p.x - p1.x))
                    inside = !inside;

                oldPoint = newPoint;
            }

            return inside;
        }

        /// <summary>
        ///     Determines whether [is in polygon] [the specified point].
        /// </summary>
        /// <param name="poly">The poly.</param>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        public static bool? IsInPolygon(this Polygon poly, Point point)
        {
            if (poly.Segments.IsNullOrEmpty())
                return null;

            var vertices = poly.GetEdgeVecArray();

            var coef = vertices.Skip(1).Select((p, i) =>
                (point.y - vertices[i].y) * (p.x - vertices[i].x)
                - (point.x - vertices[i].x) * (p.y - vertices[i].y));

            var coefNum = coef.GetEnumerator();

            if (coef.Any(p => p == 0))
                return true;

            var lastCoef = coefNum.Current;
            var count = coef.Count();

            coefNum.MoveNext();

            do
            {
                if (coefNum.Current - lastCoef < 0)
                    return false;

                lastCoef = coefNum.Current;
            } while (coefNum.MoveNext());

            return true;
        }

        /// <summary>
        ///     Determines whether [is in polygon] [the specified poly].
        /// </summary>
        /// <param name="polygon">The polygon.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>
        ///     <c>true</c> if [is in polygon] [the specified poly]; otherwise, <c>false</c>.
        /// </returns>
        public static bool? InPolygon(this Polygon polygon, int x, int y)
        {
            if (polygon.Segments.IsNullOrEmpty())
                return null;

            // Test the ray against all segments
            var intersections = 0;

            var originSegment = new SegmentF(Vector2.zero, new Vector2(x, y));

            for (var segment = 0; segment < polygon.Segments.Length; ++segment)
                // Test if current segment intersects with ray.
                // If yes, intersections++;

                if (DoesIntersect(polygon.Segments[segment], originSegment))
                    ++intersections;

            // The same as: interesections % 2 != 0
            if ((intersections & 1) == 1
            ) // If the intersection count is ood that means that the current point is inside the polygon
                return true; // Insegment of polygon

            return false;
        }

        /// <summary>
        ///     Resolves the pip problem.
        /// </summary>
        /// <param name="polygon">The polygon.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        public static PIPResult IsInPolygon(this Polygon polygon, int x, int y)
        {
            var result = polygon.InPolygon(x, y);

            if (!result.HasValue)
                return PIPResult.IsNull;

            return result.Value ? PIPResult.IsInside : PIPResult.IsOutside;
        }

        /// <summary>
        ///     Doeses the intersect.
        /// </summary>
        /// <param name="segmentA">The segment a.</param>
        /// <param name="segmentB">The segment b.</param>
        /// <returns></returns>
        public static bool DoesIntersect(SegmentF segmentA, SegmentF segmentB)
        {
            return FindIntersection(segmentA, segmentB) != default;
        }

        /// <summary>
        ///     Finds the intersection.
        /// </summary>
        /// <param name="segmentA">The segment a.</param>
        /// <param name="segmentB">The segment b.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns></returns>
        public static Vector2 FindIntersection(SegmentF segmentA, SegmentF segmentB, float tolerance = 0.001f)
        {
            float x1 = segmentA.start.x, y1 = segmentA.start.y;
            float x2 = segmentA.end.x, y2 = segmentA.end.y;

            float x3 = segmentB.start.x, y3 = segmentB.start.y;
            float x4 = segmentB.end.x, y4 = segmentB.end.y;

            return FindIntersection(x1, y1, x2, y2, x3, y3, x4, y4, tolerance);
        }

        /// <summary>
        ///     Finds the intersection.
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <param name="x3">The x3.</param>
        /// <param name="y3">The y3.</param>
        /// <param name="x4">The x4.</param>
        /// <param name="y4">The y4.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        ///     Both segments overlap vertically, ambiguous intersection vertices.
        ///     or
        ///     Both segments overlap horizontally, ambiguous intersection vertices.
        /// </exception>
        public static Vector2 FindIntersection(float x1, float y1, float x2, float y2, float x3, float y3, float x4,
            float y4, float tolerance = 0.001f)
        {
            // equations of the form x = c (two vertical segments)
            if (Math.Abs(x1 - x2) < tolerance && Math.Abs(x3 - x4) < tolerance && Math.Abs(x1 - x3) < tolerance)
                throw new Exception("Both segments overlap vertically, ambiguous intersection vertices.");

            //equations of the form y=c (two horizontal segments)
            if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance && Math.Abs(y1 - y3) < tolerance)
                throw new Exception("Both segments overlap horizontally, ambiguous intersection vertices.");

            //equations of the form x=c (two vertical segments)
            if (Math.Abs(x1 - x2) < tolerance && Math.Abs(x3 - x4) < tolerance)
                return default;

            //equations of the form y=c (two horizontal segments)
            if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance)
                return default;

            //general equation of segment is y = mx + c where m is the slope
            //assume equation of segment 1 as y1 = m1x1 + c1
            //=> -m1x1 + y1 = c1 ----(1)
            //assume equation of segment 2 as y2 = m2x2 + c2
            //=> -m2x2 + y2 = c2 -----(2)
            //if segment 1 and 2 intersect then x1=x2=x & y1=y2=y where (x,y) is the intersection point
            //so we will get below two equations
            //-m1x + y = c1 --------(3)
            //-m2x + y = c2 --------(4)

            float x, y;

            //segmentA is vertical x1 = x2
            //slope will be infinity
            //so lets derive another solution
            if (Math.Abs(x1 - x2) < tolerance)
            {
                //compute slope of segment 2 (m2) and c2
                var m2 = (y4 - y3) / (x4 - x3);
                var c2 = -m2 * x3 + y3;

                //equation of vertical segment is x = c
                //if segment 1 and 2 intersect then x1=c1=x
                //subsitute x=x1 in (4) => -m2x1 + y = c2
                // => y = c2 + m2x1
                x = x1;
                y = c2 + m2 * x1;
            }
            //segmentB is vertical x3 = x4
            //slope will be infinity
            //so lets derive another solution
            else if (Math.Abs(x3 - x4) < tolerance)
            {
                //compute slope of segment 1 (m1) and c2
                var m1 = (y2 - y1) / (x2 - x1);
                var c1 = -m1 * x1 + y1;

                //equation of vertical segment is x = c
                //if segment 1 and 2 intersect then x3=c3=x
                //subsitute x=x3 in (3) => -m1x3 + y = c1
                // => y = c1 + m1x3
                x = x3;
                y = c1 + m1 * x3;
            }
            //segmentA & segmentB are not vertical
            //(could be horizontal we can handle it with slope = 0)
            else
            {
                //compute slope of segment 1 (m1) and c2
                var m1 = (y2 - y1) / (x2 - x1);
                var c1 = -m1 * x1 + y1;

                //compute slope of segment 2 (m2) and c2
                var m2 = (y4 - y3) / (x4 - x3);
                var c2 = -m2 * x3 + y3;

                //solving equations (3) & (4) => x = (c1-c2)/(m2-m1)
                //plugging x value in equation (4) => y = c2 + m2 * x
                x = (c1 - c2) / (m2 - m1);
                y = c2 + m2 * x;

                //verify by plugging intersection point (x, y)
                //in orginal equations (1) & (2) to see if they intersect
                //otherwise x,y values will not be finite and will fail this check
                if (!(Math.Abs(-m1 * x + y - c1) < tolerance
                      && Math.Abs(-m2 * x + y - c2) < tolerance))
                    return default;
            }

            //x,y can intersect outside the segment segment since segment is infinitely long
            //so finally check if x, y is within both the segment segments
            if (IsInsideSegment(new SegmentF(x1, y1, x2, y2), x, y) &&
                IsInsideSegment(new SegmentF(x3, y3, x4, y4), x, y))
                return new Vector2 { x = x, y = y };

            //return default null (no intersection)
            return default;
        }

        /// <summary>
        ///     Returns true if given point(x,y) is inside the given segment segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static bool IsInsideSegment(SegmentF segment, float x, float y)
        {
            return (x >= segment.start.x && x <= segment.end.x
                    || x >= segment.end.x && x <= segment.start.x)
                   && (y >= segment.start.y && y <= segment.end.y
                       || y >= segment.end.y && y <= segment.start.y);
        }

        /// <summary>
        ///     Gets the formatted dir.
        /// </summary>
        /// <param name="innerDirection">The inner direction.</param>
        /// <returns></returns>
        public static string GetFormattedDir(this InnerDirection innerDirection)
        {
            var len = 5 - innerDirection.ToString().Length;

            // Debug.Log("Inner Dir: " + innerDirection + " | Length: " + len);

            return innerDirection + (len > 0 ? new string(' ', len) : "");
        }

        /// <summary>
        ///     Clones the specified enumeration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="num">The enumeration.</param>
        /// <returns></returns>
        public static T Clone<T>(this T num)
            where T : struct, IConvertible
        {
            try
            {
                return (T)(object)Convert.ToInt32(num);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        ///     Converts the range.
        /// </summary>
        /// <param name="originalStart">The original start.</param>
        /// <param name="originalEnd">The original end.</param>
        /// <param name="newStart">The new start.</param>
        /// <param name="newEnd">The new end.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static float ConvertRange(
            float originalStart, float originalEnd, // original range
            float newStart, float newEnd, // desired range
            float value) // value to convert
        {
            var scale = (newEnd - newStart) / (originalEnd - originalStart);
            return newStart + (value - originalStart) * scale;
        }

        /// <summary>
        ///     Gets the bounded noise.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="meanHeight">Height of the mean.</param>
        /// <param name="amplitude">The amplitude.</param>
        /// <returns></returns>
        // [InRange(-.5f, .5f)] && [InRange(0, 1)]
        public static float GetBoundedNoise(float value, float meanHeight, float amplitude)
        {
            return Mathf.Clamp01(ConvertRange(0, 1, -amplitude, amplitude, ConvertRange(-1, 1, 0, 1, value)) +
                                 (meanHeight + .5f));
        }

        /// <summary>
        /// Clamps the noise01.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static float ClampNoise01(float value)
        {
            return ClampNoise(value, 0, 1);
        }

        /// <summary>
        /// Clamps the noise.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns></returns>
        public static float ClampNoise(float value, int minValue, int maxValue)
        {
            return ConvertRange(-1, 1, minValue, maxValue, value);
        }

        /// <summary>
        ///     Ases the color of the component.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        public static Color AsComponentColor(this UEColor color)
        {
            return (Color)color;
        }

        /// <summary>
        ///     Ases the color of the unity.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        public static UEColor AsUnityColor(this Color color)
        {
            return (UEColor)color;
        }

        /// <summary>
        ///     Froms the base class to derived class.
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <param name="baseObj">The base object.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static T FromBaseClassToDerivedClass<T>(this object baseObj)
            where T : new() //, params object[] args)
        {
            var derivedObj = new T(); //(T)Activator.CreateInstance(typeof(T));
            var t = baseObj.GetType();

            foreach (var fieldInf in t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                fieldInf.SetValue(derivedObj, fieldInf.GetValue(baseObj));

            foreach (var propInf in t.GetProperties(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                try
                {
                    propInf.SetValue(derivedObj, propInf.GetValue(baseObj));
                }
                catch
                {
                    // Some properties hasn't setter...
                }

            return derivedObj;
        }

        /// <summary>
        ///     Gets the center.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns></returns>
        public static Point GetCenter(this ICollection<Point> points)
        {
            Point minBound = new Point(points.Min(p => p.x), points.Min(p => p.y)),
                maxBound = new Point(points.Max(p => p.x), points.Max(p => p.y));

            return new Point((maxBound.x - minBound.x) / 2, (maxBound.y - minBound.y) / 2);
        }

        /// <summary>
        ///     Casts from eng.
        /// </summary>
        /// <param name="color32">The color32.</param>
        /// <returns></returns>
        public static IEnumerable<UEColor> CastFromEng(this NativeArray<Color32> color32)
        {
            return color32.Select(x => (UEColor)x);
        }

        /// <summary>
        ///     Gets the size of.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static long GetSizeOf(this object obj)
        {
            long size = 0;

            using (Stream s = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(s, obj);
                size = s.Length;
            }

            return size;
        }

        /// <summary>
        ///     Creates the texture object.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="center">The center.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static TextureObject CreateTextureObject(this IEnumerable<Point> list, Point center, Point offset)
        {
            Point minBounds = new Point(list.Min(v => v.x), list.Min(v => v.y)),
                maxBounds = new Point(list.Max(v => v.x), list.Max(v => v.y));

            int width = Mathf.CeilToInt(maxBounds.x - minBounds.x) + 1 + offset.x * 2,
                height = Mathf.CeilToInt(maxBounds.y - minBounds.y) + 1 + offset.y * 2;

            //Debug.Log($"Min Bounds: {minBounds}");
            //Debug.Log($"Max Bounds: {maxBounds}");
            //Debug.Log($"Width: {width}");
            //Debug.Log($"Height: {height}");
            //Debug.Log($"Center: {center}");

            return new TextureObject(new Color32[width * height], width, height);
        }

        /// <summary>
        ///     Draws the texture from points.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="textureObject">The texture object.</param>
        /// <param name="center">The center.</param>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        public static TextureObject DrawTextureFromPoints(this IEnumerable<Point> list, TextureObject textureObject,
            Point center, UEColor color)
        {
            return list.DrawTextureFromPoints(center, Point.zero, color, textureObject);
        }

        /// <summary>
        ///     Draws the texture from points.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="textureObject">The texture object.</param>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        public static TextureObject DrawTextureFromPoints(this IEnumerable<Point> list, TextureObject textureObject,
            UEColor color)
        {
            return list.DrawTextureFromPoints(Point.zero, Point.zero, color, textureObject);
        }

        /// <summary>
        ///     Draws the texture from points.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="textureObject">The texture object.</param>
        /// <param name="center">The center.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static TextureObject DrawTextureFromPoints(this IEnumerable<Point> list, TextureObject textureObject,
            Point center, Point offset, UEColor color)
        {
            return list.DrawTextureFromPoints(center, offset, color, textureObject);
        }

        /// <summary>
        ///     Draws the texture from points.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="center">The center.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="tex">The tex.</param>
        /// <returns></returns>
        public static TextureObject DrawTextureFromPoints(this IEnumerable<Point> list, Point center, Point offset,
            UEColor color, TextureObject tex = null)
        {
            if (tex == null)
                tex = list.CreateTextureObject(center, offset);

            var arr = list.ToArray();

            //Texture2D tex = new Texture2D(width, height);

            for (var i = 0; i < arr.Length; i++)
            {
                var j = i == 0 ? arr.Length - 1 : i - 1;

                Point //abs = new Point(Mathf.Abs(minBounds.x), Mathf.Abs(minBounds.y)),
                    cur = offset + arr[i] - center,
                    prev = offset + arr[j] - center;

                try
                {
                    tex.Colors[P(cur.x, cur.y, tex.Width, tex.Height)] = color;

                    //cs.DrawLine(cur, prev, width, height, UnityEngine.Color.red);
                }
                catch
                {
                    Debug.LogError($"Error at: {cur} --> {arr[i]}");
                }
            }

            return new TextureObject(tex.Colors, tex.Width, tex.Height);
        }

        /// <summary>
        ///     ps the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="w">The w.</param>
        /// <param name="h">The h.</param>
        /// <returns></returns>
        public static int P(float x, float y, int w, int h)
        {
            //if (x < 0) x = w - x;
            //if (x >= w) x = x % w;
            //if (y < 0) y = h - y;
            //if (y >= h) y = y % w;

            return (int)(x + (h - y - 1) * w);
        }

        /// <summary>
        ///     Scales the specified points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="pivot">The pivot.</param>
        /// <returns></returns>
        public static IEnumerable<Point> Scale(this IEnumerable<Point> points, float scale, Vector2 pivot)
        {
            foreach (Vector2 v in points)
            {
                var magnitude = (v - pivot).magnitude;
                var n = (v - pivot).normalized;

                var _v = n * (scale * magnitude) + pivot;
                yield return new Point(_v);
            }
        }

        /// <summary>
        ///     Adds the or set.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TCol">The type of the col.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void AddOrSet<TKey, TCol, TValue>(this Dictionary<TKey, TCol> dictionary, TKey key, TValue value)
            where TCol : ICollection<TValue>, new()
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, default);

            var col = dictionary[key];

            if (col == null)
                col = new TCol(); //Activator.CreateInstance<TCol>();

            col.Add(value);

            // This is needed ?
            dictionary[key] = col;
        }

        /// <summary>
        ///     Counts the specified dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TCol">The type of the col.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns></returns>
        public static int Count<TKey, TCol, TValue>(this Dictionary<TKey, TCol> dictionary)
            where TCol : ICollection<TValue>
        {
            return dictionary.Sum(x => x.Value.Count);
        }

        /// <summary>
        ///     Grids the check.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="points">The points.</param>
        /// <param name="xGrid">The x grid.</param>
        /// <param name="yGrid">The y grid.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="func">The function.</param>
        public static void GridCheck<T>(this T[] source, IEnumerable<Point> points, int xGrid, int yGrid, int width,
                int height, Func<Point, int, int, T, T, LoopOperation?> func)
        //where T : IEquatable<T>
        {
            LoopOperation? operation = null;

            foreach (var p in points)
            {
                for (var _x = -xGrid; _x <= xGrid; ++_x)
                {
                    for (var _y = -yGrid; _y <= yGrid; ++_y)
                        //if (source[P(x + _x, y + _y, width, height)].Equals(check))
                        if (func != null)
                            try
                            {
                                operation = func.Invoke(p, _x, _y, source[P(p.x + _x, p.y + _y, width, height)],
                                    source[P(p.x, p.y, width, height)]);

                                if (operation.HasValue)
                                {
                                    if (operation.Value == LoopOperation.Continue)
                                        continue;
                                    if (operation.Value == LoopOperation.Break)
                                        break;
                                    if (operation.Value == LoopOperation.Exit)
                                        return;
                                }
                            }
                            catch
                            {
                            }

                    if (operation.HasValue)
                    {
                        if (operation.Value == LoopOperation.Continue)
                            continue;
                        if (operation.Value == LoopOperation.Break)
                            break;
                    }

                    operation = null;
                }

                operation = null;
            }
        }

        /// <summary>
        /// Grids the check.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="p">The v.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="gridSize">Size of the grid.</param>
        /// <param name="iterationCallback">The iteration callback.</param>
        /// <returns></returns>
        public static IEnumerable<T> GridCheck<T>(this T[] source, Point p, int width, int height, int gridSize, Action iterationCallback = null)
        {
            return GridCheck(source, p.x, p.y, width, height, gridSize, false, null, iterationCallback);
        }

        /// <summary>
        /// Grids the check.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TConv">The type of the conv.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="gridSize">Size of the grid.</param>
        /// <param name="useYInverted">if set to <c>true</c> [use y inverted].</param>
        /// <param name="preTransform">The pre transform.</param>
        /// <param name="transform">The transform.</param>
        /// <param name="iterationCallback">The iteration callback.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Grid size must be greater than zero. - gridSize</exception>
        /// <exception cref="System.ArgumentException">Grid size must be greater than zero. - gridSize</exception>
        public static IEnumerable<T> GridCheck<T, TConv>(this T[] source, int x, int y, int width, int height, int gridSize, bool useYInverted = false, Func<int, int, TConv> preTransform = null, Func<int, int, TConv, T> transform = null, Action iterationCallback = null)
        {
            if (gridSize < 1)
                throw new ArgumentException("Grid size must be greater than zero.", nameof(gridSize));

            if (preTransform == null && transform != null)
                throw new ArgumentNullException(nameof(preTransform));

            /*

            For(i = -N to N, included) (N=1,2,3...)
                Add(i, -N) // Top
                Add(i, N) // Bottom
                If (abs(i) != N){
                  Add(N, i) // Right
                  Add(-N, i) // Left
                }

             */

            for (var i = -gridSize; i <= gridSize; i++)
            {
                var topIndex = useYInverted
                    ? PSafe(x + i, y - gridSize, width, height, out var isTopOut)
                    : PnSafe(x + i, y - gridSize, width, height, out isTopOut);
                var bottomIndex = useYInverted
                    ? PSafe(x + i, y + gridSize, width, height, out var isBottomOut)
                    : PnSafe(x + i, y + gridSize, width, height, out isBottomOut);

                if (!isTopOut)
                {
                    yield return transform == null
                        ? source[topIndex]
                        : transform(x + i, y - gridSize, preTransform(x + i, y - gridSize));
                }

                if (!isBottomOut)
                {
                    yield return transform == null
                        ? source[bottomIndex]
                        : transform(x + i, y + gridSize, preTransform(x + i, y + gridSize));

                    // yield return source[bottomIndex];
                }

                if (Mathf.Abs(i) != gridSize)
                {
                    var leftIndex = useYInverted
                        ? PSafe(x + gridSize, y + i, width, height, out var isLeftOut)
                        : PnSafe(x + gridSize, y + i, width, height, out isLeftOut);
                    var rightIndex = useYInverted
                        ? PSafe(x - gridSize, y + i, width, height, out var isRightOut)
                        : PnSafe(x - gridSize, y + i, width, height, out isRightOut);

                    if (!isLeftOut)
                    {
                        yield return transform == null
                            ? source[leftIndex]
                            : transform(x + gridSize, y + i, preTransform(x + gridSize, y + i));

                        //yield return source[leftIndex];
                    }

                    if (!isRightOut)
                    {
                        yield return transform == null
                            ? source[rightIndex]
                            : transform(x - gridSize, y + i, preTransform(x - gridSize, y + i));

                        //yield return source[rightIndex];
                    }
                }

                iterationCallback?.Invoke();
            }
        }

        /// <summary>
        /// Grids the check.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="gridSize">Size of the grid.</param>
        /// <param name="useYInverted">if set to <c>true</c> [use y inverted].</param>
        /// <param name="transform">The transform.</param>
        /// <param name="iterationCallback">The iteration callback.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Grid size must be greater than zero. - gridSize</exception>
        public static IEnumerable<T> GridCheck<T>(this T[] source, int x, int y, int width, int height, int gridSize, bool useYInverted = false, Func<int, int, T> transform = null, Action iterationCallback = null)
        {
            if (gridSize < 1)
                throw new ArgumentException("Grid size must be greater than zero.", nameof(gridSize));

            /*

            For(i = -N to N, included) (N=1,2,3...)
                Add(i, -N) // Top
                Add(i, N) // Bottom
                If (abs(i) != N){
                  Add(N, i) // Right
                  Add(-N, i) // Left
                }

             */

            for (var i = -gridSize; i <= gridSize; i++)
            {
                var topIndex = useYInverted
                    ? PSafe(x + i, y - gridSize, width, height, out var isTopOut)
                    : PnSafe(x + i, y - gridSize, width, height, out isTopOut);
                var bottomIndex = useYInverted
                    ? PSafe(x + i, y + gridSize, width, height, out var isBottomOut)
                    : PnSafe(x + i, y + gridSize, width, height, out isBottomOut);

                if (!isTopOut)
                {
                    yield return transform == null
                        ? source[topIndex]
                        : transform(x + i, y - gridSize);
                }

                if (!isBottomOut)
                {
                    yield return transform == null
                        ? source[bottomIndex]
                        : transform(x + i, y + gridSize);

                    // yield return source[bottomIndex];
                }

                if (Mathf.Abs(i) != gridSize)
                {
                    var leftIndex = useYInverted
                        ? PSafe(x + gridSize, y + i, width, height, out var isLeftOut)
                        : PnSafe(x + gridSize, y + i, width, height, out isLeftOut);
                    var rightIndex = useYInverted
                        ? PSafe(x - gridSize, y + i, width, height, out var isRightOut)
                        : PnSafe(x - gridSize, y + i, width, height, out isRightOut);

                    if (!isLeftOut)
                    {
                        yield return transform == null
                            ? source[leftIndex]
                            : transform(x + gridSize, y + i);

                        //yield return source[leftIndex];
                    }

                    if (!isRightOut)
                    {
                        yield return transform == null
                            ? source[rightIndex]
                            : transform(x - gridSize, y + i);

                        //yield return source[rightIndex];
                    }
                }

                iterationCallback?.Invoke();
            }
        }

        /// <summary>
        /// Get the most common value in collection.
        /// </summary>
        /// <typeparam name="Tsrc">The type of the source.</typeparam>
        /// <typeparam name="Tres">The type of the resource.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="transform">The transform.</param>
        /// <returns></returns>
        public static Tres MostCommon<Tsrc, Tres>(this IEnumerable<Tsrc> source, Func<Tsrc, Tres> transform)
        {
            return source
                .GroupBy(transform)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;
        }

        /// <summary>
        /// Get the most common value in collection.
        /// </summary>
        /// <typeparam name="Tsrc">The type of the source.</typeparam>
        /// <typeparam name="Tres">The type of the resource.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="transform">The transform.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public static Tres MostCommon<Tsrc, Tres>(this IEnumerable<Tsrc> source, Func<Tsrc, Tres> transform, Predicate<Tsrc> predicate)
        {
            return source
                .Where(i => predicate(i))
                .GroupBy(transform)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;
        }

        // TODO: Remove this.
        //        /// <summary>
        //        /// Smoothes the polygon.
        //        /// </summary>
        //        /// <param name="pol">The pol.</param>
        //        /// <param name="offset">The offset.</param>
        //        /// <param name="effectDistance">The effect distance.</param>
        //        /// <param name="noise">The noise.</param>
        //        public static void SmoothPolygon(Polygon pol, Point offset, Point effectDistance, FastNoise noise)
        //        {
        //            var ps = pol.Vertices.Select(v => v - pol.Position + offset).ToArray();

        //            var list = pol.Vertices.AsEnumerable();
        //            var tex = list.CreateTextureObject(pol.Position, offset);

        //            var region = new byte[tex.Width * tex.Height];

        //            var textureCenter = new Vector2(tex.Width / 2, tex.Height / 2);

        //            for (var i = 0; i < tex.Length; ++i)
        //            {
        //                int x = tex.GetX(i),
        //                    y = tex.GetY(i);

        //                var p = new Point(x, y);
        //#if DRAW_NOISES
        //                if (!ps.IsInPolygon(v))
        //                {
        //                    tex.SetColor(x, y, Color.Lerp(Color.black, Color.white, F.GetBoundedNoise(noise.GetValueFractal(x, y), waterGT.MeanHeight, waterGT.Amplitude)));
        //                    region[F.P(x, y, tex.Width, tex.Height)] = 2;
        //                }
        //                else
        //                    tex.SetColor(x, y, Color.Lerp(Color.black, Color.white, F.GetBoundedNoise(noise.GetValueFractal(x, y), grassGT.MeanHeight, grassGT.Amplitude)));
        //#else
        //                if (!ps.IsInPolygon(p))
        //                    tex.SetColor(x, y, UnityEngine.Color.green);
        //#endif
        //            }

        //            // Edges Scaled
        //            var smoother = new CircleSmooth(textureCenter);

        //            // Edges Scaled
        //            var shadowRegion = new HashSet<Point>(smoother.Create(tex.Width, tex.Height, pol, offset, effectDistance));

        //            foreach (var pShadow in shadowRegion)
        //                region[P(pShadow.x, pShadow.y, tex.Width, tex.Height)] = 1;

        //            Debug.Log($"Real Count: {shadowRegion.Count}");

        //            int it = 0, postIt = 0, cIt = 0;

        //            HashSet<Point> innerPoints = new HashSet<Point>(),
        //                outerPoints = new HashSet<Point>();

        //            // Optimize (to get adjacents points without another array)
        //            region.GridCheck(shadowRegion, 1, 1, tex.Width, tex.Height, (p, x, y, _ic, _c) =>
        //            {
        //                ++it;

        //                if (x == 0 && y == 0)
        //                    return LoopOperation.Continue;
        //                if (_c == 3)
        //                    return LoopOperation.Continue;

        //                ++postIt;

        //                if (!shadowRegion.Contains(p + new Point(x, y)))
        //                {
        //                    if (_ic == 0)
        //                        innerPoints.Add(p);
        //                    else if (_ic == 2)
        //                        outerPoints.Add(p);

        //                    region[P(p.x, p.y, tex.Width, tex.Height)] = 3;

        //                    ++cIt;
        //                }

        //                return null;
        //            });

        //            //Debug.Log($"Iterations: {it} | Post Iterations: {postIt} | Contained Iteraions: {cIt}");

        //            // Note: We have to find a way to get sourronding biomes
        //            // Maybe passing source array
        //            smoother.DisplayColorsAndClear(tex.Width, tex.Height, tex.Colors, innerPoints, outerPoints, (_x, _y, _d) =>
        //            {
        //                //#if DRAW_NOISES
        //                // ReSharper disable once ConvertToLambdaExpression
        //                return UnityEngine.Color.Lerp(
        //                    UnityEngine.Color.black,
        //                    UnityEngine.Color.white,
        //                    Mathf.Lerp(
        //                        GetBoundedNoise(noise.GetValueFractal(_x, _y),
        //                            //waterGT.MeanHeight, waterGT.Amplitude
        //                            0, 0
        //                        ),
        //                        GetBoundedNoise(noise.GetValueFractal(_x, _y),
        //                            //grassGT.MeanHeight, grassGT.Amplitude
        //                            0, 0
        //                        ), _d));
        //                //#else
        //                //                return UnityEngine.Color.Lerp(Color.black, Color.red, _d);
        //                //#endif
        //            });

        //#if DRAW_POINTS
        //            list.DrawTextureFromPoints(tex, pol.Position, offset, Color.magenta);
        //#endif

        //#if !DRAW_NOISES

        //#if DRAW_ADJACENTS
        //            innerPoints.DrawTextureFromPoints(tex, Color.yellow);
        //            outerPoints.DrawTextureFromPoints(tex, Color.blue);
        //#endif

        //#endif
        //        }

        /// <summary>
        /// Casts to int.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static int CastToInt(this float? value)
        {
            return value.HasValue ? (int)value.Value : 0;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public static string ToString(this float? value, string format)
        {
            return value.HasValue ? value.Value.ToString(format) : string.Empty;
        }

        /// <summary>
        /// To the rounded string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string ToRoundedString(this Vector3 value)
        {
            return $"({(int)value.x}, {(int)value.y}, {(int)value.z})";
        }

        /// <summary>
        /// To the rounded string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        public static string ToRoundedString(this Vector2 value, string separator = ", ")
        {
            return $"({(int)value.x}{separator}{(int)value.y})";
        }

        /// <summary>
        /// Appends the separated line.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="c">The c.</param>
        /// <param name="length">The length.</param>
        public static void AppendSeparatedLine(this StringBuilder builder, char c = '=', int length = 30)
        {
            builder.AppendLine();
            builder.AppendLine(GetSeparator(c, length));
            builder.AppendLine();
        }

        /// <summary>
        /// Gets the separator.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string GetSeparator(char c = '=', int length = 30)
        {
            return new string(c, length);
        }

        /// <summary>
        /// Gets the lengths.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">values</exception>
        public static string GetLengths(this Vector2[,] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            return $"[{values.GetLength(0)}x{values.GetLength(1)}]";
        }

        /// <summary>
        /// Adds to average.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="lastValue">The last value.</param>
        /// <param name="times">The times.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">times</exception>
        public static float AddToAvg(this float value, float lastValue, ref int times)
        {
            if (times < 0)
                throw new ArgumentException(nameof(times));

            if (times == 0)
            {
                ++times;
                return value;
            }

            return (lastValue * times + value) / ++times;
        }

#if !UNITY_WEBGL
        /// <summary>
        /// Sizes the of.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int SizeOf<T>()
        {
            Type type = typeof(T);

            var dm = new DynamicMethod("SizeOfType", typeof(int), new Type[] { });
            ILGenerator il = dm.GetILGenerator();
            il.Emit(OpCodes.Sizeof, type);
            il.Emit(OpCodes.Ret);

            return (int)dm.Invoke(null, null);
        }

        /// <summary>
        /// Gets the size of terrain in memory.
        /// </summary>
        /// <param name="resolution">The resolution.</param>
        /// <returns></returns>
        public static long GetSizeOfTerrainInMemory(int resolution)
        {
            return GetSizeOfTerrainInMemory(resolution, resolution);
        }

        /// <summary>
        /// Gets the size of terrain in memory.
        /// </summary>
        /// <param name="xRes">The x resource.</param>
        /// <param name="zRes">The z resource.</param>
        /// <returns></returns>
        public static long GetSizeOfTerrainInMemory(int xRes, int zRes)
        {
            //if (Mathf.Sqrt(resolution - 1) % 1 <= float.Epsilon * 100)
            //    throw new ArgumentNullException(nameof(resolution), "Resolution must follow this pattern: pow(value, 2) + 1");

            // TODO: Check that resolution follows 513 * 513 pattern

            long sizeOfVector3 = SizeOf<Vector3>();
            long sizeOfInt = sizeof(int);
            //long totalResolution = resolution * resolution;

            return sizeOfInt * xRes * zRes * 6 + sizeOfVector3 * (xRes + 1) * (zRes + 1);
        }
#endif

        /// <summary>
        /// Clones the terrain.
        /// </summary>
        /// <param name="terrain">The terrain.</param>
        /// <param name="cloneData">if set to <c>true</c> [clone data].</param>
        /// <returns></returns>
        public static Terrain CloneTerrain(this Terrain terrain, bool cloneData = true)
        {
            return CloneTerrain(terrain, null, null, cloneData);
        }

        /// <summary>
        /// Clones the terrain.
        /// </summary>
        /// <param name="terrain">The terrain.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="cloneData">if set to <c>true</c> [clone data].</param>
        /// <returns></returns>
        public static Terrain CloneTerrain(this Terrain terrain, Vector3 position, Quaternion rotation, bool cloneData = true)
        {
            return CloneTerrain(terrain, (Vector3?)position, rotation, cloneData);
        }

        /// <summary>
        /// Clones the terrain.
        /// </summary>
        /// <param name="terrain">The terrain.</param>
        /// <param name="cloneData">if set to <c>true</c> [clone data].</param>
        /// <returns></returns>
        private static Terrain CloneTerrain(this Terrain terrain, Vector3? position, Quaternion? rotation, bool cloneData = true)
        {
            var newTerrain = position.HasValue && rotation.HasValue
                ? Object.Instantiate(terrain, position.Value, rotation.Value)
                : Object.Instantiate(terrain);

            if (cloneData)
            {
                var data = Object.Instantiate(terrain.terrainData);

                newTerrain.terrainData = data;
                newTerrain.GetComponent<TerrainCollider>().terrainData = data;
            }

            return newTerrain;
        }

        /// <summary>
        /// Clones the color map.
        /// </summary>
        /// <param name="UEColors">The Unity Engine color map.</param>
        /// <returns></returns>
        public static Color[] CloneMap(this UEColor[] UEColors)
        {
            return InternalCloneColorMap(UEColors.Select(c => (Color)c).GetEnumerator(), UEColors.Length);
        }

        /// <summary>
        /// Clones the color map.
        /// </summary>
        /// <param name="colors">The color map.</param>
        /// <returns></returns>
        public static Color[] CloneMap(this Color[] colors)
        {
            return InternalCloneColorMap(colors.GetEnumerator(), colors.Length);
        }

        /// <summary>
        /// Internal: Clones the color map.
        /// </summary>
        /// <param name="colors">The colors.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        private static Color[] InternalCloneColorMap(IEnumerator colors, int length)
        {
            var newColors = new Color[length];

            var index = 0;
            while (colors.MoveNext())
            {
                newColors[index] = (Color)colors.Current;
                ++index;
            }

            return newColors;
        }

        /// <summary>
        /// Get the next float.
        /// </summary>
        /// <param name="rnd">The random.</param>
        /// <returns></returns>
        public static float NextFloat(this Random rnd)
        {
            return (float)rnd.NextDouble();
        }

        /// <summary>
        /// Gets the nearest cell.
        /// </summary>
        /// <param name="cells">The cells.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="cellSize">Size of the cell.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="dict">The dictionary.</param>
        /// <returns></returns>
        public static Point GetNearestCell(this Vector2[] cells, int x, int y, int cellSize, int width, int height, Dictionary<int, Vector2[]> dict)
        {
            var myIndex = GetGridIndex(x, y, width, cellSize);
            var neighbours = dict.ContainsKey(myIndex)
                ? dict[myIndex]
                : null;

            var vector = new Vector2(x, y);
            return (neighbours ?? cells) // ?.Where(cell => cell != default)
                .OrderBy(cell => Vector2.Distance(cell, vector))
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the grid bounding neighbours indexes.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="cellSize">Size of the cell.</param>
        /// <returns></returns>
        public static IEnumerable<int> GetGridBoundingNeighboursIndexes(int x, int y, int width, int height, int cellSize)
        {
            // myIndex
            yield return GetGridIndex(x, y, width, cellSize, out var nx, out var ny);

            // Corners
            if (nx == 0 && ny == 0)
            {
                yield return GetGridIndex(x + 1, y, width, cellSize);
                yield return GetGridIndex(x, y + 1, width, cellSize);
                yield return GetGridIndex(x + 1, y + 1, width, cellSize);
            }
            else if (nx == 0 && ny + cellSize >= height)
            {
                yield return GetGridIndex(x + 1, y, width, cellSize);
                yield return GetGridIndex(x, y - 1, width, cellSize);
                yield return GetGridIndex(x + 1, y - 1, width, cellSize);
            }
            else if (nx + cellSize >= width && ny == 0)
            {
                yield return GetGridIndex(x - 1, y, width, cellSize);
                yield return GetGridIndex(x, y + 1, width, cellSize);
                yield return GetGridIndex(x - 1, y + 1, width, cellSize);
            }
            else if (nx + cellSize >= width && ny + cellSize >= height)
            {
                yield return GetGridIndex(x - 1, y, width, cellSize);
                yield return GetGridIndex(x, y - 1, width, cellSize);
                yield return GetGridIndex(x - 1, y - 1, width, cellSize);
            }
            else
            {
                // 3x2
                if (nx == 0)
                {
                    yield return GetGridIndex(x, y - 1, width, cellSize);
                    yield return GetGridIndex(x + 1, y - 1, width, cellSize);
                    yield return GetGridIndex(x + 1, y, width, cellSize);
                    yield return GetGridIndex(x + 1, y + 1, width, cellSize);
                    yield return GetGridIndex(x, y + 1, width, cellSize);
                }
                else if (ny == 0)
                {
                    yield return GetGridIndex(x - 1, y, width, cellSize);
                    yield return GetGridIndex(x - 1, y + 1, width, cellSize);
                    yield return GetGridIndex(x, y + 1, width, cellSize);
                    yield return GetGridIndex(x + 1, y + 1, width, cellSize);
                    yield return GetGridIndex(x + 1, y, width, cellSize);
                }
                else if (nx + cellSize >= width)
                {
                    yield return GetGridIndex(x, y - 1, width, cellSize);
                    yield return GetGridIndex(x - 1, y - 1, width, cellSize);
                    yield return GetGridIndex(x - 1, y, width, cellSize);
                    yield return GetGridIndex(x - 1, y + 1, width, cellSize);
                    yield return GetGridIndex(x, y + 1, width, cellSize);
                }
                else if (ny + cellSize >= height)
                {
                    yield return GetGridIndex(x - 1, y, width, cellSize);
                    yield return GetGridIndex(x - 1, y - 1, width, cellSize);
                    yield return GetGridIndex(x, y - 1, width, cellSize);
                    yield return GetGridIndex(x + 1, y - 1, width, cellSize);
                    yield return GetGridIndex(x + 1, y, width, cellSize);
                }
                else
                {
                    // 3x3
                    yield return GetGridIndex(x + 1, y, width, cellSize);
                    yield return GetGridIndex(x, y + 1, width, cellSize);
                    yield return GetGridIndex(x, y - 1, width, cellSize);
                    yield return GetGridIndex(x - 1, y, width, cellSize);
                    yield return GetGridIndex(x + 1, y - 1, width, cellSize);
                    yield return GetGridIndex(x + 1, y + 1, width, cellSize);
                    yield return GetGridIndex(x - 1, y - 1, width, cellSize);
                    yield return GetGridIndex(x - 1, y + 1, width, cellSize);
                }
            }
        }

        /// <summary>
        /// Gets the index of the grid.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="cellSize">Size of the cell.</param>
        /// <returns></returns>
        internal static int GetGridIndex(int x, int y, int width, int cellSize)
        {
            return GetGridIndex(x, y, width, cellSize, out _, out _);
        }

        /// <summary>
        /// Gets the index of the grid.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="cellSize">Size of the cell.</param>
        /// <returns></returns>
        internal static int GetGridIndex(int x, int y, int width, int cellSize, out int nx, out int ny)
        {
            GetRoundedGridPosition(x, y, cellSize, out nx, out ny);
            return Pn(nx, ny, width);
        }

        /// <summary>
        /// Gets the index of the no overlapping grid.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="cellSize">Size of the cell.</param>
        /// <param name="nx">The nx.</param>
        /// <param name="ny">The ny.</param>
        /// <returns></returns>
        internal static int GetNoOverlappingGridIndex(this Dictionary<int, Vector2[]> dict, int x, int y, int width, int cellSize, out int nx, out int ny)
        {
            int index;
            var c = 0;

            do
            {
                GetRoundedGridPosition(NextCellX(x, c), NextCellY(y, c), cellSize, out nx, out ny);
                index = Pn(nx, ny, width);
                ++c;
            }
            while (dict.ContainsKey(index) && c <= 8);

            if (c > 8)
                return -1;

            return index;
        }

        /// <summary>
        /// Get the next the cell x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static int NextCellX(int x, int index)
        {
            switch (index)
            {
                case 0:
                    return x;

                case 1:
                    return x;

                case 2:
                    return x + 1;

                case 3:
                    return x - 1;

                case 4:
                    return x + 1;

                case 5:
                    return x + 1;

                case 6:
                    return x - 1;

                case 7:
                    return x - 1;

                case 8:
                    return x + 1;
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Gets the next the cell y.
        /// </summary>
        /// <param name="y">The y.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static int NextCellY(int y, int index)
        {
            switch (index)
            {
                case 0:
                    return y;

                case 1:
                    return y + 1;

                case 2:
                    return y - 1;

                case 3:
                    return y;

                case 4:
                    return y - 1;

                case 5:
                    return y + 1;

                case 6:
                    return y - 1;

                case 7:
                    return y + 1;

                case 8:
                    return y;
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Gets the rounded grid position.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="cellSize">Size of the cell.</param>
        /// <param name="nx">The nx.</param>
        /// <param name="ny">The ny.</param>
        private static void GetRoundedGridPosition(int x, int y, int cellSize, out int nx, out int ny)
        {
            nx = (int)MathHelper.MultipleOf(x, cellSize);
            ny = (int)MathHelper.MultipleOf(y, cellSize);
        }

        /// <summary>
        /// Saves the texture and clears it from memory.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="map">The map.</param>
        /// <param name="mapWidth">Width of the map.</param>
        /// <param name="mapHeight">Height of the map.</param>
        public static void SaveAndClear(string filePath, Color32[] map, int mapWidth, int mapHeight)
        {
            var _tex = new Texture2D(mapWidth, mapHeight);

            _tex.SetPixels32(map);
            _tex.Apply();

            File.WriteAllBytes(filePath, _tex.EncodeToPNG());

            Object.DestroyImmediate(_tex);
        }

        /// <summary>
        /// Converts the string representation of a number to an integer.
        /// </summary>
        /// <param name="p">The v.</param>
        /// <returns></returns>
        public static int ToInt(this Point p)
        {
            return ToInt(p.x, p.y);
        }

        /// <summary>
        /// Converts the string representation of a number to an integer.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static int ToInt(int x, int y)
        {
            if (x < short.MinValue || x > short.MaxValue || y < short.MinValue || y > short.MaxValue)
                throw new ArgumentOutOfRangeException();

            return (x << 16) | (y);
        }

        /// <summary>
        /// To the point.
        /// </summary>
        /// <param name="intPt">The int pt.</param>
        /// <returns></returns>
        public static Point ToPoint(this int intPt)
        {
            var x = (intPt & 0xFF00) >> 16;
            var y = intPt & 0x00FF;

            return new Point(x, y);
        }

        /// <summary>
        /// To the long.
        /// </summary>
        /// <param name="p">The v.</param>
        /// <returns></returns>
        public static long ToLong(this Point p)
        {
            return ToLong(p.x, p.y);
        }

        /// <summary>
        /// To the long.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        public static long ToLong(int x, int y)
        {
            // Not needed due to type
            //if (x < short.MinValue || x > short.MaxValue || y < short.MinValue || y > short.MaxValue)
            //    throw new ArgumentOutOfRangeException();

            return (x << 32) | (y);
        }

        /// <summary>
        /// To the point.
        /// </summary>
        /// <param name="longPt">The long pt.</param>
        /// <returns></returns>
        public static PointL ToPoint(this long longPt)
        {
            var x = (longPt & 0xFFFF0000) >> 32;
            var y = longPt & 0x0000FFFF;

            return new PointL(x, y);
        }

        public static int InvertY(this int y, int mapHeight)
            => mapHeight - y - 1;

        /// <summary>
        /// Gets the active child count.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static int GetActiveChildCount(this Transform t)
        {
            return t.GetComponentsInChildren<Transform>().Length;
        }

        /// <summary>
        /// Gets the distance.
        /// </summary>
        /// <param name="x0">The x0.</param>
        /// <param name="y0">The y0.</param>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <returns></returns>
        public static float GetDistance(float x0, float y0, float x1, float y1)
        {
            var deltaX = x1 - x0;
            var deltaY = y1 - y0;

            return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        /// <summary>
        /// Gets the distance.
        /// </summary>
        /// <param name="x0">The x0.</param>
        /// <param name="y0">The y0.</param>
        /// <param name="z0">The z0.</param>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="z1">The z1.</param>
        /// <returns></returns>
        public static float GetDistance(float x0, float y0, float z0, float x1, float y1, float z1)
        {
            var deltaX = x1 - x0;
            var deltaY = y1 - y0;
            var deltaZ = z1 - z0;

            return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        }

        /// <summary>
        /// Finds the closest target.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public static Transform FindClosestTarget(this Transform t, Vector3 position)
        {
            var childs = t.GetComponentsInChildren<Transform>();

            return childs
                .OrderBy(tr => (tr.position - position).sqrMagnitude)
                .FirstOrDefault();
        }

        public static bool NextBool(this Random rnd, double midValue = 0.5)
        {
            return rnd.NextDouble() < midValue;
        }

        public static int ToOne(this int v)
        {
            var rv = v == 0 ? 0 : v / v;
            if (v < 0) rv *= -1;
            return rv;
        }

        public static Vector3 GetHeightForPoint(this Vector2 v, float raycastDistance = 0, bool tryFromAbove = true)
        {
            var position = new Vector3(v.x, 1000, v.y);
            //int raycastLayerMask = ~PedManager.Instance.groundFindingIgnoredLayerMask;

            var raycastPositions = new List<Vector3> { position };   //transform.position - Vector3.up * characterController.height;
            var raycastDirections = new List<Vector3> { Vector3.down };
            //var customMessages = new List<string> { "from center" };

            if (tryFromAbove)
            {
                raycastPositions.Add(position + Vector3.up * raycastDistance);
                raycastDirections.Add(Vector3.down);
                //customMessages.Add("from above");
            }

            for (var i = 0; i < raycastPositions.Count; i++)
            {
                //if (Physics.Raycast(raycastPositions[i], raycastDirections[i], out hit, raycastDistance, raycastLayerMask))

                RaycastHit hit;
                var hitBool = raycastDistance > 0
                    ? Physics.Raycast(raycastPositions[i], raycastDirections[i], out hit, raycastDistance)
                    : Physics.Raycast(raycastPositions[i], raycastDirections[i], out hit);

                if (hitBool)
                    return hit.point;
            }

            return new Vector3(v.x, 0, v.y);
        }

        public static Dictionary<TKey, TElement> SafeToDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector,
            IEqualityComparer<TKey> comparer = null)
        {
            var dictionary = new Dictionary<TKey, TElement>(comparer);

            if (source == null)
                return dictionary;

            foreach (var element in source)
                dictionary[keySelector(element)] = elementSelector(element);

            return dictionary;
        }

        public static int GetKey(this Point p, bool inverted = true)
        {
            return inverted
                ? P(p.x, p.y, mapWidth, mapHeight)
                : Pn(p.x, p.y, mapWidth);
        }

        ///// <summary>
        /////     Creates the texture.
        ///// </summary>
        ///// <param name="width">The width.</param>
        ///// <param name="height">The height.</param>
        ///// <param name="color">The color.</param>
        ///// <returns></returns>
        //public static Texture2D CreateTexture(int width, int height, UEColor color)
        //{
        //    var pixels = new UEColor[width * height];

        //    for (var i = 0; i < pixels.Length; i++)
        //        pixels[i] = color;

        //    var texture = new Texture2D(width, height);
        //    texture.SetPixels(pixels);
        //    texture.Apply();

        //    return texture;
        //}

        ///// <summary>
        /////     To the texture.
        ///// </summary>
        ///// <param name="color">The color.</param>
        ///// <param name="width">The width.</param>
        ///// <param name="height">The height.</param>
        ///// <returns></returns>
        //public static Texture2D ToTexture(this UEColor color, int width, int height)
        //{
        //    return CreateTexture(width, height, color);
        //}

        ///// <summary>
        /////     To the texture.
        ///// </summary>
        ///// <param name="color">The color.</param>
        ///// <returns></returns>
        //public static Texture2D ToTexture(this UEColor color)
        //{
        //    return CreateTexture(1, 1, color);
        //}

        ///// <summary>
        /////     To the texture.
        ///// </summary>
        ///// <param name="color">The color.</param>
        ///// <returns></returns>
        //public static Texture2D ToTexture(this Color32 color)
        //{
        //    return ((UEColor)color).ToTexture();
        //}

        // Serialize collection of any type to a byte stream

        public static byte[] Serialize<T>(this T obj, FloatProgressChangedEventHandler callback)
        {
            //if (callback == null) throw new ArgumentNullException(nameof(callback));

            using (var stream = new ProgressStream())
            {
                stream.ProgressChanged += callback;

                var binSerializer = new BinaryFormatter();
                binSerializer.Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        // DSerialize collection of any type to a byte stream

        public static T Deserialize<T>(this byte[] serializedObj, FloatProgressChangedEventHandler callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            T obj;
            using (var stream = new ProgressStream(serializedObj))
            {
                stream.ProgressChanged += callback;

                var binSerializer = new BinaryFormatter();
                obj = (T)binSerializer.Deserialize(stream);
            }
            return obj;
        }

        public static void ReadDataFromWebAsync(this string url, Action<byte[]> result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            Func<byte[]> readAsync = () => new System.Net.WebClient().DownloadData(url);
            readAsync.RunAsync(result);
        }

#if UNITY_EDITOR

        /// <summary>
        ///     Opens the clipboard.
        /// </summary>
        /// <param name="hWndNewOwner">The h WND new owner.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

        /// <summary>
        ///     Closes the clipboard.
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        internal static extern bool CloseClipboard();

        /// <summary>
        ///     Sets the clipboard data.
        /// </summary>
        /// <param name="uFormat">The u format.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        internal static extern bool SetClipboardData(uint uFormat, IntPtr data);

        /// <summary>
        ///     Copies to clipboard.
        /// </summary>
        /// <param name="text">The text.</param>
        public static void CopyToClipboard(this string text)
        {
            OpenClipboard(IntPtr.Zero);
            var ptr = Marshal.StringToHGlobalUni(text);
            SetClipboardData(13, ptr);
            CloseClipboard();
            Marshal.FreeHGlobal(ptr);
        }

#endif
    }
}