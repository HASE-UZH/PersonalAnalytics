using System;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowRecommender;
using WindowRecommender.Graphics;
using WindowRecommender.Native;
using WindowRecommender.Native.Fakes;
using WindowRecommender.Util;

namespace WindowRecommenderTests.Util
{
    [TestClass]
    public class WindowUtilsTest
    {
        [TestMethod]
        public void TestGetCorrectedWindowRectangle_Correct()
        {
            using (ShimsContext.Create())
            {
                ShimNativeMethods.GetWindowRectangleIntPtr = ptr => new RECT(10, 10, 20, 20);
                var windowRecord = new WindowRecord(new IntPtr(1), "", "devenv");
                Assert.AreEqual(new Rectangle(10, 10, 20, 20), WindowUtils.GetCorrectedWindowRectangle(windowRecord));
            }
        }

        [TestMethod]
        public void TestGetCorrectedWindowRectangle_Correctable()
        {
            using (ShimsContext.Create())
            {
                ShimNativeMethods.GetWindowRectangleIntPtr = ptr => new RECT(2, 10, 28, 28);
                var windowRecord = new WindowRecord(new IntPtr(1), "", "explorer");
                Assert.AreEqual(new Rectangle(10, 10, 20, 20), WindowUtils.GetCorrectedWindowRectangle(windowRecord));
            }
        }
    }
}
