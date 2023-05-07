using Backstreets.FOV.Geometry;
using NUnit.Framework;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Backstreets.Tests
{
    internal class IntersectionMathTests
    {
        [TestCaseSource(nameof(IntersectionTestcases))]
        public void TestIntersections((Line a, Line b, float2? result) testcase)
        {
            float2? result = LineMath.GetIntersection(testcase.a, testcase.b);
            
            Assert.That(result, Is.EqualTo(testcase.result));
        }

        private static readonly (Line a, Line b, float2? result)[] IntersectionTestcases =
        {
            (
                a: A.Line(float2(0, 0), float2(1, 2)),
                b: A.Line(float2(3, 0), float2(2, 2)),
                result: float2(1.5f, 3)
            ),
        };

        [TestCaseSource(nameof(ProjectionTestcases))]
        public void TestProjectionFromOrigin((Line line, float2 direction, float2? result) testcase)
        {
            float2? result = LineMath.ProjectFromOrigin(testcase.line, testcase.direction);
            
            Assert.That(result, Is.EqualTo(testcase.result));
        }

        private static readonly (Line line, float2 direction, float2? result)[] ProjectionTestcases =
        {
            (
                line: A.Line(float2(-1, 5), float2(10, 5)),
                direction: float2(0, 1),
                result: float2(0, 5)
            ),
            (
                line: A.Line(float2(-1, 5), float2(10, 5)),
                direction: float2(100, 100),
                result: float2(5, 5)
            ),
            (
                line: A.Line(float2(-1, 1), float2(1, 1)),
                direction: float2(1, 0),
                result: null
            ),
        };
    }
}