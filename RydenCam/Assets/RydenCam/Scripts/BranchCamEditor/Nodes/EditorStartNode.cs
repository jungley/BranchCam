using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.SequenceData;
using RydenCam.Common;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Managers;
using System.Linq;
using RydenCam.BranchCamEditor.Serialization;
using Assets.RydenCam.Scripts.BranchCamEditor.Serialization.Saveables;

namespace RydenCam.BranchCamEditor.Nodes
{
    public class EditorStartNode : EditorBaseNode
    {
        private GUIStyle labelStyle;
        public string SequenceName = string.Empty;
        public Side CameraSide;
        public override NodeType TypeOfNode => NodeType.StartNode;

        //start Position variables
        public string UnitySceneName;
        public bool StartPositionsEnabled;
        public bool OverrideRotation;
        public bool ReturnToOriginalPositions;
        public List<Pose> OriginalPositions;
        public List<Pose> SetStartPositions;

        public EditorStartNode(Vector2 mousePos) : base()
        {
            //Editor Window Properties
            nodeWidth = 200;
            nodeHeight = 70;
            windowRect = new Rect(mousePos.x, mousePos.y, nodeWidth, nodeHeight);

            ColorUtility.TryParseHtmlString("#009900", out nodeColor);

            //HEADERS for shots
            labelStyle = new GUIStyle();
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 10;

            //Out Point
            PointOut = new List<ConnectionPoint>();
            PointOut.Add(new ConnectionPoint(this, ConnectionPointType.Out));

        }

        public EditorStartNode(Saveable savenode) : base()
        {
            SaveableStartNode stnode = savenode as SaveableStartNode;
            SequenceName = stnode.SequenceName;
            node_id = stnode.node_id;
            CameraSide = stnode.CameraSide;
            nodeWidth = 200;
            nodeHeight = 70;
            StartPositionsEnabled = stnode.startPositionsEnabled;
            ReturnToOriginalPositions = stnode.returnToOriginalPositions;
            EditorController.Instance.ActorsInScene = stnode.ActorsInScene;
            PointOut = new List<ConnectionPoint>();
            PointOut.Add(new ConnectionPoint(this, ConnectionPointType.Out));
#if UNITY_EDITOR
            labelStyle = new GUIStyle();
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 10;
            windowRect = stnode.windowRect;
            SequenceName = stnode.SequenceName;
            ColorUtility.TryParseHtmlString("#009900", out nodeColor);
            BranchCamEditor.startNodeAdded = true;
#endif
        }

        public override void DrawForInspector()
        {
#if UNITY_EDITOR
            EditorGUIUtility.labelWidth = 75;
            EditorGUILayout.LabelField("Scene Info", labelStyleHead_Panel);
            EditorGUILayout.Space();

            //Scene Name
            EditorGUILayout.LabelField("Sequence Name", inspectorText);
            SequenceName = EditorGUILayout.TextField(SequenceName);
            EditorGUILayout.LabelField("Camera Side", inspectorText);
            CameraSide = (Side)EditorGUILayout.EnumPopup(CameraSide);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actors in Scene", labelStyleHead_Panel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Add Actor", GUILayout.Width(80), GUILayout.Height(25)))
            {
                EditorController.Instance.ActorsInScene.Add(new ActorInfo());
            }

            for (int i = 0; i < EditorController.Instance.ActorsInScene.Count; i++)
            {
                using (var horizontalScope2 = new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Actor " + (i + 1), labelStyleHead_Node);

                    if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        EditorController.Instance.ActorsInScene.RemoveAt(i);
                        //Redraw All Nodes
                        EditorController.Instance.RedrawAll();
                        break;
                    }
                }

                using (var horizontalScope33 = new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Actor", inspectorText, GUILayout.Width(80));
                    var previousActorGO = EditorController.Instance.ActorsInScene[i].ActorGO;
                    EditorController.Instance.ActorsInScene[i].ActorGO = (GameObject)EditorGUILayout.ObjectField(EditorController.Instance.ActorsInScene[i].ActorGO, typeof(GameObject), true);
                    if (previousActorGO != EditorController.Instance.ActorsInScene[i].ActorGO)
                    {
                        ActorInfo actorInfo = EditorController.Instance.ActorsInScene[i];
                        //Update the actor reference and other notes
                        actorInfo.ActorName = (actorInfo.ActorGO != null) ? actorInfo.ActorGO.name : BranchConstants.UnAssignedActor;
                        NodeManager.Instance.ReplaceActorInfo(previousActorGO?.name, actorInfo);
                    }
                }
            }


            //FOR SETTING THE PREDEFINED START POSITIONS
            using (var horizontalScope323 = new GUILayout.VerticalScope())
            {
                float originalVal = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 50;
                EditorGUILayout.Space(15f);
                GUIContent predefinedPositionsLabel = new GUIContent("Use Predefined Start Positions", "Enable this option to use predefined positions for characters in the dialogue.You can set current character positions as the start points for the conversation. If enabled, characters will return to their original positions after the dialogue ends.");

                GUILayout.BeginHorizontal();
                GUILayout.Label(predefinedPositionsLabel, labelStyleHead_Node, GUILayout.Width(250));
                GUILayout.Space(50);
                StartPositionsEnabled = EditorGUILayout.Toggle(StartPositionsEnabled, GUILayout.Width(20f), GUILayout.Height(20f));

                GUILayout.EndHorizontal();

                //Prefined positions disabled
                if (!StartPositionsEnabled)
                {
                    //clear out actor positions
                    EditorController.Instance.ActorsInScene.ForEach(x => x.PreDefinedStartPosition.position = Vector3.zero);
                    ReturnToOriginalPositions = false;
                }

                //Predefined positions enabled
                if (StartPositionsEnabled)
                {
                    string labelvalue = (string.IsNullOrEmpty(UnitySceneName)) ? "<Not Assigned>" : UnitySceneName;
                    EditorGUILayout.LabelField("Unity Scene Name: " + labelvalue, inspectorText);
                    EditorGUILayout.Space(5f);

                    if (GUILayout.Button("Set Actor Start Positions", GUILayout.Width(200), GUILayout.Height(25)))
                    {
                        try
                        {
                            EditorController.Instance.ActorsInScene.ForEach(actor => actor.PreDefinedStartPosition
                            = new Pose(actor.ActorGO.transform.root.position, actor.ActorGO.transform.root.rotation));
                            UnitySceneName = SceneManager.GetActiveScene().name;
                        }
                        catch
                        {
                            BranchLog.Error("Error happened setting the start positions");

                            EditorController.Instance.ActorsInScene.ForEach(actor => actor.PreDefinedStartPosition = new Pose(Vector3.zero, Quaternion.identity));
                            UnitySceneName = string.Empty;
                        }
                    }

                    foreach (var actorInfo in EditorController.Instance.ActorsInScene)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(actorInfo.ActorName, inspectorTextBold);
                        string positionset = (actorInfo.PreDefinedStartPosition.position == Vector3.zero) ? "<Not Assigned>" : "Position Set ✓";
                        EditorGUILayout.LabelField(positionset, inspectorText);
                        EditorGUILayout.Space();
                        GUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space(10f);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Look at each other", GUILayout.Height(40f));
                    OverrideRotation = EditorGUILayout.Toggle(OverrideRotation, GUILayout.Width(20f), GUILayout.Height(40f));
                    GUILayout.EndHorizontal();
                    EditorGUILayout.Space(5f);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Return to original positions\nwhen conversation ends", GUILayout.Height(40f));
                    ReturnToOriginalPositions = EditorGUILayout.Toggle(ReturnToOriginalPositions, GUILayout.Width(20f), GUILayout.Height(40f));
                    GUILayout.EndHorizontal();
                   
                    EditorGUIUtility.labelWidth = originalVal;
                }
            }
#endif
        }

        public override void DrawContent()
        {
#if UNITY_EDITOR
            GUI.DrawTextureWithTexCoords(new Rect(0, 0, 200.0f, 25.0f), HeaderTexture, new Rect(0, 0, 1, 1.0f));
            EditorGUI.LabelField(new Rect(4, 4, nodeWidth, nodeHeight), "Start", labelStyleHead_Node);

            //Draws Points as Well
            EditorGUILayout.LabelField(SequenceName, labelStyleHead_Node);
            PointOut[0].Draw();
#endif
        }
        public override bool isOverPoint(Vector2 mousePos)
        {
            //Convert mousepos to local over the window rect
            //Detect Out ConnectionPoint
            float xPoint = windowRect.x - mousePos.x;
            float yPoint = windowRect.y - mousePos.y;

            Vector2 localPoint = new Vector2(mousePos.x - windowRect.x, mousePos.y - windowRect.y);
            return PointOut[0].pointBounds.Contains(localPoint);
        }

        public override ConnectionPoint getConPoint(Vector2 mousePos) => PointOut[0];
        
    }
}
