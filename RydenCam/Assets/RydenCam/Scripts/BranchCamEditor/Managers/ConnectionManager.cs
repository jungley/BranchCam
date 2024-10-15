using System;
using System.Collections.Generic;
using UnityEngine;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using Assets.RydenCam.Scripts.BranchCamCC;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;


namespace RydenCam.BranchCamEditor.Managers
{
    [ExecuteAlways]
    public class ConnectionManager : INotifyPropertyChanged
    {
        public ObservableCollection<Connection> Connections;

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ConnectionManager()
        {
            Connections = new ObservableCollection<Connection>();
        }

        public void Clear()
        {
            Connections.Clear();
        }

        public void AddConnection(ConnectionPoint fromPoint, ConnectionPoint handlePoint)
        {
            if (IsOutConnected(fromPoint, handlePoint))
            {
                RemoveConnectionsFromPoints(fromPoint, handlePoint);
            }

            //Points Reference each other
            fromPoint.ConnectedTo = handlePoint;
            handlePoint.ConnectedTo = fromPoint;

            Connection newConnection = new Connection(fromPoint, handlePoint);
            Connections.Add(newConnection);
        }

        public void RemoveConnection(Connection connection)
        {
            connection.Point_A.ConnectedTo = null;
            connection.Point_B.ConnectedTo = null;

            Connections.Remove(connection);

            reassociateConnctions(Connections);
        }


        public void RemoveConnectionsFromPoints(ConnectionPoint A, ConnectionPoint B)
        {
            A.ConnectedTo = null;
            B.ConnectedTo = null;

            Connections.ToList().RemoveAll(connection => connection.ContainsPoint(A) || connection.ContainsPoint(B));

            reassociateConnctions(Connections);
        }



        public void Remove(ConnectionPoint A)
        {
            foreach(var connection in Connections)
            {
                if (connection.ContainsPoint(A))
                {
                    RemoveConnection(connection);
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
                            AddConnection(pointOut, connectedNode.PointIn);
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
                RemoveConnection(connection);
            }

            reassociateConnctions(Connections);
        }

        private void reassociateConnctions(ObservableCollection<Connection> Connections1)
        {
            //Reassociate lost Connections
            foreach (var connection in Connections1)
            {
                var pointA = connection.Point_A;
                var pointB = connection.Point_B;
                pointA.ConnectedTo = pointB;
                pointB.ConnectedTo = pointA;
            }
        }

    }
}

