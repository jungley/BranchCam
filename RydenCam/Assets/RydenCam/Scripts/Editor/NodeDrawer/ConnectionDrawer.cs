using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.RydenCam.Scripts.Editor.NodeDrawer
{
    public class ConnectionDrawer
    {
        private Connection connection { get; set; }


        private static Texture2D arrowImageLoaded { get; set; }
        private static Texture2D arrowImage
        {
            get
            {
                if (arrowImageLoaded == null)
                {
                    //RS TODO Duplicate arrowImage loading
                    arrowImageLoaded = Resources.Load("arrowImage2") as Texture2D;
                }
                return arrowImageLoaded;
            }
        }


        public ConnectionDrawer(Connection _connection) 
        {
            connection = _connection;
        }


        public void Draw()
        {
            Vector2 inGlobalPoint = connection.GetInPoint().GetGlobalPoint();
            Vector2 outGlobalPoint = connection.GetOutPoint().GetGlobalPoint();

            Vector3 endPos = new Vector3(outGlobalPoint.x, outGlobalPoint.y, 0);
            Vector3 startPos = new Vector3(inGlobalPoint.x, inGlobalPoint.y, 0);

            // Draw special curve for certain conditions, otherwise draw a bezier curve
            if (shouldDrawGotoCurve(connection))
            {
                DrawGotoCurve();
                return;
            }
            else
            {
                Handles.DrawBezier(startPos, endPos, startPos, endPos, Color.green, null, 5);
                Handles.color = Color.green;
            }

            DrawArrowPointer(connection);

            // Check and handle click to remove connection
            CheckAndHandleClickToRemoveConnection(outGlobalPoint, inGlobalPoint);
        }



        public void DrawGotoCurve()
        {
            Vector2 result_01 = connection.Point_A.GetGlobalPoint();
            Vector3 startPos = new Vector3(result_01.x, result_01.y, 0);
            Vector2 result_02 = connection.Point_B.GetGlobalPoint();
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


        public void DrawArrowPointer(Connection connection)
        {
            Vector2 endPos = connection.GetInPoint().GetGlobalPoint();
            Vector2 startPos = connection.GetOutPoint().GetGlobalPoint();

            //Calculate rotation from out point to in point 
            //Arrow always needs to point to an IN node
            float angle = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * 180 / Mathf.PI;
            angle -= 90;
            GUIUtility.RotateAroundPivot(angle, endPos);
            //move up y by amount so not directly over dot
            GUI.DrawTexture(new Rect(endPos.x - 10, endPos.y - 25, 20, 20), arrowImage, ScaleMode.StretchToFill, true, 20.0F);
            GUIUtility.RotateAroundPivot(-angle, endPos);
        }

        private bool shouldDrawGotoCurve(Connection connection)
        {
            var PointIn = connection.GetInPoint(); 
            var PointOut = connection.GetOutPoint();

            return PointIn.GetGlobalPoint().y < PointOut.GetGlobalPoint().y 
                && PointOut.Type == ConnectionPointType.Out 
                && PointIn.Type == ConnectionPointType.In;
        }

        private void CheckAndHandleClickToRemoveConnection(Vector2 inGlobalPoint, Vector2 outGlobalPoint)
        {
            Vector2 midpoint = (inGlobalPoint + outGlobalPoint) * 0.5f;
            if (Handles.Button(new Vector3(midpoint.x, midpoint.y, 0), Quaternion.identity, 8, 20, Handles.RectangleHandleCap))
            {
                connection.Point_A.ClearPointer();
                connection.Point_B.ClearPointer();
                connection.RemoveConnection();
            }
        }


        //https://answers.unity.com/questions/392606/line-drawing-how-can-i-interpolate-between-points.html
        private Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve, float smoothness)
        {
            List<Vector3> points;
            List<Vector3> curvedPoints;
            int pointsLength = 0;
            int curvedLength = 0;

            if (smoothness < 1.0f) smoothness = 1.0f;

            pointsLength = arrayToCurve.Length;

            curvedLength = (pointsLength * Mathf.RoundToInt(smoothness)) - 1;
            curvedPoints = new List<Vector3>(curvedLength);

            float t = 0.0f;
            for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
            {
                t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

                points = new List<Vector3>(arrayToCurve);

                for (int j = pointsLength - 1; j > 0; j--)
                {
                    for (int i = 0; i < j; i++)
                    {
                        points[i] = (1 - t) * points[i] + t * points[i + 1];
                    }
                }

                curvedPoints.Add(points[0]);
            }

            return (curvedPoints.ToArray());
        }
    }
}
