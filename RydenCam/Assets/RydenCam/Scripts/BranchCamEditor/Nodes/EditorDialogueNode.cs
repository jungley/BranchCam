using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RydenCam.SequenceData;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using System.Linq;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Serialization;
using RydenCam.BranchCamEditor.Serialization.Saveables;
using RydenCam.BranchCamEditor.PreviewRender;


namespace RydenCam.BranchCamEditor.Nodes
{
    [ExecuteAlways]
    public class EditorDialogueNode : EditorBaseNode, IPositionalNode
    {
        public ConversationData NodeConvodata { get; set; }
        [TextArea]
        private Vector2 scrollPos;
        private Vector2 scrollPosInspector;
        private GUIStyle labelStyle;
        private GUIStyle textareaStyle;
        public int ActorIndex;

        public override NodeType TypeOfNode => NodeType.DialogueNode;

        [ExecuteAlways]
        public EditorDialogueNode(Vector2 mousePos) : base()
        {
            //Instantiate Dialogue
            //Put Actor thats first in the list by default
            NodeConvodata = new ConversationData(EditorController.Instance.ActorsInScene[0]);

            defineStyles();

            //Set First Actor Available
            Sel_ActorID = EditorController.Instance.ActorsInScene[0].ActorID;
            NodeConvodata.Actor = EditorController.Instance.ActorsInScene.Where(x => x.ActorID == Sel_ActorID).FirstOrDefault();

            //Size 
            nodeWidth = 200;
            nodeHeight = 120;

            ColorUtility.TryParseHtmlString("#1700FF", out nodeColor);

            windowRect = new Rect(mousePos.x, mousePos.y, nodeWidth, nodeHeight);

            //Instantiate ConnectionPoints
            PointIn = new ConnectionPoint(this, ConnectionPointType.In);
            PointOut = new List<ConnectionPoint>();
            PointOut.Add(new ConnectionPoint(this, ConnectionPointType.Out));

            //Add 1 Dialogue
            AddDialogue();

            //Add Single Default Shot
            NodeConvodata.ShotConfig = new CamShotConfig(NodeConvodata.Actor.ActorName, CameraGoal.Portrait, CameraDistance.Mid, CameraAngle.EyeLevel, CustomCameraType.None);
        }

        public EditorDialogueNode(Saveable savenode) : base()
        {
            //Cast it down
            SaveableDialogueNode dianode = (SaveableDialogueNode)savenode;

            defineStyles();
            nodeWidth = 200;
            nodeHeight = 120;

            //Saveable info
            windowRect = dianode.windowRect;
            node_id = dianode.node_id;

            ColorUtility.TryParseHtmlString("#1700FF", out nodeColor);

            ActorInfo actor = dianode.NodeConvodata.Actor;
            NodeConvodata = new ConversationData(actor, dianode.NodeConvodata.DialogTextList);

            //Editor Specifications
            Sel_ActorID = actor.ActorID;
            ActorIndex = EditorController.Instance.ActorsInScene.FindIndex(x => x.ActorID == Sel_ActorID);

            //Instantiate ConnectionPoints
            PointIn = new ConnectionPoint(this, ConnectionPointType.In);
            PointOut = new List<ConnectionPoint>();
            PointOut.Add(new ConnectionPoint(this, ConnectionPointType.Out));

            //Set Camera Info
            if (savenode.goal_type == CameraGoal.Custom)
            {
                NodeConvodata.ShotConfig =
                    new CamShotConfig(actor.ActorName, savenode.goal_customtype, savenode.CamPositon, savenode.CamRotation);

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

        public bool ReachedLastDialogueText(int currentIndex) => currentIndex == NodeConvodata.DialogTextList.Count - 1;

        public override EditorBaseNode GetNextNode() => PointOut[0]?.ConnectedTo?.node;
        

        public void AddDialogue() => NodeConvodata.DialogTextList.Add(string.Empty);


        public void defineStyles()
        {
#if UNITY_EDITOR
            //Style HEADERS for shots
            labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.black;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 10;

            //Textarea
            textareaStyle = new GUIStyle(EditorStyles.textArea);
            textareaStyle.wordWrap = true;
            textareaStyle.margin = new RectOffset(20, 0, 0, 0);
#endif
        }

#if UNITY_EDITOR
        public override void DrawForInspector()
        {
            base.DrawForInspector();
            EditorGUILayout.LabelField("Dialogue Info", labelStyleHead_Panel);
            EditorGUILayout.Space();
            GUILayout.Label("Actor (Camera Focus Target)", inspectorText, GUILayout.Width(150));

            int indexx = EditorGUILayout.Popup(ActorIndex, EditorController.Instance.ActorsInScene.Select(x => x.ActorName).ToArray(), GUILayout.Width(200));
            EditorGUILayout.Space(20);
            //Call when changed
            if (indexx != ActorIndex)
            {
                ActorIndex = indexx;
                Sel_ActorID = EditorController.Instance.ActorsInScene[ActorIndex].ActorID;
                NodeConvodata.Actor = EditorController.Instance.ActorsInScene.Where(x => x.ActorID == Sel_ActorID).FirstOrDefault();
                NodeConvodata.ShotConfig.actor = NodeConvodata.Actor.ActorName;
            }

            //ID associated with the Actor no longer exists but Dialgoue contains an actor
            if (!EditorController.Instance.ActorsInScene.Any(x => x.ActorID == Sel_ActorID))
            {
                //Clear actor
                NodeConvodata.Actor = null;
                ActorIndex = -1;
            }

            GUIStyle myTextAreaStyle = new GUIStyle(EditorStyles.textArea);
            myTextAreaStyle.wordWrap = true;

            if (GUILayout.Button("Add Dialogue", GUILayout.Width(100), GUILayout.Height(25)))
            {
                AddDialogue();
            }

            scrollPosInspector = EditorGUILayout.BeginScrollView(scrollPosInspector, GUILayout.Width(250), GUILayout.Height(280));


            //Loop through Dialogue to display
            for (int y = 0; y < NodeConvodata.DialogTextList.Count; y++)
            {
                using (var horizontalScope224 = new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Dialogue " + (y + 1), inspectorText, GUILayout.Width(180));
                    if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        NodeConvodata.DialogTextList.RemoveAt(y);
                        EditorController.Instance.RedrawAll();
                        break;
                    }
                }

                NodeConvodata.DialogTextList[y] = EditorGUILayout.TextArea(NodeConvodata.DialogTextList[y], myTextAreaStyle, GUILayout.Width(200), GUILayout.Height(120));
            }

            EditorGUIUtility.labelWidth = 75;
            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();

            GUI.DrawTextureWithTexCoords(new Rect(0, 443, 250.0f, 25.0f), HeaderTexture, new Rect(0, 0, 1, 1.0f));
            DrawUICamCompOptions(NodeConvodata);
        }


        public override void DrawContent()
        {
            GUIStyle myTextAreaStyle = new GUIStyle(EditorStyles.textArea);
            myTextAreaStyle.wordWrap = true;
            myTextAreaStyle.margin = new RectOffset(-20, 0, 0, 0);

            GUI.DrawTextureWithTexCoords(new Rect(0, 0, 280.0f, 25.0f), HeaderTexture, new Rect(0, 0, 1, 1.0f));
            EditorGUI.LabelField(new Rect(4, 4, nodeWidth, nodeHeight), "Dialogue", labelStyleHead_Node);

            //Check if actor has been assigned first
            if (NodeConvodata.Actor == null)
            {
                try { EditorGUILayout.LabelField(BranchConstants.UnAssignedActor, labelStyleHead_Node); }
                catch (Exception) { };
            }
            else
            {
                try { EditorGUILayout.LabelField(NodeConvodata.Actor.ActorName, labelStyleHead_Node); } catch (Exception) { };
            }

            //Needs to be scrollable for multiple dialogs like decision
            try
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(nodeWidth - 10), GUILayout.Height(nodeHeight - 70));
            }
            catch(Exception) { };

            for (int i = 0; i < NodeConvodata.DialogTextList.Count; i++)
            {
                try { NodeConvodata.DialogTextList[i] = EditorGUILayout.TextArea(NodeConvodata.DialogTextList[i], GUILayout.Width(nodeWidth - 40), GUILayout.Height(20)); } catch (Exception) { };
            }


            EditorGUILayout.EndScrollView();

            //Draw Points
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
            return (PointIn.Bounds.Contains(localPoint) || PointOut[0].Bounds.Contains(localPoint));
        }


        //TODO Move to connection Point
        public override ConnectionPoint getConPoint(Vector2 mousePos)
        {
            ///Convert mousepos to local over the window rect
            //Detect Out ConnectionPoint
            float xPoint = windowRect.x - mousePos.x;
            float yPoint = windowRect.y - mousePos.y;
            Vector2 localPoint = new Vector2(mousePos.x - windowRect.x, mousePos.y - windowRect.y);

            if (PointIn.Bounds.Contains(localPoint))
            {
                return PointIn;
            }
            else if (PointOut[0].Bounds.Contains(localPoint))
            {
                return PointOut[0];
            }
            else
            {
                Debug.Log("This shouldnt have happened");
                return null;
            }
        }
    }
}