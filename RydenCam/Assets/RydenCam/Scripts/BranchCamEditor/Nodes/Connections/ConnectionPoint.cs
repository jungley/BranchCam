using UnityEngine;
using UnityEditor;
using RydenCam.Common;

namespace RydenCam.BranchCamEditor.Nodes.Connections
{

    [ExecuteAlways]
    [System.Serializable]
    public class ConnectionPoint
    {
        public ConnectionPointType type;
        public EditorBaseNode node;
        public ConnectionPoint connectedTo;
        public Rect pointBounds;
        private Color pointColor = new Color(0, 0.8f, 0, 1);

        public ConnectionPoint(EditorBaseNode node, ConnectionPointType ty)
        {
            System.Guid guidVal = System.Guid.NewGuid();

            this.node = node;
            if (ty == ConnectionPointType.In)
            {
                this.type = ConnectionPointType.In;
                pointBounds = new Rect((node.nodeWidth / 2 - 10), 0, 20, 18);
            }
            else if (ty == ConnectionPointType.Out)
            {
                this.type = ConnectionPointType.Out;
                pointBounds = new Rect((node.nodeWidth / 2 - 10), node.nodeHeight - 16, 20, 18);
            }
        }

        public void ClearPointer()
        {
            connectedTo = null;
        }

        public Vector2 getGlobalPoint()
        {
            float globalXPos = (node.windowRect.x) + pointBounds.center.x;
            float globalYPos = (node.windowRect.y) + pointBounds.center.y;
            return new Vector2(globalXPos, globalYPos);
        }

#if UNITY_EDITOR
        public void Draw(Color col)
        {
            Handles.color = col;
            Handles.DrawSolidDisc(pointBounds.center, new Vector3(0, 0, 1), 7.0f);

            if (connectedTo != null)
            {
                Handles.DrawWireDisc(pointBounds.center, new Vector3(0, 0, 1), 10.0f);
            }
        }

        public void Draw()
        {
            Handles.color = pointColor;
            Handles.DrawSolidDisc(pointBounds.center, new Vector3(0, 0, 1), 7.0f);

            if (connectedTo != null)
            {
                Handles.DrawWireDisc(pointBounds.center, new Vector3(0, 0, 1), 10.0f);
            }
        }

        //For the decision node
        public void Draw(int num)
        {

            Handles.color = pointColor;//Color.black;//pointColor;
            Handles.DrawSolidDisc(pointBounds.center, new Vector3(0, 0, 1), 7.0f);

            if (connectedTo != null)
            {
                Handles.color = pointColor;
                Handles.DrawWireDisc(pointBounds.center, new Vector3(0, 0, 1), 10.0f);
                Handles.color = pointColor; //Color.black;
            }
            Vector3 pos = pointBounds.center;
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
