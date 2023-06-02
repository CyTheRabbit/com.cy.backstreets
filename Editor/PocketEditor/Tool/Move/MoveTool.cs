﻿using System;
using System.Linq;
using Backstreets.Data;
using Backstreets.Editor.PocketEditor.Model;
using Backstreets.Editor.PocketEditor.View;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Backstreets.Editor.PocketEditor.Tool.Move
{
    internal class MoveTool : IGeometryTool
    {
        private readonly GeometryModel model;
        private SelectionBox selectionBox;
        private VertexID[] selectedCorners;
        private Vector2 preciseMoveInput;


        public MoveTool(GeometryModel model)
        {
            this.model = model;
            selectionBox = default;
            selectedCorners = Array.Empty<VertexID>();
            preciseMoveInput = Vector2.zero;
        }


        public GeometryType DrawMask => GeometryType.Everything;

        public GeometryType PickMask => GeometryType.None;


        public void OnBeforeView(Event @event)
        {
            ProcessSelection(@event);

            if (!selectionBox.IsDragging)
            {
                ProcessMoveHandle();
            }
        }

        public void OnViewEvent(Event @event, GeometryID hotGeometry)
        {
            if (@event is { type: EventType.Repaint })
            {
                DrawSelectedCorners();

                if (!selectionBox.IsDragging)
                {
                    DrawPreciseMovePreview();
                }
            }
        }

        private void DrawSelectedCorners()
        {
            foreach (VertexID cornerID in selectedCorners)
            {
                float2 corner = model.Corners.Get(cornerID);
                GeometryDrawer.DrawCorner(corner, Color.yellow, thickness: 2f);
            }
        }

        private void DrawPreciseMovePreview()
        {
            if (preciseMoveInput == default) return;

            foreach (VertexID cornerID in selectedCorners)
            {
                float2 corner = model.Corners.Get(cornerID);
                GeometryDrawer.DrawCorner(corner + (float2)preciseMoveInput, Color.white, thickness: 0.5f);
            }
        }

        public void Dispose()
        {
        }

        public void OnInspectorGUI()
        {
            if (selectedCorners.Length == 0)
            {
                GUILayout.Label("Select geometry to see more options");
                return;
            }

            using EditorGUI.ChangeCheckScope check = new();

            GUILayout.Label("Precise Input");
            using (new EditorGUI.IndentLevelScope())
            using (new GUILayout.HorizontalScope())
            {
                preciseMoveInput = EditorGUILayout.Vector2Field(GUIContent.none, preciseMoveInput);
                if (GUILayout.Button("Move"))
                {
                    MoveSelectedCorners(preciseMoveInput);
                }

            }

            if (check.changed)
            {
                model.UpdateView();
            }
        }


        private void ProcessSelection(Event @event)
        {
            using EditorGUI.ChangeCheckScope check = new();
            selectionBox = SelectionBox.Handle(selectionBox, @event);
            if (check.changed)
            {
                CaptureSelectedCorners();
            }
        }

        private void ProcessMoveHandle()
        {
            if (selectedCorners.Length == 0) return;

            Vector2 center = GetSelectionCenter();
            Vector2 newCenter = Handles.PositionHandle(center, Quaternion.identity);
            Vector2 delta = newCenter - center;
            if (delta != Vector2.zero)
            {
                MoveSelectedCorners(delta);
            }
        }

        private void CaptureSelectedCorners()
        {
            Rect rect = selectionBox.WorldRect;
            selectedCorners = model.Corners.All
                .Where(corner => rect.Contains(corner.Position))
                .Select(corner => corner.ID)
                .ToArray();
            model.UpdateView();
        }

        private void MoveSelectedCorners(Vector2 offset)
        {
            using RecordChangesScope changes = model.RecordChanges("Move corners");
            foreach (VertexID id in selectedCorners)
            {
                float2 position = model.Corners.Get(id);
                float2 newPosition = position + (float2)offset;
                model.Corners.Update(id, newPosition);
            }
        }

        private Vector2 GetSelectionCenter() =>
            selectedCorners
                .Select(id => model.Corners.Get(id))
                .Aggregate(float2.zero, (sum, corner) => sum + corner) /
            selectedCorners.Length;
    }
}
