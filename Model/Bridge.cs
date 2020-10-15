using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using uzSurfaceMapper.Model.Enums;
using uzSurfaceMapper.Extensions;
using UnityEngine;

namespace uzSurfaceMapper.Model
{
    /// <summary>
    ///     Bridge class
    /// </summary>
    [Serializable]
    public class Bridge
    {
        /// <summary>
        ///     The distances
        /// </summary>
        public static List<Tuple<Bridge, Vector2>> distances = new List<Tuple<Bridge, Vector2>>();

        /// <summary>
        ///     The index
        /// </summary>
        private static int index;

        /// <summary>
        ///     The index
        /// </summary>
        private readonly int Index;

        /// <summary>
        ///     The minimum bound
        /// </summary>
        private Point minBound, maxBound, center;

        /// <summary>
        ///     The pixels
        /// </summary>
        private readonly List<Point> pixels = new List<Point>();

        /// <summary>
        ///     The rectangle
        /// </summary>
        private Rect rectangle;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Bridge" /> class.
        /// </summary>
        public Bridge()
        {
            Index = index;

            if (Monitor.IsEntered(index))
                ++index;
            else
                Interlocked.Increment(ref index);
        }

        /// <summary>
        ///     Gets the center.
        /// </summary>
        /// <value>
        ///     The center.
        /// </value>
        public Point Center
        {
            get
            {
                if (center == null)
                    CalculateCenter();

                return center;
            }
            set => center = value;
        }

        /// <summary>
        ///     Gets the rectangle.
        /// </summary>
        /// <value>
        ///     The rectangle.
        /// </value>
        public Rect Rectangle
        {
            get
            {
                if (rectangle == null)
                    rectangle = new Rect(minBound, maxBound);

                return rectangle;
            }
            set => rectangle = value;
        }

        /// <summary>
        ///     Gets the pixel count.
        /// </summary>
        /// <value>
        ///     The pixel count.
        /// </value>
        public int PixelCount => pixels.Count;

        /// <summary>
        ///     Adds the pixel.
        /// </summary>
        /// <param name="p">The p.</param>
        public void AddPixel(Point p)
        {
            pixels.Add(p);
        }

        /// <summary>
        ///     Calculates the center.
        /// </summary>
        private void CalculateCenter()
        {
            minBound = new Point(pixels.Min(p => p.x), pixels.Min(p => p.y));
            maxBound = new Point(pixels.Max(p => p.x), pixels.Max(p => p.y));

            center = Vector2.Lerp(minBound, maxBound, .5f);
        }

        /// <summary>
        ///     Finishes this instance.
        /// </summary>
        public void Finish()
        {
            CalculateCenter();
            pixels.Clear();
        }

        /// <summary>
        ///     Valids the outer bounds.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="maxDist">The maximum dist.</param>
        /// <returns></returns>
        /// <exception cref="Exception">You must call 'Finish' method before this.</exception>
        public bool ValidOuterBounds(Color[] source, int width, int height, float maxDist = 2500)
        {
            if (center == null)
                throw new Exception("You must call 'Finish' method before this.");

            var xAxisTask = new Task<Vector2>(() =>
            {
                var currentX = minBound.x - 1;

                try
                {
                    do
                    {
                        --currentX;
                    } while (source[F.P(currentX, minBound.y, width, height)] == GroundType.Water.GetColor() ||
                             rectangle.Contains(new Vector2(currentX, minBound.y)));
                }
                catch
                {
                    return new Vector2(currentX, minBound.y);
                }

                return new Vector2(currentX, minBound.y);
            });

            var yAxisTask = new Task<Vector2>(() =>
            {
                var currentY = minBound.y - 1;

                try
                {
                    do
                    {
                        --currentY;
                    } while (source[F.P(minBound.x, currentY, width, height)] == GroundType.Water.GetColor() ||
                             rectangle.Contains(new Vector2(minBound.x, currentY)));
                }
                catch
                {
                    return new Vector2(minBound.x, currentY);
                }

                return new Vector2(minBound.x, currentY);
            });

            xAxisTask.Start();
            yAxisTask.Start();

            while (!(xAxisTask.IsCompleted && yAxisTask.IsCompleted))
            {
            }

            var dX = Vector2.Distance(xAxisTask.Result, Center);
            var dY = Vector2.Distance(yAxisTask.Result, Center);

            if (false)
            {
                var d = new Vector2(dX, dY);

                Debug.Log($"{ToString()} | D: {d.SimplifiedToString()} --> {d.x + d.y:F2}");

                distances.Add(new Tuple<Bridge, Vector2>(this, d));
            }

            return dX + dY <= maxDist;
        }

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Index: {Index} | Center: {Center}";
        }
    }
}