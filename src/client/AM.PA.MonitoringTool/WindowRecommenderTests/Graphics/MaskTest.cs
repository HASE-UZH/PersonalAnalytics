using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowRecommender.Graphics;

namespace WindowRecommenderTests.Graphics
{
    [TestClass]
    public class MaskTest
    {
        [TestMethod]
        public void TestCutSingle_NoOverlap()
        {
            var source = new Rectangle(0, 0, 1, 1);
            var cover = new Rectangle(2, 2, 3, 3);
            var rects = Mask.Cut(source, cover);
            CollectionAssert.AreEquivalent(new List<Rectangle> { source }, rects);
        }

        [TestMethod]
        public void TestCutSingle_Corner()
        {
            var source = new Rectangle(1, 0, 3, 2);
            var cover = new Rectangle(0, 1, 2, 3);
            var rects = Mask.Cut(source, cover);
            CollectionAssert.AreEquivalent(new List<Rectangle>
            {
                new Rectangle(1, 0, 3, 1),
                new Rectangle(2, 1, 3, 2)
            }, rects);
        }

        [TestMethod]
        public void TestCutSingle_Side()
        {
            var source = new Rectangle(1, 0, 3, 3);
            var cover = new Rectangle(0, 1, 2, 2);
            var rects = Mask.Cut(source, cover);
            CollectionAssert.AreEquivalent(new List<Rectangle>
            {
                new Rectangle(1, 0, 3, 1),
                new Rectangle(2, 1, 3, 3),
                new Rectangle(1, 2, 2, 3)
            }, rects);
        }

        [TestMethod]
        public void TestCutSingle_Inside()
        {
            var source = new Rectangle(0, 0, 3, 3);
            var cover = new Rectangle(1, 1, 2, 2);
            var rects = Mask.Cut(source, cover);
            CollectionAssert.AreEquivalent(new List<Rectangle>
            {
                new Rectangle(0, 0, 1, 2),
                new Rectangle(1, 0, 3, 1),
                new Rectangle(2, 1, 3, 3),
                new Rectangle(0, 2, 2, 3)
            }, rects);
        }

        [TestMethod]
        public void TestCutMultiple_NoOverlap()
        {
            var source = new Rectangle(0, 0, 1, 1);
            var covers = new List<Rectangle>
            {
                new Rectangle(1, 1, 3, 3),
                new Rectangle(2, 2, 4, 4)
            };
            var rects = Mask.Cut(source, covers);
            CollectionAssert.AreEquivalent(new List<Rectangle> { source }, rects.ToList());
        }

        [TestMethod]
        public void TestCutMultiple_TwoCorners()
        {
            var source = new Rectangle(1, 1, 4, 4);
            var covers = new List<Rectangle>
            {
                new Rectangle(0, 3, 2, 5),
                new Rectangle(3, 0, 5, 2)
            };
            var rects = Mask.Cut(source, covers);
            CollectionAssert.AreEquivalent(new List<Rectangle>
            {
                new Rectangle(1, 1, 3, 2),
                new Rectangle(1, 2, 4, 3),
                new Rectangle(2, 3, 4, 4)
            }, rects.ToList());
        }

        [TestMethod]
        public void TestCutShow_HideVisible()
        {
            var source = new Rectangle(0, 0, 1, 1);
            var covers = new List<(Rectangle rect, bool show)>
            {
                (new Rectangle(0, 0, 1, 1), false),
                (new Rectangle(0, 0, 1, 1), true)
            };
            var rects = Mask.Cut(source, covers);
            CollectionAssert.AreEquivalent(new List<Rectangle>
            {
                new Rectangle(0, 0, 1, 1)
            }, rects.ToList());
        }

        [TestMethod]
        public void TestCutShow_ShowHideShow()
        {
            var source = new Rectangle(0, 0, 1, 1);
            var covers = new List<(Rectangle rect, bool show)>
            {
                (new Rectangle(0, 0, 1, 1), true),
                (new Rectangle(0, 0, 1, 1), false),
                (new Rectangle(0, 0, 1, 1), true)
            };
            var rects = Mask.Cut(source, covers);
            CollectionAssert.AreEquivalent(new List<Rectangle>(), rects.ToList());
        }

        [TestMethod]
        public void TestCutNegative_Corner()
        {
            var source = new Rectangle(0, 0, 4, 4);
            var covers = new List<(Rectangle rect, bool show)>
            {
                (new Rectangle(1, 1, 3, 3), false),
                (new Rectangle(2, 2, 5, 5), true)
            };
            var rects = Mask.Cut(source, covers);
            CollectionAssert.AreEquivalent(new List<Rectangle>
            {
                new Rectangle(2, 0, 4, 2),
                new Rectangle(0, 0, 2, 4),
                new Rectangle(2, 2, 3, 3)
            }, rects.ToList());
        }
    }
}
