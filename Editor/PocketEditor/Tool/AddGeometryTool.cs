using System.Collections.Generic;
using System.Linq;
using Backstreets.Data;
using Backstreets.Editor.PocketEditor.Model;
using Backstreets.Editor.PocketEditor.View;
using Backstreets.FOV.Geometry;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.Tool
{
    internal class AddGeometryTool : IGeometryTool
    {
        private readonly GeometryModel model;
        private readonly DisplacedTemplate offsetTemplate;
        private int controlID;

        public AddGeometryTool(GeometryModel model)
        {
            this.model = model;
            offsetTemplate = new DisplacedTemplate(template: SquareTemplate());
        }

        public GeometryType DrawMask => GeometryType.Everything;

        public GeometryType PickMask => GeometryType.None;

        public void OnBeforeView(Event @event)
        {
            switch (@event)
            {
                case { type: EventType.MouseDown, button: 0 }
                    when HandleUtility.nearestControl == controlID:
                {
                    GUIUtility.hotControl = controlID;
                    @event.Use();
                    break;
                }
                case { type: EventType.MouseUp, button: 0 }
                    when GUIUtility.hotControl == controlID:
                {
                    SpawnTemplate();
                    @event.Use();
                    break;
                }
                case { type: EventType.MouseMove or EventType.MouseDrag }:
                {
                    offsetTemplate.Displacement = @event.GetGeometryPosition();
                    break;
                }
                case { type: EventType.Layout }:
                {
                    controlID = GUIUtility.GetControlID(hint: GetHashCode(), FocusType.Passive);
                    HandleUtility.AddDefaultControl(controlID);
                    break;
                }
                case { type: EventType.Repaint }:
                {
                    DrawTemplatePreview();
                    break;
                }
            }
        }

        public void OnViewEvent(Event @event, GeometryID hotGeometry)
        {
        }

        public void Dispose()
        {
        }

        public void OnInspectorGUI()
        {
        }


        private void DrawTemplatePreview()
        {
            foreach (Line line in offsetTemplate)
            {
                const float dashSize = 5;
                Handles.DrawDottedLine(new float3(line.Right, 0), new float3(line.Left, 0), dashSize);
            }
        }

        private void SpawnTemplate()
        {
            foreach (Line edge in offsetTemplate)
            {
                EdgeData edgeData = new() { id = default, right = edge.Right, left = edge.Left };
                model.Edges.Create(ref edgeData);
            }
        }

        private IEnumerable<Line> SquareTemplate()
        {
            const float side = 0.5f;
            const float halfSide = side / 2;
            float2 topLeft = new(-halfSide, halfSide);
            float2 topRight = new(halfSide, halfSide);
            float2 bottomLeft = new(-halfSide, -halfSide);
            float2 bottomRight = new(halfSide, -halfSide);
            yield return new Line(bottomLeft, topLeft);
            yield return new Line(topLeft, topRight);
            yield return new Line(topRight, bottomRight);
            yield return new Line(bottomRight, bottomLeft);
        }


        private class DisplacedTemplate
        {
            private IEnumerable<Line> template;
            private IEnumerable<Line> cachedEnumerable;

            public DisplacedTemplate(IEnumerable<Line> template = default, float2 displacement = default)
            {
                Template = template ?? Enumerable.Empty<Line>();
                Displacement = displacement;
            }

            public float2 Displacement { get; set; }

            public IEnumerable<Line> Template
            {
                get => template;
                set
                {
                    template = value;
                    cachedEnumerable = template.Select(Displace);
                }
            }

            public IEnumerator<Line> GetEnumerator() => cachedEnumerable.GetEnumerator();


            private Line Displace(Line line) => new(line.Right + Displacement, line.Left + Displacement);
        }
    }
}
