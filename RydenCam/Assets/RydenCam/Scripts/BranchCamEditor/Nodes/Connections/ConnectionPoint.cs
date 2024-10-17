using UnityEngine;
using RydenCam.Common;
using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Managers;

namespace RydenCam.BranchCamEditor.Nodes.Connections
{

    [ExecuteAlways]
    [System.Serializable]
    public class ConnectionPoint
    {
        public ConnectionPointType Type;

        public string NodeId;
        public string ConnectedNodeId;

        public Rect LocalBounds;
        public static Color Color => new Color(0, 0.8f, 0, 1);

        private Node node { get; set; }
        public Node Node 
        {
            get
            {
                if(node == null && !string.IsNullOrEmpty(NodeId))
                {
                    node = NodeManager.Instance.FindNode(NodeId);
                }
                return node;
            }
            set
            {
                node = value;
                NodeId = value?.NodeId;
            }
        }
        
        private ConnectionPoint connectedTo { get; set; }
        public ConnectionPoint ConnectedTo
        {
            get { return connectedTo; }
            set 
            { 
                connectedTo = value;
                ConnectedNodeId = connectedTo?.Node?.NodeId;
            }    
        }

        public ConnectionPoint(Node node, ConnectionPointType type)
        {
            Node = node;
            Type = type;

            // Determine the local bounds based on the type of connection point
            if (node == null) return;

            if (type == ConnectionPointType.In)
            {
                // For input connection points (In), position the bounds at the top center of the node
                LocalBounds = new Rect((node.NodeWidth / 2 - 10), 0, 20, 18);
            }
            else if (type == ConnectionPointType.Out)
            {
                // For output connection points (Out), position the bounds at the bottom center of the node
                LocalBounds = new Rect((node.NodeWidth / 2 - 10), node.NodeHeight - 16, 20, 18);
            }
        }

        public void ClearPointer()
        {
            ConnectedTo = null;
        }


        private Vector2 globalPoint { get; set; }
        public Vector2 GlobalPoint
        {
            get
            {
                if (Node != null)
                { 
                    float globalXPos = (Node.EditorPosition.x) + LocalBounds.center.x;
                    float globalYPos = (Node.EditorPosition.y) + LocalBounds.center.y;
                    globalPoint = new Vector2(globalXPos, globalYPos);
                }

                return globalPoint;

            }
            set
            {
                globalPoint = value;
            }
        }

    }
}
