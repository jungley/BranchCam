using Assets.RydenCam.Scripts.BranchCamCC;
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
        public NodeCC Node;

        protected INodeCommand Command;

        protected GUIStyle labelStyle { get; set; }

        public Rect windowRect { get; set; }

        private Texture2D _headerTexture { get; set; }
        protected Texture2D HeaderTexture
        {
            get
            {
                if (_headerTexture == null)
                {
                    _headerTexture = new Texture2D(1, 1);
                    _headerTexture.SetPixel(1, 1, nodeColor);
                    _headerTexture.Apply();
                }

                return _headerTexture;
            }
        }

        protected GUIStyle labelStyleHead_Panel { get; set; }
        protected GUIStyle labelStyleHead_Node { get; set; }
        protected GUIStyle inspectorText { get; set; }
        protected GUIStyle inspectorTextBold { get; set; }

        protected GUIStyle textareaStyle { get; set; }

        public float nodeWidth { get; set; }
        public virtual float nodeHeight { get; set; }

        public Color nodeColor { get; set; }

        public int WindowId { get; set; }



        public NodeDrawerBase(NodeCC node)
        {
            Node = node;

            nodeColor = Color.gray;

            //Styles used in Nodes
            labelStyleHead_Panel = new GUIStyle();
            labelStyleHead_Panel.normal.textColor = Color.white;
            labelStyleHead_Panel.fontStyle = FontStyle.Bold;
            labelStyleHead_Panel.fontSize = 15;

            labelStyleHead_Node = new GUIStyle();
            labelStyleHead_Node.normal.textColor = Color.white;
            labelStyleHead_Node.fontStyle = FontStyle.Bold;
            labelStyleHead_Node.fontSize = 15;

            //Styles used in inspector
            textareaStyle = new GUIStyle(EditorStyles.textArea);
            textareaStyle.wordWrap = true;
            textareaStyle.margin = new RectOffset(20, 0, 0, 0);

            inspectorText = new GUIStyle();
            inspectorText.normal.textColor = Color.white;

            inspectorTextBold = new GUIStyle();
            inspectorTextBold.normal.textColor = Color.white;
            inspectorTextBold.fontStyle = FontStyle.Bold;
        }

        public abstract void DrawNode(int index);

        public abstract void DrawNodeInspector();


        protected void DrawUserHandledConnection(NodeCC node)
        {

        }


        protected int heightConnectionPoint => 18;
        protected int widthConnectionPoint => 20;

        
        protected void DrawPoint(Rect bounds, Color color, bool isConnected)
        {
            Handles.color = color;
            Handles.DrawSolidDisc(bounds.center, Vector3.forward, 7.0f);
            if (isConnected)
            {
                Handles.DrawWireDisc(bounds.center, Vector3.forward, 10.0f);
            }
        }
        

        protected void DrawConnectionPoints()
        {
            //Draw In Point
            if (Node.PointIn != null)
            {
                Rect pointBoundsIn = new Rect((nodeWidth / 2 - 10), 0, widthConnectionPoint, heightConnectionPoint);
                DrawPoint(pointBoundsIn, Node.PointIn.Color, Node.PointIn.ConnectedTo != null);
            }

            //Draw Out Point
            DrawOutPoint();
        }

        protected virtual void DrawOutPoint()
        {
            Rect pointBoundsOut = new Rect((nodeWidth / 2 - 10), nodeHeight - 16, widthConnectionPoint, heightConnectionPoint);
            DrawPoint(pointBoundsOut, Node.PointOut.FirstOrDefault().Color, Node.PointOut.FirstOrDefault().ConnectedTo != null);
        }

    }
}
