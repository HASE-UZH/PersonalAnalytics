using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowRecommender.Util;

namespace WindowRecommenderTests.Util
{
    [TestClass]
    public class TextUtilsTest
    {
        [TestMethod]
        public void TestPrepareTitle_Replace()
        {
            var tests = new[]
            {
                (input: "", expected: new string[0]),
                (input: "x-x", expected: new[] {"x-x"}),
                (input: "x'x", expected: new[] {"x'x"}),
                (input: "xyz", expected: new[] {"xyz"}),
                (input: "öüß", expected: new[] {"öüß"}),
                (input: "123", expected: new string[0]),
                (input: "12 ??", expected: new string[0]),
                (input: "xx1yy", expected: new[] {"xx", "yy"}),
                (input: "xx_yy", expected: new[] {"xx", "yy"}),
            };
            foreach (var (input, expected) in tests)
            {
                var actual = TextUtils.PrepareTitle(input).ToArray();
                CollectionAssert.AreEqual(expected, actual, $"input: {input}, expected: '{string.Join(", ", expected)}', actual: '{string.Join(", ", actual)}'");
            }
        }

        [TestMethod]
        public void TestPrepareTitle_Split()
        {
            var tests = new[]
            {
                (input: "", expected: new string[0]),
                (input: " ", expected: new string[0]),
                (input: "  ", expected: new string[0]),
                (input: " xx ", expected: new[] {"xx"}),
                (input: "  xx", expected: new[] {"xx"}),
                (input: "xx yy", expected: new[] {"xx", "yy"}),
            };
            foreach (var (input, expected) in tests)
            {
                var actual = TextUtils.PrepareTitle(input).ToArray();
                CollectionAssert.AreEqual(expected, actual, $"input: {input}, expected: '{string.Join(", ", expected)}', actual: '{string.Join(", ", actual)}'");
            }
        }

        [TestMethod]
        public void TestPrepareTitle_MinLength()
        {
            var tests = new[]
            {
                (input: "", expected: new string[0]),
                (input: "x", expected: new string[0]),
                (input: "-", expected: new string[0]),
                (input: "xx", expected: new[] {"xx"}),
            };
            foreach (var (input, expected) in tests)
            {
                var actual = TextUtils.PrepareTitle(input).ToArray();
                CollectionAssert.AreEqual(expected, actual, $"input: {input}, expected: '{string.Join(", ", expected)}', actual: '{string.Join(", ", actual)}'");
            }
        }

        [TestMethod]
        public void TestPrepareTitle_StopWords()
        {
            CollectionAssert.AreEqual(new[] { "home" }, TextUtils.PrepareTitle("my home").ToArray());
            CollectionAssert.AreEqual(new[] { "home" }, TextUtils.PrepareTitle("My Home").ToArray());
        }

        [TestMethod]
        public void TestPrepareTitle_ToLower()
        {
            CollectionAssert.AreEqual(new[] { "xx" }, TextUtils.PrepareTitle("XX").ToArray());
        }
    }
}