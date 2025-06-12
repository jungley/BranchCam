using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
using Assets.RydenCam.Scripts.NodeCommands;
using Codice.Client.BaseCommands;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.RydenCam.Scripts.Editor
{

    //Responsible for Drawing the UI components of the Node
    //Called from Drawer factory in NodeGraphViewModel

    //Draws the node based on the node passed

    public abstract class NodeDrawerBase
    {
        public Node Node;

        protected NodeCommand Command;

        protected GUIStyle labelStyle { get; set; }

        public Rect WindowRect { get; set; }

        public bool IsActive => NodeManager.Instance.ActiveNode?.NodeId == Node.NodeId;

        private Texture2D _headerTexture { get; set; }
        protected Texture2D HeaderTexture
        {
            get
            {
                if (_headerTexture == null)
                {
                    _headerTexture = new Texture2D(1, 1);
                    _headerTexture.SetPixel(1, 1, NodeColor);
                    _headerTexture.Apply();
                }

                return _headerTexture;
            }
        }

        private Texture2D highlightTexCache;

        protected Texture2D HighlightTex
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


        protected GUIStyle labelStyleHead_Panel { get; set; }
        protected GUIStyle labelStyleHead_Node { get; set; }
        protected GUIStyle inspectorText { get; set; }
        protected GUIStyle inspectorTextBold { get; set; }
        protected GUIStyle textAreaStyleNode { get; set; }
        protected GUIStyle textAreaStyleInspector { get; set; }

        public Color NodeColor { get; set; }

        public virtual float InspectorWidth => 245;

        public int WindowId { get; set; }



        public NodeDrawerBase(Node node)
        {
            Node = node;

            NodeColor = Color.gray;

            //Styles used in Nodes
            labelStyleHead_Panel = new GUIStyle();
            labelStyleHead_Panel.normal.textColor = Color.white;
            labelStyleHead_Panel.fontStyle = FontStyle.Bold;
            labelStyleHead_Panel.fontSize = 15;

            labelStyleHead_Node = new GUIStyle();
            labelStyleHead_Node.normal.textColor = Color.white;
            labelStyleHead_Node.fontStyle = FontStyle.Bold;
            labelStyleHead_Node.fontSize = 15;

            //TextArea Node
            textAreaStyleNode = new GUIStyle(EditorStyles.textArea);
            textAreaStyleNode.wordWrap = true;
            textAreaStyleNode.alignment = TextAnchor.MiddleCenter;


            //TextArea Inspector
            textAreaStyleInspector = new GUIStyle(EditorStyles.textArea);
            textAreaStyleInspector.wordWrap = true;
            textAreaStyleInspector.margin = new RectOffset(-20, 0, 0, 0);

            inspectorText = new GUIStyle();
            inspectorText.normal.textColor = Color.white;

            inspectorTextBold = new GUIStyle();
            inspectorTextBold.normal.textColor = Color.white;
            inspectorTextBold.fontStyle = FontStyle.Bold;
        }

        public abstract void DrawNode(int index);

        public abstract void DrawNodeInspector();

        public void HighlightNode()
        {
            Rect expandedRect = new Rect(
                WindowRect.x - 2,               // Shift left by 5  
                WindowRect.y - 2,               // Shift down by 5  
                WindowRect.width + 4,           // Increase width by 10 (5 left + 5 right)  
                WindowRect.height + 4           // Increase height by 10 (5 up + 5 down)  
            );

            GUI.DrawTextureWithTexCoords(expandedRect, HighlightTex, new Rect(0, 0, 1, 1.0f));
        }

        public bool IsOverPoint(Vector2 mousePos)
        {
            bool isOverPoint = WindowRect.Contains(mousePos);
            return isOverPoint;
        }

        public ConnectionPoint GetHandlePoint(Vector2 mousePos)
        {
            //Outside the bounds of the node
            if(!WindowRect.Contains(mousePos))
            {
                return null;
            }

            Vector2 localPoint = new Vector2(mousePos.x - WindowRect.x, mousePos.y - WindowRect.y);

            if (Node.PointIn != null && Node.PointIn.LocalBounds.Contains(localPoint))
            {
                return Node.PointIn;
            }
            
            foreach(var point in Node.PointOut)
            {
                if(point.LocalBounds.Contains(localPoint))
                {
                    return point;
                }
            }

            return null;
        }

        protected int heightConnectionPoint => 18;
        protected int widthConnectionPoint => 20;
        
        protected void DrawPoint(Rect bounds, bool isConnected)
        {
            Handles.color = ConnectionPoint.Color;
            Handles.DrawSolidDisc(bounds.center, Vector3.forward, 7.0f);
            if (isConnected)
            {
                Handles.DrawWireDisc(bounds.center, Vector3.forward, 10.0f);
            }
        }

        //For Decision and Dialogue nodes / Nodes that contain text
        protected float CalculateNodeHeightFromText(List<string> dialogueList, float areaWidth) 
        {
            float totalHeight = 60;

            foreach (var dialogText in dialogueList)
            {
                // Get the height for each dialogText, with a minimum of 50
                float height = Mathf.Max(EditorGUILayoutExtensions.GetTextAreaHeight(dialogText, areaWidth), 50);

                // Adjust height (if necessary)
                height = height < 50 ? 60 : height + 10;
                totalHeight += height;
            }

            return totalHeight;
        }

        protected void DrawConnectionPoints()
        {
            //Draw In Point
            if (Node.PointIn != null)
            {
                Rect pointBoundsIn = new Rect((Node.NodeWidth / 2 - 10), 0, widthConnectionPoint, heightConnectionPoint);
                DrawPoint(pointBoundsIn, Node.PointIn.ConnectedTo != null);
            }

            //Draw Out Point
            DrawOutPoint();
        }

        protected virtual void DrawOutPoint()
        {
            Rect pointBoundsOut = new Rect((Node.NodeWidth / 2 - 10), Node.NodeHeight - 16, widthConnectionPoint, heightConnectionPoint);
            //RS TODO Move to command eventually? 
            Node.PointOut.FirstOrDefault().LocalBounds = pointBoundsOut;
            DrawPoint(pointBoundsOut, Node.PointOut.FirstOrDefault().ConnectedTo != null);
        }
    }
}
