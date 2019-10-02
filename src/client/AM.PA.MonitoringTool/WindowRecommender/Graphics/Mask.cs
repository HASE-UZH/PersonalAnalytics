using System;
using System.Collections.Generic;
using System.Linq;

namespace WindowRecommender.Graphics
{
    internal static class Mask
    {
        public static IEnumerable<Rectangle> Cut(Rectangle screenRectangle, IEnumerable<(Rectangle rectangle, bool show)> windowInfos)
        {
            return windowInfos.Reverse().Aggregate(new[] { screenRectangle }.AsEnumerable(), (rectangles, windowInfo) =>
            {
                if (windowInfo.show)
                {
                    return rectangles.SelectMany(rectangle => Cut(rectangle, windowInfo.rectangle));
                }
                var rectangleList = rectangles.ToList();
                return rectangleList.Concat(Cut(windowInfo.rectangle, rectangleList));
            });
        }

        internal static IEnumerable<Rectangle> Cut(Rectangle screen, IEnumerable<Rectangle> windows)
        {
            return windows.Aggregate(new[] { screen }.AsEnumerable(), (rectangles, window) =>
            {
                return rectangles.SelectMany(rectangle => Cut(rectangle, window));
            });
        }

        /// <summary>
        /// Cut a rectangle out of another source rectangle.
        /// </summary>
        /// <param name="source">Rectangle to cut up.</param>
        /// <param name="cover">Covering Rectangle.</param>
        /// <returns>List of rectangle required for source minus cover.</returns>
        internal static List<Rectangle> Cut(Rectangle source, Rectangle cover)
        {
            var rects = new List<Rectangle>();
            if (source.IntersectsWith(cover))
            {
                if (source.Left < cover.Left)
                {
                    rects.Add(new Rectangle(source.Left, source.Top, cover.Left, Math.Min(source.Bottom, cover.Bottom)));
                }
                if (source.Top < cover.Top)
                {
                    rects.Add(new Rectangle(Math.Max(source.Left, cover.Left), source.Top, source.Right, cover.Top));
                }
                if (source.Right > cover.Right)
                {
                    rects.Add(new Rectangle(cover.Right, Math.Max(source.Top, cover.Top), source.Right, source.Bottom));
                }
                if (source.Bottom > cover.Bottom)
                {
                    rects.Add(new Rectangle(source.Left, cover.Bottom, Math.Min(source.Right, cover.Right), source.Bottom));
                }
            }
            else
            {
                rects.Add(source);
            }
            return rects;
        }
    }
}
