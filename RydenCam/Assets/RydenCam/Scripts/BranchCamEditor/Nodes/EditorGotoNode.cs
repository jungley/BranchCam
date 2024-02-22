using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.SequenceData;
using System;
using RydenCam.Common;
using RydenCam.BranchCamEditor.Serialization;
using RydenCam.BranchCamEditor.Serialization.Saveables;

namespace RydenCam.BranchCamEditor.Nodes
{
    [ExecuteAlways]
    [System.Serializable]
    public class EditorGotoNode : EditorBaseNode
    {

        private GUIStyle labelStyle;

        public override NodeType TypeOfNode => NodeType.GoToNode;

        public EditorGotoNode(Vector2 mousePos) : base()
        {
            nodeWidth = 100;
            nodeHeight = 50;
            windowRect = new Rect(mousePos.x, mousePos.y, nodeWidth, nodeHeight);

            //HEADERS for shots
            labelStyle = new GUIStyle();
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 10;

            ColorUtility.TryParseHtmlString("#FF530D", out nodeColor);

            //Instantiate ConnectionPoints
            PointIn = new ConnectionPoint(this, ConnectionPointType.In);
            PointOut = new List<ConnectionPoint>();
            PointOut.Add(new ConnectionPoint(this, ConnectionPointType.Out));
        }

#if UNITY_EDITOR
        public override void DrawForInspector()
        {
            base.DrawForInspector();
            GUI.Label(new Rect(0, 35, 250.0f, 25.0f), "Goes To:", labelStyleHead_Panel);

            EditorGUILayout.Space();
            if (PointOut[0].connectedTo != null)
            {
                NodeType nodetype = PointOut[0].connectedTo.node.TypeOfNode;
                switch(nodetype)
                {
                    case (NodeType.DialogueNode):
                        EditorGUILayout.LabelField("Dialogue Node", labelStyleHead_Panel);
                        break;
                    case (NodeType.DecisionNode):
                        EditorGUILayout.LabelField("Decision Node", labelStyleHead_Panel);
                        break;
                    case (NodeType.GoToNode):
                        EditorGUILayout.LabelField("Go-To Node", labelStyleHead_Panel);
                        break;
                    case (NodeType.ActionNode):
                        EditorGUILayout.LabelField("Action Node", labelStyleHead_Panel);
                        break;
                }
            }
            else
            {
                EditorGUILayout.LabelField("No outward connection", labelStyleHead_Panel);
            }
        }


        public override void DrawContent()
        {
            //Handeling Repaint issue
            EditorGUILayout.LabelField("Go-To", labelStyleHead_Node);
            PointIn.Draw();
            PointOut[0].Draw();
        }
#endif


        public override bool isOverPoint(Vector2 mousePos)
        {
            //Convert mousepos to local over the window rect
            //Detect Out ConnectionPoint
            float xPoint = mousePos.x - windowRect.x;
            float yPoint = mousePos.y - windowRect.y;

            Vector2 localPoint = new Vector2(xPoint, yPoint);

            //If mouseposition is over point
            return (PointIn.pointBounds.Contains(localPoint) || PointOut[0].pointBounds.Contains(localPoint));
        }

        public override ConnectionPoint getConPoint(Vector2 mousePos)
        {
            ///Convert mousepos to local over the window rect
            //Detect Out ConnectionPoint
            float xPoint = windowRect.x - mousePos.x;
            float yPoint = windowRect.y - mousePos.y;
            Vector2 localPoint = new Vector2(mousePos.x - windowRect.x, mousePos.y - windowRect.y);

            if (PointIn.pointBounds.Contains(localPoint))
            {
                return PointIn;
            }
            else if (PointOut[0].pointBounds.Contains(localPoint))
            {
                return PointOut[0];
            }
            else
            {
                BranchLog.Log("This shouldnt have happened");
                return null;
            }
        }

        public void defineStyles()
        {
            //Style HEADERS for shots
            labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.black;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 10;
        }

        public EditorGotoNode(Saveable savenode) : base()
        {
            SaveableGotoNode gonode = savenode as SaveableGotoNode;
            defineStyles();

            ColorUtility.TryParseHtmlString("#FF530D", out nodeColor);

            nodeWidth = 100;
            nodeHeight = 50;
            labelStyle = new GUIStyle();
            labelStyle.fontStyle = FontStyle.Bold;

            windowRect = gonode.windowRect;
            node_id = gonode.node_id;

            //Instantiate ConnectionPoints
            PointIn = new ConnectionPoint(this, ConnectionPointType.In);
            PointOut = new List<ConnectionPoint>();
            PointOut.Add(new ConnectionPoint(this, ConnectionPointType.Out));
        }
    }
}