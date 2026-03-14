using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
using Assets.RydenCam.Scripts.NodeCommands;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using RydenCam.Editor.Styling;

namespace Assets.RydenCam.Scripts.Editor
{

    //Responsible for Drawing the UI components of the Node
    //Called from Drawer factory in NodeGraphViewModel
    //Draws the node based on the node passed
    public abstract class NodeDrawer
    {
        public abstract Node Node { get; set; }

        protected abstract NodeCommand Command { get; }

        protected GUIStyle labelStyle { get; set; }

        public virtual float InspectorWidth => 245;

        public bool IsActive => NodeManager.Instance.ActiveNode?.NodeId == Node.NodeId;

        private Texture2D _headerTexture { get; set; }
        protected Texture2D HeaderTexture
        {
            get
            {
                if (_headerTexture == null)
                {
                    _headerTexture = new Texture2D(1, 1);
                    _headerTexture.SetPixel(0, 0, Command.NodeColor);
                    _headerTexture.Apply();
                }

                return _headerTexture;
            }
        }


        protected GUIStyle labelStyleHead_Panel { get; set; }
        protected GUIStyle labelStyleHead_Node { get; set; }
        protected GUIStyle inspectorText { get; set; }
        protected GUIStyle inspectorTextBold { get; set; }
        protected GUIStyle textAreaStyleNode { get; set; }
        protected GUIStyle textAreaStyleInspector { get; set; }

        public int WindowId { get; set; }



        public NodeDrawer(Node node)
        {
            Node = node;

            //Styles used in Nodes
            labelStyleHead_Panel = new GUIStyle();
            labelStyleHead_Panel.normal.textColor = BranchCamEditorTheme.TextPrimary;
            labelStyleHead_Panel.fontStyle = FontStyle.Bold;
            labelStyleHead_Panel.fontSize = BranchCamEditorTheme.FontTitle;

            labelStyleHead_Node = new GUIStyle();
            labelStyleHead_Node.normal.textColor = BranchCamEditorTheme.TextPrimary;
            labelStyleHead_Node.fontStyle = FontStyle.Bold;
            labelStyleHead_Node.fontSize = BranchCamEditorTheme.FontBody;

            //TextArea Node
            textAreaStyleNode = new GUIStyle(EditorStyles.textArea);
            textAreaStyleNode.wordWrap = true;
            textAreaStyleNode.alignment = TextAnchor.MiddleCenter;
            textAreaStyleNode.fontSize = BranchCamEditorTheme.FontBody;


            //TextArea Inspector
            textAreaStyleInspector = new GUIStyle(EditorStyles.textArea);
            textAreaStyleInspector.wordWrap = true;
            textAreaStyleInspector.margin = new RectOffset(-20, 0, 0, 0);
            textAreaStyleInspector.fontSize = BranchCamEditorTheme.FontBody;

            inspectorText = new GUIStyle();
            inspectorText.normal.textColor = BranchCamEditorTheme.TextSecondary;
            inspectorText.fontSize = BranchCamEditorTheme.FontBody;

            inspectorTextBold = new GUIStyle();
            inspectorTextBold.normal.textColor = BranchCamEditorTheme.TextPrimary;
            inspectorTextBold.fontStyle = FontStyle.Bold;
            inspectorTextBold.fontSize = BranchCamEditorTheme.FontBody;
        }

        public abstract void DrawNode(int index);

        public abstract void DrawNodeInspector();

        protected int heightConnectionPoint => 18;
        protected int widthConnectionPoint => 20;

        protected static float SnapToPixel(float value)
        {
            float pixelsPerPoint = Mathf.Max(1f, EditorGUIUtility.pixelsPerPoint);
            return Mathf.Round(value * pixelsPerPoint) / pixelsPerPoint;
        }

        protected static Vector2 SnapToPixel(Vector2 value)
        {
            return new Vector2(SnapToPixel(value.x), SnapToPixel(value.y));
        }
        
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

        protected int GetActorIndex(string name)
        {
            var list = NodeManager.Instance.ActorsInScene;
            for (int i = 0; i < list.Count; i++)
                if (list[i].ActorName == name)
                    return i;
            return -1;
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
            var firstOut = Node.PointOut?.FirstOrDefault();
            if (firstOut == null) return;

            Rect pointBoundsOut = new Rect((Node.NodeWidth / 2 - 10), Node.NodeHeight - 16, widthConnectionPoint, heightConnectionPoint);
            firstOut.LocalBounds = pointBoundsOut;
            DrawPoint(pointBoundsOut, firstOut.ConnectedTo != null);
        }
    }
}
