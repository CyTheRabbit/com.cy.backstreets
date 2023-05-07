using Backstreets.Data;
using Backstreets.FOV.Geometry;
using Unity.Collections;
using Unity.Jobs;

namespace Backstreets.FOV
{
    public struct PotentiallyVisibleSet : INativeDisposable
    {
        public PocketID OriginPocket;
        public NativeList<Window> VisibleWindows;
        public NativeParallelHashSet<PocketID> VisiblePockets; 

        public PotentiallyVisibleSet(PocketID originPocket, int portalsCapacity, int pocketsCapacity, Allocator allocator)
        {
            OriginPocket = originPocket;
            VisibleWindows = new NativeList<Window>(portalsCapacity, allocator);
            VisiblePockets = new NativeParallelHashSet<PocketID>(pocketsCapacity, allocator);
            VisiblePockets.Add(OriginPocket);
        }

        public void TryAdd(Portal portal, Window throughWindow)
        {
            if (throughWindow.Depth >= DepthLimit) return;

            if (IsBefore(line: portal.Edge, window: throughWindow.ExitBound)) return;

            Line? overlap = LineMath.ClipLine(portal.Edge, throughWindow.ExitBound);
            if (overlap is { } nearBound)
            {
                Add(new Window
                {
                    Portal = portal,
                    ExitBound = nearBound,
                    Depth = throughWindow.Depth + 1,
                    ParentIndex = throughWindow.Index
                });
            }
        }

        public void TryAdd(Portal portal)
        {
            if (IsVisible(portal.Edge))
            {
                Add(new Window
                {
                    Portal = portal,
                    ExitBound = portal.Edge,
                    Depth = 1,
                    ParentIndex = -1
                });
            }
        }

        public void Dispose()
        {
            VisibleWindows.Dispose();
            VisiblePockets.Dispose();
        }

        public JobHandle Dispose(JobHandle inputDeps) => JobHandle.CombineDependencies(
            VisibleWindows.Dispose(inputDeps),
            VisiblePockets.Dispose(inputDeps));

        private void Add(Window window)
        {
            window.Index = VisibleWindows.Length;
            VisibleWindows.Add(window);
            VisiblePockets.Add(window.Exit);
        }


        private const int DepthLimit = 32;

        private static bool IsVisible(Line line) => LineMath.GetOriginDomain(line) is LineMath.LineDomain.Bottom;
        
        private static bool IsBefore(Line line, Line window) => LineMath.GetDomain(window, line) is
            LineMath.LineDomain.Bottom or LineMath.LineDomain.Line;


        public struct Window
        {
            public Portal Portal;
            public Line ExitBound;
            public int Depth;
            public int Index;
            public int ParentIndex;

            public bool HasParent => ParentIndex >= 0;

            public PocketID Exit => Portal.Exit;
        }
    }
}