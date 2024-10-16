using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.RydenCam.Scripts.Editor.NodeDrawer
{
    public class ConnectionDrawer
    {
        private Connection connection;
        private ConnectionRenderer renderer;

        public ConnectionDrawer(Connection connection = null)
        {
            this.connection = connection;
            renderer = new ConnectionRenderer();
        }

        public void Draw(bool isUserDrawing = false)
        {
            renderer.Draw(connection, isUserDrawing);
        }

        public void DrawFromUserHandle(ConnectionPoint selectedConnectionPoint, Vector2 mousePosition)
        {
            var userHandlePoint = new ConnectionPoint(null, ConnectionPointType.UserHandleOnGUI)
            {
                GlobalPoint = mousePosition
            };

            var newConnection = new Connection(selectedConnectionPoint, userHandlePoint);
            renderer.Draw(newConnection, isUserDrawing: true);
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
                {
                    arrowImageLoaded = Resources.Load("arrowImage2") as Texture2D;
                }
                return arrowImageLoaded;
            }
        }

        public void Draw(Connection connection, bool isUserDrawing)
        {
            Vector2 inGlobalPoint = GetInGlobalPoint(connection, isUserDrawing);
            Vector2 outGlobalPoint = GetOutGlobalPoint(connection, isUserDrawing);

            Vector3 startPos = new Vector3(inGlobalPoint.x, inGlobalPoint.y, 0);
            Vector3 endPos = new Vector3(outGlobalPoint.x, outGlobalPoint.y, 0);


            if (ShouldDrawGotoCurve(connection))
            {
                DrawGotoCurve(connection);
            }
            else
            {
                Handles.DrawBezier(startPos, endPos, startPos, endPos, Color.green, null, 5);
                CheckAndHandleClickToRemoveConnection(connection, inGlobalPoint, outGlobalPoint);
                DrawArrow(endPos, startPos, isUserDrawing);
            }

        }

        public void DrawArrow(Vector2 startPos, Vector2 endPos, bool isUserDrawing = false)
        {
            //Arrow needs to be switched in draw mode
            if(isUserDrawing)
            {
                Vector3 temp = startPos;
                startPos = endPos;
                endPos = temp;
            }

            float angle = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * Mathf.Rad2Deg - 90;
            GUIUtility.RotateAroundPivot(angle, endPos);
            GUI.DrawTexture(new Rect(endPos.x - 10, endPos.y - 25, 20, 20), ArrowImage, ScaleMode.StretchToFill, true, 20.0F);
            GUIUtility.RotateAroundPivot(-angle, endPos);
        }

        private Vector2 GetInGlobalPoint(Connection connection, bool isUserDrawing)
        {
            return isUserDrawing
                ? (connection.GetInPoint() ?? connection.GetOutPoint()).GlobalPoint
                : connection.GetInPoint().GlobalPoint;
        }

        private Vector2 GetOutGlobalPoint(Connection connection, bool isUserDrawing)
        {
            return isUserDrawing
                ? connection.GetUserHandlePoint().GlobalPoint
                : connection.GetOutPoint().GlobalPoint;
        }

        private bool ShouldDrawGotoCurve(Connection connection)
        {
            var pointIn = connection.GetInPoint() ?? connection.GetUserHandlePoint();
            var pointOut = connection.GetOutPoint() ?? connection.GetUserHandlePoint();

            bool isPointInLower = pointIn.GlobalPoint.y < pointOut.GlobalPoint.y;
            bool isValidGotoConnectionCurve =
                (pointOut.Type == ConnectionPointType.Out && pointIn.Type == ConnectionPointType.In) ||
                (pointOut.Type == ConnectionPointType.Out && pointIn.Type == ConnectionPointType.UserHandleOnGUI);

            return isPointInLower && isValidGotoConnectionCurve;
        }

        private void DrawGotoCurve(Connection connection)
        {
            Vector2 result_01 = connection.Point_A.GlobalPoint;
            Vector3 startPos = new Vector3(result_01.x, result_01.y, 0);
            Vector2 result_02 = connection.Point_B.GlobalPoint;
            Vector3 endPos = new Vector3(result_02.x, result_02.y, 0);
            Vector3 center = new Vector3((startPos.x + endPos.x) / 2, (endPos.y + startPos.y) / 2);
            float arc;
            float handlearc;
            if (endPos.x <= startPos.x)
            {
                arc = -600.0f;
                handlearc = 300.0f;
            }
            else
            {
                arc = 600.0f;
                handlearc = -300.0f;
            }
            center.x += arc;
            Vector3[] vector3array = new Vector3[] { startPos, center, endPos };
            vector3array = MakeSmoothCurve(vector3array, 90.0f);
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(5.0f, vector3array);
            center.x += handlearc;

            
            if (Handles.Button(center, Quaternion.identity, 8, 20, Handles.RectangleHandleCap))
            {
                connection.Point_A.ClearPointer();
                connection.Point_B.ClearPointer();
                connection.RemoveConnection();
            }
            
            

            Handles.color = Color.green;
            return;
        }

        private void CheckAndHandleClickToRemoveConnection(Connection connection, Vector2 inGlobalPoint, Vector2 outGlobalPoint, bool isGotoConnection = false)
        {
            Vector2 midpoint = (inGlobalPoint + outGlobalPoint) * 0.5f;

            if (Handles.Button(new Vector3(midpoint.x, midpoint.y, 0), Quaternion.identity, 8, 20, Handles.RectangleHandleCap))
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
