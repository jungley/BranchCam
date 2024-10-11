using UnityEngine;
using UnityEditor;
using RydenCam.Common;
using Assets.RydenCam.Scripts.BranchCamCC;

namespace RydenCam.BranchCamEditor.Nodes.Connections
{

    [ExecuteAlways]
    [System.Serializable]
    public class ConnectionPoint
    {

        public NodeCC Node;
        [SerializeField] public ConnectionPointType Type;
        [SerializeField] public ConnectionPoint ConnectedTo;
        [SerializeField] public Rect LocalBounds;
        [SerializeField] public Color Color = new Color(0, 0.8f, 0, 1);

        public ConnectionPoint(NodeCC node, ConnectionPointType type)
        {
            Node = node;
            Type = type;
            // Determine the local bounds based on the type of connection point
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

        public Vector2 GetGlobalPoint()
        {
            float globalXPos = (Node.EditorPosition.x) + LocalBounds.center.x;
            float globalYPos = (Node.EditorPosition.y) + LocalBounds.center.y;
            return new Vector2(globalXPos, globalYPos);
        }

#if UNITY_EDITOR
        public void Draw(Color col)
        {
            Handles.color = col;
            Handles.DrawSolidDisc(LocalBounds.center, new Vector3(0, 0, 1), 7.0f);

            if (ConnectedTo != null)
            {
                Handles.DrawWireDisc(LocalBounds.center, new Vector3(0, 0, 1), 10.0f);
            }
        }

        public void Draw()
        {
            Handles.color = Color;
            Handles.DrawSolidDisc(LocalBounds.center, new Vector3(0, 0, 1), 7.0f);

            if (ConnectedTo != null)
            {
                Handles.DrawWireDisc(LocalBounds.center, new Vector3(0, 0, 1), 10.0f);
            }
        }

        //For the decision node
        public void Draw(int num)
        {

            Handles.color = Color;//Color.black;//pointColor;
            Handles.DrawSolidDisc(LocalBounds.center, new Vector3(0, 0, 1), 7.0f);

            if (ConnectedTo != null)
            {
                Handles.color = Color;
                Handles.DrawWireDisc(LocalBounds.center, new Vector3(0, 0, 1), 10.0f);
                Handles.color = Color; //Color.black;
            }
            Vector3 pos = LocalBounds.center;
            pos = new Vector3(pos.x - 3, pos.y - 12, pos.z);

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 10;
            Handles.Label(pos, "" + num, style);

        }
#endif
    }
}
