using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.NodeCommands;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.RydenCam.Scripts.Editor.NodeDrawer
{
    public class StartNodeDrawer : NodeDrawerBase
    {
        private StartNode startNode { get; set; }
        private StartNodeCommand startCommand { get; set; }

        public override float InspectorWidth => 335;

        public StartNodeDrawer(Node node) : base(node)
        {
            startNode = node as StartNode;
            startCommand = new StartNodeCommand(node);

            labelStyle = new GUIStyle();
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 10;

            WindowRect = new Rect(node.EditorPosition.x, node.EditorPosition.y, node.NodeWidth, node.NodeHeight);

            ColorUtility.TryParseHtmlString("#009900", out Color colorref);
            NodeColor = colorref;
        }

        public override void DrawNode(int index)
        {
            GUI.backgroundColor = Color.gray;

            WindowRect =
                GUI.Window(index, WindowRect,
                    (windowId) =>
                    {

                        GUI.DrawTextureWithTexCoords(new Rect(0, 0, 200.0f, 25.0f), HeaderTexture, new Rect(0, 0, 1, 1.0f));
                        EditorGUI.LabelField(new Rect(4, 4, Node.NodeWidth, Node.NodeHeight), "Start", labelStyleHead_Node);

                        EditorGUILayout.LabelField(startNode.SequenceName, labelStyleHead_Node);

                        Rect deleteButtonRect = new Rect(Node.NodeWidth - 20, 0, 20, 20);

                        if (GUI.Button(deleteButtonRect, "X"))
                        {
                            startCommand.RemoveNode();
                        }

                        DrawConnectionPoints();

                        GUI.DragWindow();

                    }, "");

            if (IsActive) HighlightSelctedNode();

            Node.EditorPosition = new Vector2(WindowRect.x, WindowRect.y);
        }



        public override void DrawNodeInspector()
        {
            EditorGUIUtility.labelWidth = 75;
            EditorGUILayout.LabelField("Scene Info", labelStyleHead_Panel);
            EditorGUILayout.Space();

            //Scene Name
            EditorGUILayout.LabelField("Sequence Name", inspectorText);
            startNode.SequenceName = EditorGUILayout.TextField(startNode.SequenceName);
            EditorGUILayout.LabelField("Camera Side", inspectorText);
            startNode.CameraSide = (Side)EditorGUILayout.EnumPopup(startNode.CameraSide);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actors in Scene", labelStyleHead_Panel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Add Actor", GUILayout.Width(80), GUILayout.Height(25)))
            {
                startCommand.AddActor();
            }

            for (int actorIndex = 0; actorIndex < startNode.ActorsInScene.Count; actorIndex++)
            {
                using (var actorListingsScope = new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Actor " + (actorIndex + 1), labelStyleHead_Node);

                    if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        startCommand.RemoveActor(actorIndex);
                        break;
                    }
                }

                using (var focusTargetsScope = new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Focus Target", inspectorText, GUILayout.Width(80));
                    startNode.ActorsInScene[actorIndex].ActorGO = (GameObject)EditorGUILayout.ObjectField(startNode.ActorsInScene[actorIndex].ActorGO, typeof(GameObject), true);
                }
            }

            using (var startPositionSettingsScope = new GUILayout.VerticalScope())
            {
                EditorGUIUtility.labelWidth = 50;
                EditorGUILayout.Space(15f);
                GUIContent predefinedPositionsLabel = new GUIContent("Use Predefined Start Positions", "Enable this option to use predefined positions for characters in the dialogue.You can set current character positions as the start points for the conversation. If enabled, characters will return to their original positions after the dialogue ends.");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(predefinedPositionsLabel, labelStyleHead_Node, GUILayout.Width(250));
                EditorGUILayout.Space(50);
                startNode.StartPositionsEnabled = EditorGUILayout.Toggle(startNode.StartPositionsEnabled, GUILayout.Width(20f), GUILayout.Height(20f));
                EditorGUILayout.EndHorizontal();


                //Predefined positions enabled
                if (startNode.StartPositionsEnabled)
                {
                    string labelvalue = (string.IsNullOrEmpty(startNode.UnitySceneName)) ? "<Not Assigned>" : startNode.UnitySceneName;
                    EditorGUILayout.LabelField("Unity Scene Name: " + labelvalue, inspectorText);
                    EditorGUILayout.Space(5f);

                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("Assign Actor Start Positions", GUILayout.Width(200), GUILayout.Height(25)))
                    {
                        startCommand.AssignActorStartPositionData();
                    }

                    if (startNode.ActorsInScene.Where(x => x.PreDefinedStartPositionEnabled).Any())
                    {
                        if (GUILayout.Button("Clear", GUILayout.Width(100), GUILayout.Height(25)))
                        {
                            startCommand.ClearActorsStartPositinonData();
                        }
                    }

                    EditorGUILayout.EndHorizontal();


                    foreach (var actorInfo in NodeManager.Instance.ActorsInScene)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(actorInfo.ActorName, inspectorTextBold);
                        string positionset = startCommand.GetPreDefinedStartPositionDisplayData(actorInfo);
                        EditorGUILayout.LabelField(positionset, inspectorText);
                        EditorGUILayout.Space();
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space(10f);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Look at each other (Override Rotations)", GUILayout.Height(40f));
                    startNode.OverrideRotation = EditorGUILayout.Toggle(startNode.OverrideRotation, GUILayout.Width(40f), GUILayout.Height(40f));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(5f);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Return to original positions\nwhen conversation ends", GUILayout.Height(40f));
                    startNode.ReturnToOriginalPositions = EditorGUILayout.Toggle(startNode.ReturnToOriginalPositions, GUILayout.Width(40f), GUILayout.Height(40f));
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }

}
