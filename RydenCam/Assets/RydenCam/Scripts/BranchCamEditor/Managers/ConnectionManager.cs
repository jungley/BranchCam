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
        public List<Connection> Connections;
        
        private static ConnectionManager instance;
        public static ConnectionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ConnectionManager();
                }
                return instance;
            }
        }

        private ConnectionManager()
        {
            Connections = new List<Connection>();
        } 
        

        public void Clear()
        {
            Connections.Clear();
        }

        public void AddConnection(ConnectionPoint fromPoint, ConnectionPoint handlePoint, Action<Connection> action)
        {
            if (IsOutConnected(fromPoint, handlePoint))
            {
                Remove(fromPoint, handlePoint);
            }

            //Points Reference each other
            fromPoint.ConnectedTo = handlePoint;
            handlePoint.ConnectedTo = fromPoint;


            Connection newConnection = new Connection(fromPoint, handlePoint, action);
            Connections.Add(newConnection);
        }

        public void Remove(Connection connection)
        {
            connection.Point_A.ConnectedTo = null;
            connection.Point_B.ConnectedTo = null;
            

            Connections.Remove(connection);
        }

        public static void OnClickRemoveConnection(Connection connection)
        {
            Instance.Remove(connection);
        }


        //RSTODO Not sure if this method is necessary with the below Remove?
        public void Remove(ConnectionPoint A, ConnectionPoint B)
        {
            A.ConnectedTo = null;
            B.ConnectedTo = null;

            Connections.RemoveAll(connection => connection.ContainsPoint(A) || connection.ContainsPoint(B));
        }
        

        public void Remove(ConnectionPoint A)
        {
            foreach(var connection in Connections)
            {
                if (connection.ContainsPoint(A))
                {
                    Remove(connection);
                    return;
                }
            }
        }

        public bool IsOutConnected(ConnectionPoint A, ConnectionPoint B)
        {
            return (A.Type == ConnectionPointType.Out && A.ConnectedTo != null) ||
                   (B.Type == ConnectionPointType.Out && B.ConnectedTo != null);
        }

        public void CreateConnections(List<NodeCC> nodes)
        {
            //Associate Connections
            foreach (var node in nodes)
            {
                foreach (var pointOut in node.PointOut)
                {
                    NodeCC connectedNode = NodeManager.Instance.FindNode(pointOut.ConnectedNodeId);
                    {
                        if (connectedNode != null)
                        {
                            AddConnection(pointOut, connectedNode.PointIn, OnClickRemoveConnection);
                        }
                    }
                }
            }
        }


        public void RemoveAssociatedConnections(NodeCC node)
        {
            var pointsToRemove = new HashSet<ConnectionPoint> { node.PointIn };
            pointsToRemove.UnionWith(node.PointOut);

            var connectionsToRemove = new List<Connection>();

            foreach (var connection in Connections)
            {
                foreach (var point in pointsToRemove)
                {
                    if (connection.ContainsPoint(point))
                    {
                        connectionsToRemove.Add(connection);
                        break;
                    }
                }
            }

            // Remove identified connections
            foreach (var connection in connectionsToRemove)
            {
                Remove(connection);
            }
            
            //Reassociate lost Connections
            foreach(var connection in Connections)
            {
                var pointA = connection.Point_A;
                var pointB = connection.Point_B;
                pointA.ConnectedTo = pointB;
                pointB.ConnectedTo = pointA;
            }
        }
    }
}
