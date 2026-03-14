using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using UnityEngine;

namespace Assets.RydenCam.Scripts.NodeCommands
{
    public abstract class NodeCommand
    {
        protected Node Node { get; set; }

        /// <summary>
        /// Drawer properties. Needed in Command class for referencing interactions
        /// </summary>
        public Rect WindowRect { get; set; }
        public Color NodeColor { get; set; }

        public bool IsActive => NodeManager.Instance.ActiveNode?.NodeId == Node.NodeId;


        public NodeCommand(Node node)
        {
            Node = node;
            NodeManager.Instance.RegisterNodeCommand(node, this);
        }

        public void HighlightIfActive()
        {
            if (IsActive)
            {
                HighlightNode();
            }
        }

        /// <summary>
        /// Removes the node from the NodeManager and any associated connections
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode()
        {
            NodeManager.Instance.ActiveNode = null;
            NodeManager.Instance.RemoveNode(Node);
            ConnectionManager.Instance.RemoveAssociatedConnections(Node);
        }

        private Texture2D highlightTexCache;

        public Texture2D HighlightTex
        {
            get
            {
                if (highlightTexCache == null)
                {
                    CreateHighlightTexture();
                }

                return highlightTexCache;
            }
        }

        public void HighlightNode()
        {
            Rect expandedRect = new Rect(
                WindowRect.x - 2,               // Shift left by 5  
                WindowRect.y - 2,               // Shift down by 5  
                WindowRect.width + 4,           // Increase width by 10 (5 left + 5 right)  
                WindowRect.height + 4           // Increase height by 10 (5 up + 5 down)  
                );

#if UNITY_EDITOR
            GUI.DrawTextureWithTexCoords(expandedRect, HighlightTex, new Rect(0, 0, 1, 1.0f));
#endif
        }

        public void ClearHighlightTexture() => highlightTexCache = null;

        public void CreateHighlightTexture(Color? color = null)
        {
            Color NColor = color ?? NodeColor;

            Rect rect = new Rect(WindowRect);
            int width = (int)rect.width;
            int height = (int)rect.height;
            int borderWidth = 2;

            Texture2D texture = new Texture2D(width, height);
            Color[] colors = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = x < borderWidth || x >= width - borderWidth ||
                                    y < borderWidth || y >= height - borderWidth;


                    colors[y * width + x] = isBorder ? NColor : Color.clear;
                }
            }

            texture.SetPixels(colors);
            texture.Apply();
            highlightTexCache = texture;
        }


        public virtual ConnectionPoint SelectedEndPointFromNode(ConnectionPointType incomingType, Vector2 mousePos)
        {
            if (incomingType == ConnectionPointType.Out)
                return Node.PointIn;

            if (incomingType == ConnectionPointType.In && Node.PointOut != null && Node.PointOut.Count > 0)
                return Node.PointOut[0];

            return null;
        }

        public ConnectionPoint GetSelectedStartPoint(Vector2 mousePos)
        {
            //Outside the bounds of the node
            if (!WindowRect.Contains(mousePos))
            {
                return null;
            }

            Vector2 localPoint = new Vector2(mousePos.x - WindowRect.x, mousePos.y - WindowRect.y);

            if (Node.PointIn != null && Node.PointIn.LocalBounds.Contains(localPoint))
            {
                return Node.PointIn;
            }

            foreach (var point in Node.PointOut)
            {
                if (point.LocalBounds.Contains(localPoint))
                {
                    return point;
                }
            }

            return null;
        }
    }
}
