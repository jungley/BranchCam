using System;
using System.Collections.Generic;
using UnityEngine;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;
using Assets.RydenCam.Scripts.BranchCamCC;

namespace RydenCam.BranchCamEditor.Managers
{
    [System.Serializable]
    [ExecuteAlways]
    public class ConnectionManager
    {
        private List<Connection> connectionList;
        public static ConnectionManager Instance { get; private set; }
        
        private ConnectionManager()
        {
            connectionList = new List<Connection>();
        } 
        
        static ConnectionManager()
        {
            Instance = new ConnectionManager();
        }
        
        
        public int GetLength()
        {
            return connectionList.Count;
        }
        

        public void Clear()
        {
            connectionList.Clear();
        }

        public void AddConnection(ConnectionPoint fromPoint, ConnectionPoint handlePoint, Action<Connection> action)
        {
            var pointIN = fromPoint.Type == ConnectionPointType.In ? fromPoint : handlePoint.Type == ConnectionPointType.In ? handlePoint : null;
            var pointOUT = fromPoint.Type == ConnectionPointType.Out ? fromPoint : handlePoint.Type == ConnectionPointType.Out ? handlePoint : null;

            Connection newConnection = new Connection(pointIN, pointOUT, action);
            connectionList.Add(newConnection);
        }

        public void Remove(Connection connection)
        {
            connection.Point_OUT.ConnectedTo = null;
            connection.Point_IN.ConnectedTo = null;
            connectionList.Remove(connection);
        }

        public bool IsOutConnected(ConnectionPoint A, ConnectionPoint B)
        {
            return (A.Type == ConnectionPointType.Out && A.ConnectedTo != null) ||
                   (B.Type == ConnectionPointType.Out && B.ConnectedTo != null);
        }

        public void Remove(ConnectionPoint A, ConnectionPoint B)
        {
            try { A.ConnectedTo.ConnectedTo = null; }
            catch (Exception) { }

            try { B.ConnectedTo.ConnectedTo = null; }
            catch (Exception) { }

            connectionList.RemoveAll(connection => connection.ContainsPoint(A) || connection.ContainsPoint(B));
        }

        public void Remove(ConnectionPoint A)
        {
            for (int i = 0; i < connectionList.Count; i++)
            {
                if (connectionList[i].ContainsPoint(A))
                {
                    Remove(connectionList[i]);
                    return;
                }
            }
        }

        public void RemoveAssocConnec(NodeCC node)
        {
            List<ConnectionPoint> delPointList = new List<ConnectionPoint> { node.PointIn };
            delPointList.AddRange(node.PointOut);

            for (int i = 0; i < delPointList.Count; i++)
            {
                for (int j = 0; j < connectionList.Count; j++)
                {
                    if (connectionList[j].ContainsPoint(delPointList[i]))
                    {
                        connectionList.RemoveAt(j);
                        j = -1;
                    }
                }
            }
        }

        public void DrawConnections()
        {
#if UNITY_EDITOR
            //Connection can sometimes be modified
            try
            {
                foreach (Connection connection in connectionList)
                {
                    connection.Draw();
                }
            }
            catch (Exception) { }
#endif
        }
    }
}
