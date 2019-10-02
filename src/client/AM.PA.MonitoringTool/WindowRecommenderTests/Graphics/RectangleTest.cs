using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowRecommender.Graphics;
using WindowRecommender.Native;

namespace WindowRecommenderTests.Graphics
{
    [TestClass]
    public class RectangleTest
    {
        [TestMethod]
        public void TestIntersectsWith_BottomRight()
        {
            var a = new Rectangle(0, 0, 10, 10);
            var b = new Rectangle(5, 5, 15, 15);
            Assert.IsTrue(a.IntersectsWith(b));
        }

        [TestMethod]
        public void TestIntersectsWith_None()
        {
            var a = new Rectangle(0, 0, 10, 10);
            var b = new Rectangle(15, 15, 20, 20);
            Assert.IsFalse(a.IntersectsWith(b));
        }

        [TestMethod]
        public void TestIntersectsWith_Same()
        {
            var a = new Rectangle(0, 0, 10, 10);
            var b = new Rectangle(0, 0, 10, 10);
            Assert.IsTrue(a.IntersectsWith(b));
        }

        [TestMethod]
        public void TestIntersectsWith_Left()
        {
            var a = new Rectangle(5, 5, 10, 10);
            var b = new Rectangle(0, 6, 6, 8);
            Assert.IsTrue(a.IntersectsWith(b));
        }

        [TestMethod]
        public void TestEquals_Equal()
        {
            Assert.AreEqual(new Rectangle(), new Rectangle());
            Assert.AreEqual(new Rectangle(1, 1, 2, 2), new Rectangle(1, 1, 2, 2));
        }

        [TestMethod]
        public void TestEquals_NotEqual()
        {
            Assert.IsFalse(new Rectangle().Equals(null));
            Assert.AreNotEqual(new Rectangle(), 1);
            Assert.AreNotEqual(new Rectangle(), new Rectangle(1, 1, 1, 1));
            Assert.AreNotEqual(new Rectangle(1, 1, 2, 2), new Rectangle(2, 2, 3, 3));
        }

        [TestMethod]
        public void TestGetHashCode_Equal()
        {
            Assert.AreEqual(new Rectangle().GetHashCode(), new Rectangle().GetHashCode());
            Assert.AreEqual(new Rectangle(1, 1, 2, 2).GetHashCode(), new Rectangle(1, 1, 2, 2).GetHashCode());
        }

        [TestMethod]
        public void TestGetHashCode_NotEqual()
        {
            Assert.AreNotEqual(new Rectangle().GetHashCode(), new Rectangle(1, 1, 1, 1).GetHashCode());
            Assert.AreNotEqual(new Rectangle(1, 1, 2, 2).GetHashCode(), new Rectangle(2, 2, 3, 3).GetHashCode());
        }

        [TestMethod]
        public void TestTranslatedRelative()
        {
            var rectangle1 = new Rectangle(1, 1, 1, 1);
            var rectangle2 = new Rectangle(2, 2, 2, 2);
            Assert.AreEqual(rectangle1, Rectangle.TranslatedRelative(new Rectangle(), rectangle1));
            Assert.AreEqual(rectangle1, Rectangle.TranslatedRelative(rectangle1, rectangle2));
            Assert.AreEqual(new Rectangle(), Rectangle.TranslatedRelative(rectangle1, rectangle1));
        }

        [TestMethod]
        public void TestCast_RECT()
        {
            var rectangle = new Rectangle(1, 2, 3, 4);
            var rect = (RECT)rectangle;
            Assert.AreEqual(rectangle, (Rectangle)rect);
        }
    }
}
