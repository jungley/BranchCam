using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.NodeCommands;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using RydenCam.BranchCamEditor.PreviewRender;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions.DatatStructures;

namespace Assets.RydenCam.Scripts.Editor.NodeDrawers
{
    internal class DialogueNodeDrawer : NodeDrawer, IClearable
    {
        private DialogueNodeCommand dialogueCommand;
        private DialogueNode dialogueNode { get; set; }
        protected override NodeCommand Command => dialogueCommand;
        public override Node Node
        {
            get => dialogueNode;
            set => dialogueNode = value as DialogueNode;
        }


        private DialoguePreview<DialogueNode> preview { get; set; }
        private NodeCamShotSelector nodeCamShotSelector { get; set; }

        private Vector2 scrollPosInspector { get; set; }
        private int ActorEditorDropdownIndex { get; set; }

        public override float InspectorWidth => 245;

        public DialogueNodeDrawer(Node _node) : base(_node)
        {
            dialogueNode = _node as DialogueNode;
            dialogueCommand = new DialogueNodeCommand(dialogueNode);
            preview = new DialoguePreview<DialogueNode>(dialogueNode);

            nodeCamShotSelector = new NodeCamShotSelector(dialogueNode, inspectorText, labelStyleHead_Panel);
            nodeCamShotSelector.UpdateShotRender += () => preview.UpdateShotRender();


            dialogueCommand.WindowRect = new Rect(dialogueNode.EditorPosition.x, dialogueNode.EditorPosition.y, dialogueNode.NodeWidth, dialogueNode.NodeHeight);

            dialogueCommand.TextAreaRectIndex = new TwoWayDictionary<int, Rect>();

            ColorUtility.TryParseHtmlString("#1700FF", out Color colorref);
            Command.NodeColor = colorref;

            ActorEditorDropdownIndex =
                dialogueNode?.NodeConvodata?.Actor?.ActorName is string actorName
                ? GetActorIndex(actorName)
                : -1;

        }

        public override void DrawNode(int index)
        {
            int buffer = 42;
            Command.WindowRect = GUI.Window(index, new Rect(dialogueNode.EditorPosition.x, dialogueNode.EditorPosition.y, dialogueNode.NodeWidth, dialogueNode.NodeHeight),
                (windowId) =>
                {

                    GUI.DrawTextureWithTexCoords(new Rect(0, 0, 280.0f, 25.0f), HeaderTexture, new Rect(0, 0, 1, 1.0f));
                    EditorGUI.LabelField(new Rect(4, 4, dialogueNode.NodeWidth, dialogueNode.NodeHeight), "Dialogue", labelStyleHead_Node);


                    int indexx = EditorGUILayout.Popup(ActorEditorDropdownIndex, NodeManager.Instance.ActorsInScene.Select(x => x.ActorName).ToArray(), GUILayout.Width(200));
                    if (indexx != ActorEditorDropdownIndex)
                    {
                        dialogueCommand.AssignNewActor(indexx);
                        preview.UpdateShotRender();
                        ActorEditorDropdownIndex = indexx;
                    }

                    dialogueCommand.TextAreaRectIndex.Clear();
                    for (int i = 0; i < dialogueNode.NodeConvodata.DialogTextList.Count; i++)
                    {
                        dialogueNode.NodeConvodata.DialogTextList[i] = EditorGUILayoutExtensions.SetTextAreaExpandable(dialogueCommand.WindowRect, dialogueCommand.TextAreaRectIndex, i, ref buffer, dialogueNode.NodeConvodata.DialogTextList[i], textAreaStyleNode, areaHeight: 50, textWidth: dialogueNode.NodeWidth - 10);
                        GUILayout.Space(5);
                    }

                    dialogueNode.NodeHeight = CalculateNodeHeightFromText(dialogueNode.NodeConvodata.DialogTextList, dialogueNode.NodeWidth - 10);
                    
                    Rect deleteButtonRect = new Rect(dialogueNode.NodeWidth - 20, 0, 20, 20);
                    if (GUI.Button(deleteButtonRect, "X"))
                    {
                        Command.RemoveNode();
                    }

                    DrawConnectionPoints();

                    GUI.DragWindow();

                }, "");

            preview.DrawPreviewWindow();

            Command.HighlightIfActive();

            Node.EditorPosition = new Vector2(Command.WindowRect.x, Command.WindowRect.y);

        }

        public override void DrawNodeInspector()
        {
            EditorGUILayout.LabelField("Dialogue Info", labelStyleHead_Panel);
            EditorGUILayout.Space();
            GUILayout.Label("Actor (Camera Focus Target)", inspectorText, GUILayout.Width(150));

            int indexx = EditorGUILayout.Popup(ActorEditorDropdownIndex, NodeManager.Instance.ActorsInScene.Select(x => x.ActorName).ToArray(), GUILayout.Width(200));
            EditorGUILayout.Space(10);
            //Call when changed
            if (indexx != ActorEditorDropdownIndex)
            {
                dialogueCommand.AssignNewActor(indexx);
                preview.UpdateShotRender();
                ActorEditorDropdownIndex = indexx;
            }

            nodeCamShotSelector?.DrawUICamCompOptions();

        }

        public void Clear()
        {
            //dialogueCommand.CustomCameraCommand.ClearCameraSceneObject();
        }
    }
}
