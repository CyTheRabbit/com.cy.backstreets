using System;
using static Backstreets.Viewport.ViewportMath;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.Viewport.Jobs
{
    [BurstCompile]
    internal struct GetVisibleEdgesJob : IJob
    {
        public GetVisibleEdgesJob(
            NativeArray<ViewportLine> inputEdges,
            NativeList<ViewportLine> outputEdges)
        {
            this.inputEdges = inputEdges;
            this.outputEdges = outputEdges;
        }

        [ReadOnly] private readonly NativeArray<ViewportLine> inputEdges;
        [WriteOnly] private NativeList<ViewportLine> outputEdges;

        public void Execute()
        {
            // Assume input edges are sorted by left vertex

            NativeList<ViewportLine> currentLines = new(initialCapacity: 16, Allocator.Temp);

            ViewportPoint? currentPointer = inputEdges[0].Left;
            int currentIndex = 0;

            while (currentPointer is { } pointer)
            {
                UpdateOngoingLines(in inputEdges, in pointer, ref currentIndex, ref currentLines);
                ViewportLine? closestLine = FindClosestLine(in currentLines, in pointer);
                ViewportPoint? nextPointer = FindNextPoint(in inputEdges, in currentLines, in currentIndex);
                if (closestLine is { } line && nextPointer is { } endPointer)
                {
                    ViewportPoint lineStart = ProjectFromOrigin(line, pointer) ?? throw new ArithmeticException();
                    ViewportPoint lineEnd = ProjectFromOrigin(line, endPointer) ?? throw new ArithmeticException();
                    ViewportLine visibleLine = new(lineStart, lineEnd);
                    outputEdges.Add(visibleLine);
                }

                currentPointer = nextPointer;
            }

            currentLines.Dispose();
        }

        private ViewportPoint? FindNextPoint(in NativeArray<ViewportLine> mayBegin, in NativeList<ViewportLine> mayEnd, in int readIndex)
        {
            bool hasUnreadLines = readIndex < mayBegin.Length;
            bool hasOngoingLines = !mayEnd.IsEmpty;
            if (!(hasUnreadLines || hasOngoingLines)) return null;
            ViewportPoint next = hasUnreadLines ? mayBegin[readIndex].Left
                : mayEnd[0].Right;

            foreach (ViewportLine ongoingLine in mayEnd)
            {
                if (ongoingLine.Right < next)
                {
                    next = ongoingLine.Right;
                }
            }

            return next;
        }

        private ViewportLine? FindClosestLine(in NativeList<ViewportLine> ongoingLines, in ViewportPoint angle)
        {
            (ViewportLine Line, ViewportPoint Point)? closest = null; 
            foreach (ViewportLine line in ongoingLines)
            {
                if (ProjectFromOrigin(line, angle) is not { } projected) throw new ArithmeticException();
                if (closest is { Point: var closestProjected } && closestProjected.SqrDistance < projected.SqrDistance) continue;

                closest = (line, projected);
            }

            return closest?.Line;
        }

        private void UpdateOngoingLines(
            in NativeArray<ViewportLine> input,
            in ViewportPoint pointer,
            ref int readIndex,
            ref NativeList<ViewportLine> output)
        {
            while (readIndex < input.Length && input[readIndex].Left <= pointer)
            {
                output.Add(input[readIndex++]);
            }
            
            for (int i = output.Length - 1; i >= 0; i--)
            {
                if (output[i].Right > pointer) continue;
                output.RemoveAt(i);
            }
        }
    }
}