using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RydenCam.SequenceData;
using RydenCam.Common;

namespace RydenCam.BranchCamEditor.Nodes.Connections
{
    [ExecuteAlways]
    [System.Serializable]
    public class Connection
    {
        public ConnectionPoint Point_IN;
        public ConnectionPoint Point_OUT;
        public Action<Connection> OnClickRemoveConnection;
        Texture2D arrowImage;

        public Connection(ConnectionPoint p1, ConnectionPoint p2, Action<Connection> OnClickRemoveConnection)
        {
            this.Point_IN = p1;
            this.Point_OUT = p2;
            this.OnClickRemoveConnection = OnClickRemoveConnection;
            arrowImage = Resources.Load("arrowImage2") as Texture2D;
        }

        public bool ContainsPoint(ConnectionPoint A)
        {
            //return (A == Point_IN || A == Point_OUT);

            if ((A == Point_IN || A == Point_OUT))
            {
                return true;
            }
            return false;
        }

        public void RemoveConnection()
        {
            if (OnClickRemoveConnection != null)
            {
                OnClickRemoveConnection(this);
            }
        }

#if UNITY_EDITOR
        public void DrawGotoCurve()
        {
            Vector2 result_01 = Point_IN.getGlobalPoint();
            Vector3 startPos = new Vector3(result_01.x, result_01.y, 0);
            Vector2 result_02 = Point_OUT.getGlobalPoint();
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
            vector3array = Curver.MakeSmoothCurve(vector3array, 90.0f);
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(5.0f, vector3array);
            center.x += handlearc;

            if (Handles.Button(center, Quaternion.identity, 8, 20, Handles.RectangleHandleCap))
            {
                Point_IN.ClearPointer();
                Point_OUT.ClearPointer();
                RemoveConnection();
            }
            Handles.color = Color.green;
            return;
        }


        public void DrawArrowPointer(Vector3 startPos, Vector3 endPos)
        {
            //Calculate rotation from out point to in point 
            //Arrow always needs to point to an IN node
            float angle = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * 180 / Mathf.PI;
            angle -= 90;
            GUIUtility.RotateAroundPivot(angle, endPos);
            //move up y by amount so not directly over dot
            GUI.DrawTexture(new Rect(endPos.x - 10, endPos.y - 25, 20, 20), arrowImage, ScaleMode.StretchToFill, true, 20.0F);
            GUIUtility.RotateAroundPivot(-angle, endPos);
        }

        //Draws the Connection
        public void Draw()
        {
            //Wow what is happening
            if (Point_OUT.node.TypeOfNode == NodeType.GoToNode && Point_OUT.type == ConnectionPointType.Out 
                || Point_IN.node.TypeOfNode == NodeType.GoToNode && Point_IN.type == ConnectionPointType.Out)
            {
                DrawGotoCurve();
                return;
            }
            Vector2 result_01 = Point_IN.getGlobalPoint();
            Vector3 startPos = new Vector3(result_01.x, result_01.y, 0);
            Vector2 result_02 = Point_OUT.getGlobalPoint();
            Vector3 endPos = new Vector3(result_02.x, result_02.y, 0);
            Handles.DrawBezier(startPos, endPos, startPos, endPos, Color.green, null, 5);

            if (Point_IN.type == ConnectionPointType.Out)
            {
                DrawArrowPointer(startPos, endPos);
            }
            else
            {
                DrawArrowPointer(endPos, startPos);
            }


            Handles.color = Color.green;
            //Check Click Remove
            if (Handles.Button((Point_IN.getGlobalPoint() + Point_OUT.getGlobalPoint()) * 0.5f, Quaternion.identity, 8, 20, Handles.RectangleHandleCap))
            {
                Point_IN.ClearPointer();
                Point_OUT.ClearPointer();
                RemoveConnection();
            }
        }
#endif
    }


#if UNITY_EDITOR
    //CURVE CLASS FROM
    //https://answers.unity.com/questions/392606/line-drawing-how-can-i-interpolate-between-points.html
    public static class Curver
    {
        //arrayToCurve is original Vector3 array, smoothness is the number of interpolations. 
        public static Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve, float smoothness)
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
#endif
}
