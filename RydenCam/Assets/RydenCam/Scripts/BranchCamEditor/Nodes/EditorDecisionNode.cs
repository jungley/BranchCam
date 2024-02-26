using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RydenCam.SequenceData;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System.Linq;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Serialization;
using RydenCam.BranchCamEditor.Serialization.Saveables;

namespace RydenCam.BranchCamEditor.Nodes
{
    public class EditorDecisionNode : EditorBaseNode, IPositionalNode
    {
        public ConversationData NodeConvodata { get; set; }

        //Action Delegate
        public Action<EditorBaseNode> OnRemoveNode;
        //Style
        private GUIStyle labelStyle;
        private GUIStyle textareaStyle;
        private GUIStyle nodetextFieldColor;

        //Scroll
        private Vector2 scrollPos;
        private Vector2 scrollPosInspector;

        //Decision Options
        public List<string> DecisionOptions;

        public int ActorIndex;
        public bool ShowPreviousDialog;

        public override NodeType TypeOfNode => NodeType.DecisionNode;

        public EditorDecisionNode(Vector2 mousePos) : base()
        {
            //Instantiate ConvoData
            //Put Actor thats first in the list
            NodeConvodata = new ConversationData(EditorController.Instance.ActorsInScene[0]);
            ShowPreviousDialog = true;

            defineStyles();
            //Set First Actor Available
            ActorIndex = 0;
            Sel_ActorID = EditorController.Instance.ActorsInScene[ActorIndex].ActorID;
            NodeConvodata.Actor = EditorController.Instance.ActorsInScene.Where(x => x.ActorID == Sel_ActorID).FirstOrDefault();

            ColorUtility.TryParseHtmlString("#990099", out nodeColor);

            //Size
            nodeHeight = 150;
            nodeWidth = 200;
            //Set the WindowRect
            windowRect = new Rect(mousePos.x, mousePos.y, nodeWidth, nodeHeight);

            //declare decision size
            DecisionOptions = new List<string>();
            PointOut = new List<ConnectionPoint>();
            PointIn = new ConnectionPoint(this, ConnectionPointType.In);

            //Add 2 Decisons by Default
            addDecision();
            addDecision();

            //Add Default Camera Position
            NodeConvodata.ShotConfig = new CamShotConfig(NodeConvodata.Actor.ActorName, CameraGoal.Portrait, CameraDistance.Mid, CameraAngle.EyeLevel, CustomCameraType.None);
        }

        public EditorBaseNode MakeDecision(int choiceIndex)
        {
            return PointOut[choiceIndex].connectedTo?.node;
        }


        public void defineStyles()
        {
#if UNITY_EDITOR
            //Style HEADERS for shots
            labelStyle = new GUIStyle();
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 10;

            //Textarea
            textareaStyle = new GUIStyle(EditorStyles.textArea);
            textareaStyle.wordWrap = true;
            textareaStyle.margin = new RectOffset(20, 0, 0, 0);

            //nodetextFieldColor
            nodetextFieldColor = new GUIStyle();
            nodetextFieldColor.normal.textColor = Color.white;
#endif
        }

        public override void DrawForInspector()
        {
#if UNITY_EDITOR
            base.DrawForInspector();
            EditorGUILayout.LabelField("Decision Info", labelStyleHead_Panel);
            EditorGUILayout.Space();
            GUILayout.Label("Actor", inspectorText, GUILayout.Width(150));

            int indexx = EditorGUILayout.Popup(ActorIndex, EditorController.Instance.ActorsInScene.Select(x => x.ActorName).ToArray(), GUILayout.Width(200));
            //Call when changed
            if (indexx != ActorIndex)
            {
                ActorIndex = indexx;
                Sel_ActorID = EditorController.Instance.ActorsInScene[ActorIndex].ActorID;
                NodeConvodata.Actor = EditorController.Instance.ActorsInScene.Where(x => x.ActorID == Sel_ActorID).FirstOrDefault();
            }

            //ID associated with the Actor no longer exists but Dialgoue contains an actor
            if (!EditorController.Instance.ActorsInScene.Any(x => x.ActorID == Sel_ActorID))
            {
                //Clear actor
                NodeConvodata.Actor = null;
                ActorIndex = -1;
            }

            using (var horizontalScope33 = new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Show Previous Dialog", inspectorText, GUILayout.Width(150));
                ShowPreviousDialog = EditorGUILayout.Toggle(ShowPreviousDialog);
            }

            EditorGUILayout.Space();


            //Add Choice button
            //Cap at 9 Choices
            if (GUILayout.Button("Add Choice", GUILayout.Width(80), GUILayout.Height(25)) && PointOut.Count < 9)
            {
                addDecision();
                adjustOutLocations();
                //Redraw
                EditorController.Instance.RedrawAll();
            }

            scrollPosInspector = EditorGUILayout.BeginScrollView(scrollPosInspector, GUILayout.Width(250), GUILayout.Height(280));

            //Loop through choices
            for (int i = 0; i < DecisionOptions.Count; i++)
            {
                using (var horizontalScope2 = new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Choice " + (i + 1), inspectorText, GUILayout.Width(195));
                    if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        ConnectionManager.Instance.Remove(PointOut[i]);
                        PointOut.RemoveAt(i);
                        DecisionOptions.RemoveAt(i);
                        adjustOutLocations();
                        EditorController.Instance.RedrawAll();
                        break;
                    }
                }
                DecisionOptions[i] = EditorGUILayout.TextArea(DecisionOptions[i], textareaStyle, GUILayout.Width(200), GUILayout.Height(60));
            }
            EditorGUILayout.EndScrollView();

            GUI.DrawTextureWithTexCoords(new Rect(0, 443, 250.0f, 25.0f), HeaderTexture, new Rect(0, 0, 1, 1.0f));
            DrawUICamCompOptions(NodeConvodata);
#endif
        }


        public override bool isOverPoint(Vector2 mousePos)
        {
            //Convert mousepos to local over the window rect
            //Detect Out ConnectionPoint
            float xPoint = mousePos.x - windowRect.x;
            float yPoint = mousePos.y - windowRect.y;

            Vector2 localPoint = new Vector2(xPoint, yPoint);
            for (int i = 0; i < PointOut.Count; i++)
            {
                if (PointOut[i].pointBounds.Contains(localPoint))
                {
                    return true;
                }
            }

            return (PointIn.pointBounds.Contains(localPoint));
        }

        public override ConnectionPoint getConPoint(Vector2 mousePos)
        {
            //Convert mousepos to local over the window rect
            //Detect Out ConnectionPoint
            float xPoint = mousePos.x - windowRect.x;
            float yPoint = mousePos.y - windowRect.y;

            Vector2 localPoint = new Vector2(xPoint, yPoint);
            if (PointIn.pointBounds.Contains(localPoint))
            {
                return PointIn;
            }

            for (int i = 0; i < PointOut.Count; i++)
            {
                if (PointOut[i].pointBounds.Contains(localPoint))
                {
                    return PointOut[i];
                }
            }

            //Did not find anything 
            return null;
        }

        //Out Point positions on the bottom of the decision node
        public void adjustOutLocations()
        {
            float lineLength = (nodeWidth - 10);
            int dotCount = PointOut.Count;
            float spacing = lineLength / dotCount;
            float startPos = 0;
            if (dotCount % 2 == 0)
            {
                startPos = (lineLength / 2) - (spacing / 2);
                startPos = startPos - ((dotCount / 2 - 1) * spacing);
            }
            else
            {
                startPos = (lineLength / 2) - (((dotCount - 1) / 2) * spacing);
            }

            for (int i = 0; i < dotCount; i++)
            {
                PointOut[i].pointBounds = new Rect((startPos), nodeHeight - 16, 20, 18);
                startPos += spacing;
            }
        }

        public void addDecision()
        {
            DecisionOptions.Add("");
            PointOut.Add(new ConnectionPoint(this, ConnectionPointType.Out));
            adjustOutLocations();
        }

        public override void DrawContent()
        {
#if UNITY_EDITOR
            //Draws close button
            GUI.DrawTextureWithTexCoords(new Rect(0, 0, 200.0f, 25.0f), HeaderTexture, new Rect(0, 0, 1, 1.0f));
            EditorGUI.LabelField(new Rect(4, 4, nodeWidth, nodeHeight), "Decision", labelStyleHead_Node);

            PointIn.Draw();

            EditorGUILayout.Space();

            if (NodeConvodata.Actor == null)
            {
                EditorGUILayout.LabelField(BranchConstants.UnAssignedActor, labelStyleHead_Node);
            }
            else
            {
                EditorGUILayout.LabelField(NodeConvodata.Actor.ActorName, labelStyleHead_Node);
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(nodeWidth - 10), GUILayout.Height(nodeHeight - 70));

            //Formats/Displays each decision option and text in node
            for (int k = 0; k < DecisionOptions.Count; k++)
            {
                EditorGUILayout.LabelField("", GUILayout.Height(28.0f), GUILayout.Width(100));
                EditorGUILayout.BeginHorizontal();
                Color oldCol = GUI.color;
                GUI.color = Color.black;
                GUI.Box(new Rect(0, (k) * 30, 200, 30), "");
                GUI.color = oldCol;
                GUI.Label(new Rect(10, ((k) * 30) + 5, 100, 20), "" + (k + 1), labelStyleHead_Node);
                DecisionOptions[k] = GUI.TextField(new Rect(25, ((k) * 30) + 5, 130, 20), DecisionOptions[k]);
                GUI.color = oldCol;
                EditorGUILayout.EndHorizontal();
            }


            EditorGUILayout.EndScrollView();

            for (int i = 0; i < PointOut.Count; i++)
            {
                PointOut[i].Draw((i + 1));
            }
#endif
        }

        public EditorDecisionNode(Saveable savenode) : base()
        {
            //Cast it down
            SaveableDecisionNode decnode = (SaveableDecisionNode)savenode;

            defineStyles();
            //Set First Actor Available
            ActorInfo actor = decnode.NodeConvodata.Actor;
            decnode.NodeConvodata = new ConversationData(actor);
            NodeConvodata = decnode.NodeConvodata;
            ShowPreviousDialog = decnode.ShowPreviousDialog;

            //Editor Specifications
            ActorIndex = EditorController.Instance.ActorsInScene.FindIndex(x => x.ActorID == actor.ActorID);
            Sel_ActorID = actor.ActorID;

            ColorUtility.TryParseHtmlString("#990099", out nodeColor);

            //Size
            nodeHeight = 150;
            nodeWidth = 200;
            //Set the WindowRect
            windowRect = decnode.windowRect;
            node_id = decnode.node_id;

            //declare decision size
            DecisionOptions = new List<string>();
            DecisionOptions = decnode.DecisionOptions;
            PointOut = new List<ConnectionPoint>();
            for (int i = 0; i < DecisionOptions.Count; i++)
            {
                PointOut.Add(new ConnectionPoint(this, ConnectionPointType.Out));
            }
            adjustOutLocations();

            PointIn = new ConnectionPoint(this, ConnectionPointType.In);

            //Enum.TryParse(savenode.goal_type, out CameraGoal saveNode_goal_type);
            //Enum.TryParse(savenode.goal_customtype, out CustomCameraType saveNode_custom_type);
            //Set Camera Info
            if (savenode.goal_type == CameraGoal.Custom)
            {
                NodeConvodata.ShotConfig = new CamShotConfig(actor.ActorName, savenode.goal_customtype, savenode.CamPositon, savenode.CamRotation);
                NodeConvodata.ShotConfig.LocalRelativeActorPos = savenode.LocalActorPos;
                NodeConvodata.ShotConfig.LocalRelativeActorRot = savenode.LocalActorRot;
            }
            else
            {
                NodeConvodata.ShotConfig = new CamShotConfig(actor.ActorName, savenode.goal_type, savenode.goal_dist, savenode.goal_angle, savenode.goal_customtype);
            }

            NodeConvodata.ShotConfig.oppositeActor = savenode.oppositeActor;
            NodeConvodata.ShotConfig.actor = actor.ActorName;
        }
    }
}
