using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.RydenCam.Scripts.Editor.NodeDrawers
{
    public class ConnectionDrawer
    {
        private readonly Connection connection;
        private static readonly ConnectionRenderer renderer = new ConnectionRenderer();



        public ConnectionDrawer(Connection connection = null)
        {
            this.connection = connection;
        }

        /// <summary>
        /// Draws the stored connection.
        /// </summary>
        public void Draw()
        {
            if (connection != null)
                renderer.Draw(connection);
        }

        /// <summary>
        /// Draws a temporary connection from a selected point to the current mouse position.
        /// </summary>
        public void DrawUserHandle(ConnectionPoint selectedConnectionPoint, Vector2 mousePosition)
        {
            var userHandlePoint = new ConnectionPoint(null, ConnectionPointType.UserHandleOnGUI)
            {
                GlobalPoint = mousePosition
            };
            var tempConnection = new Connection(selectedConnectionPoint, userHandlePoint);
            renderer.Draw(tempConnection, isUserDrawing: true);
        }
    }

    public class ConnectionRenderer
    {
        private static Texture2D arrowImageLoaded;
        private static Texture2D ArrowImage
        {
            get
            {
                if (arrowImageLoaded == null)
                    arrowImageLoaded = Resources.Load("arrowImage2") as Texture2D;
                return arrowImageLoaded;
            }
        }

        public void Draw(Connection connection, bool isUserDrawing = false)
        {
            // 1. Get connection points
            var (start, end) = GetConnectionPoints(connection, isUserDrawing);

            // 2. Draw the connection curve
            if (ShouldDrawGotoCurve(connection, start, end, isUserDrawing))
            {
                DrawGotoCurve(connection, start, end);
            }
            else
            {
                DrawBezierCurve(start, end);
                DrawRemoveButton(connection, start, end);
                DrawArrow(end, start, isUserDrawing);
            }
        }

        private (Vector2 start, Vector2 end) GetConnectionPoints(Connection connection, bool isUserDrawing)
        {
            if (isUserDrawing)
            {
                var inPoint = connection.GetInPoint() ?? connection.GetOutPoint();
                var outPoint = connection.GetUserHandlePoint();
                return (inPoint.GlobalPoint, outPoint.GlobalPoint);
            }
            else
            {
                return (connection.GetInPoint().GlobalPoint, connection.GetOutPoint().GlobalPoint);
            }
        }

        private bool ShouldDrawGotoCurve(Connection connection, Vector2 start, Vector2 end, bool isUserDrawing)
        {
            var pointIn = connection.GetInPoint() ?? connection.GetUserHandlePoint();
            var pointOut = connection.GetOutPoint() ?? connection.GetUserHandlePoint();

            bool isValidGoto = pointOut.Type == ConnectionPointType.Out &&
                               (pointIn.Type == ConnectionPointType.In || pointIn.Type == ConnectionPointType.UserHandleOnGUI);

            bool isPointInLower = isUserDrawing ? start.y > end.y : start.y < end.y;

            return isPointInLower && isValidGoto;
        }

        private void DrawBezierCurve(Vector2 start, Vector2 end)
        {
            Handles.DrawBezier(start, end, start, end, Color.green, null, 5);
        }

        private void DrawRemoveButton(Connection connection, Vector2 start, Vector2 end)
        {
            Vector2 midpoint = (start + end) * 0.5f;
            Handles.color = Color.green;
            if (Handles.Button(new Vector3(midpoint.x, midpoint.y, 0), Quaternion.identity, 8, 20, Handles.RectangleHandleCap))
            {
                connection.Point_A.ClearPointer();
                connection.Point_B.ClearPointer();
                connection.RemoveConnection();
            }
        }

        private void DrawArrow(Vector2 start, Vector2 end, bool isUserDrawing)
        {
            if (isUserDrawing)
                (start, end) = (end, start);

            float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg - 90;
            GUIUtility.RotateAroundPivot(angle, end);
            GUI.DrawTexture(new Rect(end.x - 10, end.y - 25, 20, 20), ArrowImage, ScaleMode.StretchToFill, true, 20.0F);
            GUIUtility.RotateAroundPivot(-angle, end);
        }

        private void DrawGotoCurve(Connection connection, Vector2 start, Vector2 end)
        {
            Vector3 startPos = start;
            Vector3 endPos = end;
            Vector3 center = new Vector3((startPos.x + endPos.x) / 2, (endPos.y + startPos.y) / 2);

            float arc = endPos.x <= startPos.x ? -600.0f : 600.0f;
            float handleArc = endPos.x <= startPos.x ? 300.0f : -300.0f;
            center.x += arc;

            Vector3[] curvePoints = MakeSmoothCurve(new[] { startPos, center, endPos }, 90.0f);
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(5.0f, curvePoints);

            center.x += handleArc;
            if (Handles.Button(center, Quaternion.identity, 8, 20, Handles.RectangleHandleCap))
            {
                connection.Point_A.ClearPointer();
                connection.Point_B.ClearPointer();
                connection.RemoveConnection();
            }
        }

        private Vector3[] MakeSmoothCurve(Vector3[] points, float smoothness)
        {
            if (smoothness < 1.0f) smoothness = 1.0f;
            int pointsLength = points.Length;
            int curvedLength = (pointsLength * Mathf.RoundToInt(smoothness)) - 1;
            List<Vector3> curvedPoints = new List<Vector3>(curvedLength);

            for (int pointIndex = 0; pointIndex <= curvedLength; pointIndex++)
            {
                float t = Mathf.InverseLerp(0, curvedLength, pointIndex);
                List<Vector3> currentPoints = new List<Vector3>(points);

                for (int j = pointsLength - 1; j > 0; j--)
                {
                    for (int i = 0; i < j; i++)
                    {
                        currentPoints[i] = (1 - t) * currentPoints[i] + t * currentPoints[i + 1];
                    }
                }

                curvedPoints.Add(currentPoints[0]);
            }

            return curvedPoints.ToArray();
        }
    }
}
