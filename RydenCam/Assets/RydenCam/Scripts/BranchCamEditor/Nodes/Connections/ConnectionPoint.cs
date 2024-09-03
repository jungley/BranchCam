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

        public NodeCC Node { get; set; }

        public ConnectionPointType type;
        public EditorBaseNode node;
        public ConnectionPoint ConnectedTo;
        public Rect Bounds;
        public Color Color = new Color(0, 0.8f, 0, 1);


        public ConnectionPoint(NodeCC node, ConnectionPointType ty)
        {
            Node = node;
            type = ty;
        }



        public ConnectionPoint(EditorBaseNode node, ConnectionPointType ty)
        {
            System.Guid guidVal = System.Guid.NewGuid();

            this.node = node;
            if (ty == ConnectionPointType.In)
            {
                this.type = ConnectionPointType.In;
                Bounds = new Rect((node.nodeWidth / 2 - 10), 0, 20, 18);
            }
            else if (ty == ConnectionPointType.Out)
            {
                this.type = ConnectionPointType.Out;
                Bounds = new Rect((node.nodeWidth / 2 - 10), node.nodeHeight - 16, 20, 18);
            }
        }

        public void ClearPointer()
        {
            ConnectedTo = null;
        }

        public Vector2 getGlobalPoint()
        {
            float globalXPos = (node.windowRect.x) + Bounds.center.x;
            float globalYPos = (node.windowRect.y) + Bounds.center.y;
            return new Vector2(globalXPos, globalYPos);
        }

#if UNITY_EDITOR
        public void Draw(Color col)
        {
            Handles.color = col;
            Handles.DrawSolidDisc(Bounds.center, new Vector3(0, 0, 1), 7.0f);

            if (ConnectedTo != null)
            {
                Handles.DrawWireDisc(Bounds.center, new Vector3(0, 0, 1), 10.0f);
            }
        }

        public void Draw()
        {
            Handles.color = Color;
            Handles.DrawSolidDisc(Bounds.center, new Vector3(0, 0, 1), 7.0f);

            if (ConnectedTo != null)
            {
                Handles.DrawWireDisc(Bounds.center, new Vector3(0, 0, 1), 10.0f);
            }
        }

        //For the decision node
        public void Draw(int num)
        {

            Handles.color = Color;//Color.black;//pointColor;
            Handles.DrawSolidDisc(Bounds.center, new Vector3(0, 0, 1), 7.0f);

            if (ConnectedTo != null)
            {
                Handles.color = Color;
                Handles.DrawWireDisc(Bounds.center, new Vector3(0, 0, 1), 10.0f);
                Handles.color = Color; //Color.black;
            }
            Vector3 pos = Bounds.center;
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
