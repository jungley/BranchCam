using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.NodeCommands;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.PreviewRender;
using RydenCam.Common;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;

namespace Assets.RydenCam.Scripts.Editor.NodeDrawer
{
    internal class DialogueNodeDrawer : NodeDrawerBase
    {
        private DialogueNode node { get; set; }
        private DialogueNodeCommand command { get; set; }
        private DialoguePreview<DialogueNode> preview { get; set; }
        private NodeCameraOptionsDrawer nodeCameraOptionsDrawer { get; set; }

        private GUIStyle textAreaStyleNode { get; set; }

        private GUIStyle textAreaStyleInspector { get; set; }

        private Vector2 scrollPosInspector { get; set; }
        private int ActorEditorDropdownIndex { get; set; }


        public DialogueNodeDrawer(Node _node) : base(_node)
        {
            node = _node as DialogueNode;
            command = new DialogueNodeCommand(node);
            preview = new DialoguePreview<DialogueNode>(node);

            nodeCameraOptionsDrawer = new NodeCameraOptionsDrawer(node, inspectorText, labelStyleHead_Panel);
            nodeCameraOptionsDrawer.OnPropertyChange += () => preview.UpdateShotRender();


            WindowRect = new Rect(node.EditorPosition.x, node.EditorPosition.y, node.NodeWidth, node.NodeHeight);

            ColorUtility.TryParseHtmlString("#1700FF", out Color colorref);
            NodeColor = colorref;

            ActorEditorDropdownIndex = node?.NodeConvodata?.Actor?.ActorName is string actorName
                ? NodeManager.Instance.ActorsInScene().FindIndex(actor => actor.ActorName == actorName)
                : -1;

        }

        public override void DeSelect()
        {
            command.CustomCameraCommand.ClearCameraSceneObject();
        }

        public override void DrawNode(int index)
        {
            GUI.backgroundColor = Color.gray;

            WindowRect = GUI.Window(index, new Rect(node.EditorPosition.x, node.EditorPosition.y, node.NodeWidth, node.NodeHeight),
                (windowId) =>
                {

                    GUI.DrawTextureWithTexCoords(new Rect(0, 0, 280.0f, 25.0f), HeaderTexture, new Rect(0, 0, 1, 1.0f));
                    EditorGUI.LabelField(new Rect(4, 4, node.NodeWidth, node.NodeHeight), "Dialogue", labelStyleHead_Node);

                    EditorGUILayout.LabelField(node.NodeConvodata.Actor == null
                        ? BranchConstants.UnAssignedActor
                        : node.NodeConvodata.Actor.ActorName,
                        labelStyleHead_Node);

                    for (int i = 0; i < node.NodeConvodata.DialogTextList.Count; i++)
                    {
                        node.NodeConvodata.DialogTextList[i] = EditorGUILayoutExtensions.SetTextAreaExpandable(node.NodeConvodata.DialogTextList[i], textAreaStyleNode, areaHeight: 50, textWidth: node.NodeWidth - 10);
                         GUILayout.Space(5);
                    }

                    Node.NodeHeight = CalculateNodeHeightFromText(node.NodeConvodata.DialogTextList, node.NodeWidth - 10);

                    Rect deleteButtonRect = new Rect(node.NodeWidth - 20, 0, 20, 20);
                    if (GUI.Button(deleteButtonRect, "X"))
                    {
                        command.RemoveNode(node);
                    }

                    DrawConnectionPoints();

                    GUI.DragWindow();

                }, "");

            preview.DrawPreviewWindow();

            if (IsActive) HighlightSelctedNode();

            Node.EditorPosition = new Vector2(WindowRect.x, WindowRect.y);

        }

        public override void DrawNodeInspector()
        {
            EditorGUILayout.LabelField("Dialogue Info", labelStyleHead_Panel);
            EditorGUILayout.Space();
            GUILayout.Label("Actor (Camera Focus Target)", inspectorText, GUILayout.Width(150));

            int indexx = EditorGUILayout.Popup(ActorEditorDropdownIndex, NodeManager.Instance.ActorsInScene().Select(x => x.ActorName).ToArray(), GUILayout.Width(200));
            EditorGUILayout.Space(20);
            //Call when changed
            if (indexx != ActorEditorDropdownIndex)
            {
                command.AssignNewActor(indexx);
                preview.UpdateShotRender();
                ActorEditorDropdownIndex = indexx;
            }

            if (GUILayout.Button("Add Dialogue", GUILayout.Width(100), GUILayout.Height(25)))
            {
                command.AddDialogue();
            }

            scrollPosInspector = EditorGUILayout.BeginScrollView(scrollPosInspector, GUILayout.Width(250), GUILayout.Height(280));


            //Loop through Dialogue to display
            for (int y = 0; y < node.NodeConvodata.DialogTextList.Count; y++)
            {
                using (var horizontalScope224 = new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Dialogue " + (y + 1), inspectorText, GUILayout.Width(180));
                    if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        command.RemoveDialogue(y);
                        break;
                    }
                }


                node.NodeConvodata.DialogTextList[y] = EditorGUILayoutExtensions.SetTextAreaExpandable(node.NodeConvodata.DialogTextList[y], textAreaStyleInspector, areaHeight: 120, textWidth: 200);
            }

            EditorGUIUtility.labelWidth = 75;
            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();

            //Color Banner but cant dyanically change position when file button shifts everything down
            //GUI.DrawTextureWithTexCoords(new Rect(0, 443, 250.0f, 25.0f), HeaderTexture, new Rect(0, 0, 1, 1.0f));

            nodeCameraOptionsDrawer.DrawUICamCompOptions();

        }
    }
}
