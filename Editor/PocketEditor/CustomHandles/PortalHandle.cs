using System.Linq;
using Backstreets.FOV.Geometry;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Editor.PocketEditor.CustomHandles
{
    public static class PortalHandle
    {
        public static void Static(Line line, Color? color, float? thickness)
        {
            if (Event.current is not {type: EventType.Repaint}) return;

            Draw(line, color ?? Handles.color, thickness ?? 0);
        }
        
        public static bool Clickable(Line line, Color? color, float? thickness)
        {
            int controlID = GUIUtility.GetControlID(ControlHint, FocusType.Passive);
            HandleUtility.AddControl(controlID, Glow.DistanceToPointer(line));
            HandleUtility.AddControl(controlID, Arrow.DistanceToPointer(line));

            bool isHovered = HandleUtility.nearestControl == controlID;
            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.Repaint:
                    Draw(line, isHovered ? Color.white : color ?? Handles.color, thickness ?? 0);
                    return false;
                case EventType.MouseDown when isHovered:
                    GUIUtility.hotControl = controlID;
                    Event.current.Use();
                    return false;
                case EventType.MouseUp when isHovered && GUIUtility.hotControl == controlID:
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                    return true;
                default:
                    return false;
            }
        }

        private static void Draw(Line line, Color color, float thickness)
        {
            using Handles.DrawingScope _ = new(color, MakeLineMatrix(line));
            Glow.Draw(line);
            Arrow.Draw(thickness);
        }

        private static Matrix4x4 MakeLineMatrix(Line line)
        {
            Vector2 middle = (line.Left + line.Right) / 2;
            Vector2 up = Vector2.Perpendicular(line.Right - line.Left);
            return Matrix4x4.Translate(middle) *
                   Matrix4x4.LookAt(Vector3.zero, Vector3.forward, up);
        }

        private static Matrix4x4 MakeLineMatrixStretched(Line line)
        {
            Vector2 middle = (line.Right + line.Left) / 2;
            Vector2 up = Vector2.Perpendicular(line.Right - line.Left);
            float width = math.length(line.Left - line.Right);
            return Matrix4x4.Translate(middle) *
                   Matrix4x4.LookAt(Vector3.zero, Vector3.forward, up) *
                   Matrix4x4.Scale(new Vector3(width, 1, 1));
        }

        private static readonly int ControlHint = "PortalHandle".GetHashCode();

        private static class Arrow
        {
            private static readonly (Vector3 p1, Vector3 p2)[] Lines;
            private static readonly int[] SegmentIndices;
            private const float ArrowLength = 0.5f;
            private const float WingLength = ArrowLength / 2;
            private const float WingsAngle = 20;

            static Arrow()
            {
                Quaternion leftWingRotation = Quaternion.AngleAxis(WingsAngle, Vector3.forward);
                Quaternion rightWingRotation = Quaternion.AngleAxis(-WingsAngle, Vector3.forward);

                Vector3 tip = Vector3.zero;
                Vector3 origin = tip + Vector3.down * ArrowLength;
                Vector3 leftWing = leftWingRotation * Vector3.down * WingLength + tip;
                Vector3 rightWing = rightWingRotation * Vector3.down * WingLength + tip;

                Lines = new[] { (origin, tip), (tip, leftWing), (tip, rightWing) };
            }

            internal static void Draw(float thickness)
            {
                foreach ((Vector3 p1, Vector3 p2) in Lines)
                {
                    Handles.DrawLine(p1, p2, thickness);
                }
            }

            internal static float DistanceToPointer(Line line)
            {
                using Handles.DrawingScope matrixScope = new(MakeLineMatrix(line));
                return Lines.Select(l => HandleUtility.DistanceToLine(l.p1, l.p2)).Min();
            }
        }

        private static class Glow
        {
            private static readonly Mesh Mesh;
            private const float Depth = 0.1f;

            static Glow()
            {
                Color opaque = Color.white;
                Color clear = new(1, 1, 1, 0);

                Mesh = new Mesh
                {
                    vertices = new[]
                    {
                        new Vector3(-0.5f, 0, 0),
                        new Vector3(0.5f, 0, 0),
                        new Vector3(-0.5f, -Depth, 0),
                        new Vector3(0.5f, -Depth, 0),
                    },
                    colors = new[] { opaque, opaque, clear, clear },
                    triangles = new[] { 0, 1, 2, 1, 3, 2 },
                    normals = Enumerable.Repeat(Vector3.back, 4).ToArray(),
                };
            }
            
            internal static void Draw(Line line)
            {
                Matrix4x4 matrix = MakeLineMatrixStretched(line);
                Color color = Handles.color * new Color(1f, 1f, 1f, 0.5f) + (Handles.lighting
                    ? new Color(0.0f, 0.0f, 0.0f, 0.5f)
                    : new Color(0.0f, 0.0f, 0.0f, 0.0f));
                CommandBuffer cmd = CommandBufferPool.Get("Portal gradient");
                cmd.SetGlobalColor("_HandleColor", color);
                cmd.SetGlobalFloat("_HandleSize", 1);
                cmd.SetGlobalMatrix("_ObjectToWorld", matrix);
                cmd.DrawMesh(Mesh, matrix, HandleUtility.handleMaterial, 0, 0);
                Graphics.ExecuteCommandBuffer(cmd);
            }

            internal static float DistanceToPointer(Line line)
            {
                using Handles.DrawingScope matrixScope = new(MakeLineMatrixStretched(line));
                return HandleUtility.DistanceToRectangle(Vector3.down / 2, Quaternion.identity, 0.5f);
            }
        }
    }
}