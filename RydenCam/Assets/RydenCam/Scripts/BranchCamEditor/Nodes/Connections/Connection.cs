using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RydenCam.SequenceData;
using RydenCam.Common;

//here
namespace RydenCam.BranchCamEditor.Nodes.Connections
{
    [ExecuteAlways]
    [System.Serializable]
    public class Connection
    {
        public ConnectionPoint Point_A;
        public ConnectionPoint Point_B;
        public Action<Connection> OnClickRemoveConnection;

        public Connection(ConnectionPoint pointA, ConnectionPoint pointB, Action<Connection> OnClickRemoveConnection)
        {
            this.Point_A = pointA;
            this.Point_B = pointB;
            this.OnClickRemoveConnection = OnClickRemoveConnection;
        }

        public ConnectionPoint GetInPoint()
        {
            return Point_A?.Type == ConnectionPointType.In ? Point_A :
                   Point_B?.Type == ConnectionPointType.In ? Point_B :
                   null;
        }

        public ConnectionPoint GetOutPoint()
        {
            return Point_A?.Type == ConnectionPointType.Out ? Point_A :
                    Point_B?.Type == ConnectionPointType.Out ? Point_B :
                null;
        }


        public bool ContainsPoint(ConnectionPoint point)
        {
            return (point == Point_A || point == Point_B);
        }

        public void RemoveConnection()
        {
            if (OnClickRemoveConnection != null)
            {
                OnClickRemoveConnection(this);
            }
        }
    }
}
