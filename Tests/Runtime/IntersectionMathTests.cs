using Backstreets.Viewport;
using NUnit.Framework;
using UnityEngine;

namespace Backstreets.Tests
{
    internal class IntersectionMathTests
    {
        [TestCaseSource(nameof(IntersectionTestcases))]
        public void TestIntersections((ViewportLine a, ViewportLine b, Vector2? result) testcase)
        {
            Vector2? result = ViewportMath.GetIntersection(testcase.a, testcase.b);
            
            Assert.That(result, Is.EqualTo(testcase.result));
        }

        private static readonly (ViewportLine a, ViewportLine b, Vector2? result)[] IntersectionTestcases =
        {
            (
                a: A.Line(A.Point(0, 0), A.Point(1, 2)),
                b: A.Line(A.Point(3, 0), A.Point(2, 2)),
                result: new Vector2(1.5f, 3)
            ),
        };

        [TestCaseSource(nameof(ProjectionTestcases))]
        public void TestProjectionFromOrigin((ViewportLine line, Vector2 direction, Vector2? result) testcase)
        {
            Vector2? result = ViewportMath.ProjectFromOrigin(testcase.line, testcase.direction);
            
            Assert.That(result, Is.EqualTo(testcase.result));
        }

        private static readonly (ViewportLine line, Vector2 direction, Vector2? result)[] ProjectionTestcases =
        {
            (
                line: A.Line(A.Point(-1, 5), A.Point(10, 5)),
                direction: new Vector2(0, 1),
                result: new Vector2(0, 5)
            ),
            (
                line: A.Line(A.Point(-1, 5), A.Point(10, 5)),
                direction: new Vector2(100, 100),
                result: new Vector2(5, 5)
            ),
            (
                line: A.Line(A.Point(-1, 1), A.Point(1, 1)),
                direction: new Vector2(1, 0),
                result: null
            ),
        };
    }
}