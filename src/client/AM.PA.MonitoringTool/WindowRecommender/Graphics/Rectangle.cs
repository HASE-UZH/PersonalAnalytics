using WindowRecommender.Native;
using GameOverlayRectangle = GameOverlay.Drawing.Rectangle;

namespace WindowRecommender.Graphics
{
    internal struct Rectangle
    {
        /// <summary>
        /// The x-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        internal readonly int Left;

        /// <summary>
        /// The y-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        internal readonly int Top;

        /// <summary>
        /// The x-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        internal readonly int Right;

        /// <summary>
        /// The y-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        internal readonly int Bottom;

        internal Rectangle(float left, float top, float right, float bottom)
        {
            Left = (int)left;
            Top = (int)top;
            Right = (int)right;
            Bottom = (int)bottom;
        }

        internal bool IntersectsWith(Rectangle rectangle)
        {
            if (Left < rectangle.Right && rectangle.Left < Right && Top < rectangle.Bottom)
            {
                return rectangle.Top < Bottom;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var r = (Rectangle)obj;
            return Left == r.Left && Top == r.Top && Right == r.Right && Bottom == r.Bottom;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Left;
                hashCode = (hashCode * 397) ^ Top;
                hashCode = (hashCode * 397) ^ Right;
                hashCode = (hashCode * 397) ^ Bottom;
                return hashCode;
            }
        }

        public static Rectangle TranslatedRelative(Rectangle target, Rectangle source)
        {
            return new Rectangle(source.Left - target.Left, source.Top - target.Top, source.Right - target.Left, source.Bottom - target.Top);
        }

        public static implicit operator RECT(Rectangle rectangle)
        {
            return new RECT(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        }

        public static explicit operator Rectangle(RECT rect)
        {
            return new Rectangle(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        public static implicit operator GameOverlayRectangle(Rectangle rectangle)
        {
            return new GameOverlayRectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        }

        public static explicit operator Rectangle(GameOverlayRectangle rectangle)
        {
            return new Rectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        }
    }
}
