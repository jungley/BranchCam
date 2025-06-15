using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
using Assets.RydenCam.Scripts.NodeCommands;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.PreviewRender;
using RydenCam.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.RydenCam.Scripts.Editor.NodeDrawers
{
    internal class DecisionNodeDrawer : NodeDrawer, IClearable
    {
        private DecisionNodeCommand decisionCommand;
        private DecisionNode decisionNode { get; set; }
        protected override NodeCommand Command => decisionCommand;
        public override Node Node
        {
            get => decisionNode;
            set => decisionNode = value as DecisionNode;
        }

        private DialoguePreview<DecisionNode> preview { get; set; }
        private NodeCameraOptionsDrawer nodeCameraOptionsDrawer { get; set; }
        private int ActorEditorDropdownIndex { get; set; }
        private Vector2 scrollPosInspector { get; set; }
        private GUIStyle decisionOptionNumber { get; set; }

        public DecisionNodeDrawer(Node _node) : base(_node)
        {
            decisionNode = _node as DecisionNode;

            decisionCommand = new DecisionNodeCommand(decisionNode);

            preview = new DialoguePreview<DecisionNode>(decisionNode);

            nodeCameraOptionsDrawer = new NodeCameraOptionsDrawer(decisionNode, inspectorText, labelStyleHead_Panel);
            nodeCameraOptionsDrawer.UpdateShotRender += () => preview.UpdateShotRender();

            decisionCommand.WindowRect = new Rect(decisionNode.EditorPosition.x, decisionNode.EditorPosition.y, decisionNode.NodeWidth, decisionNode.NodeHeight);

            decisionCommand.TextAreaRect = new Dictionary<int, Rect>();

            ColorUtility.TryParseHtmlString("#990099", out Color colorref);
            Command.NodeColor = colorref;

            decisionOptionNumber = new GUIStyle(labelStyleHead_Node);
            decisionOptionNumber.fontSize = 13;
            decisionOptionNumber.normal.textColor = Color.black;

            ActorEditorDropdownIndex = decisionNode?.NodeConvodata?.Actor?.ActorName is string actorName
                ? NodeManager.Instance.ActorsInScene.FindIndex(actor => actor.ActorName == actorName)
                : -1;
        }

        public override void DrawNode(int index)
        {

            int buffer = 42;
            GUI.backgroundColor = Color.gray;

            Command.WindowRect = GUI.Window(index, new Rect(decisionNode.EditorPosition.x, decisionNode.EditorPosition.y, decisionNode.NodeWidth, decisionNode.NodeHeight),
                (windowId) =>
                 {
                     GUI.DrawTextureWithTexCoords(new Rect(0, 0, 200.0f, 25.0f), HeaderTexture, new Rect(0, 0, 1, 1.0f));
                     EditorGUI.LabelField(new Rect(4, 4, decisionNode.NodeWidth, decisionNode.NodeHeight), "Decision", labelStyleHead_Node);

                     EditorGUILayout.LabelField(decisionNode.NodeConvodata.Actor == null
                        ? BranchConstants.UnAssignedActor
                        : decisionNode.NodeConvodata.Actor.ActorName,
                        labelStyleHead_Node);

                     decisionCommand.TextAreaRect.Clear();
                     for (int decisionIndex = 0; decisionIndex < decisionNode.DecisionOptions.Count; decisionIndex++)
                     {
                         GUILayout.BeginHorizontal();
                            GUILayout.Label("" + (decisionIndex + 1), labelStyleHead_Node, GUILayout.Width(10));
                         decisionNode.DecisionOptions[decisionIndex] = EditorGUILayoutExtensions.SetTextAreaExpandable(Command.WindowRect, decisionCommand.TextAreaRect, decisionIndex, ref buffer, decisionNode.DecisionOptions[decisionIndex], textAreaStyleNode, areaHeight: 50, textWidth: decisionNode.NodeWidth - 25);
                         GUILayout.EndHorizontal();
                         GUILayout.Space(5);

                     }

                     Node.NodeHeight = CalculateNodeHeightFromText(decisionNode.DecisionOptions, decisionNode.NodeWidth - 25);

                     Rect deleteButtonRect = new Rect(decisionNode.NodeWidth - 20, 0, 20, 20);
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
            EditorGUILayout.LabelField("Decision Info", labelStyleHead_Panel);
            EditorGUILayout.Space();
            GUILayout.Label("Actor (Camera Focus Target)", inspectorText, GUILayout.Width(150));

            int indexx = EditorGUILayout.Popup(ActorEditorDropdownIndex,  NodeManager.Instance.StartNode.ActorsInScene.Select(x => x.ActorName).ToArray(), GUILayout.Width(200));

            if(indexx != ActorEditorDropdownIndex)
            {
                decisionCommand.AssignNewActor(indexx);
                preview.UpdateShotRender();
                ActorEditorDropdownIndex = indexx;
            }

            using (var horizontalScopeShowPreviewOption = new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Show Previous Dialog", inspectorText, GUILayout.Width(150));
                decisionNode.ShowPreviousDialog = EditorGUILayout.Toggle(decisionNode.ShowPreviousDialog);
            }

            EditorGUILayout.Space();


            nodeCameraOptionsDrawer.DrawUICamCompOptions();        
        }

        //Draws and recalculates the spacing of the decision out points based on
        //the adding or removing of decision options
        protected override void DrawOutPoint()
        {
            int dotCount = decisionNode.PointOut.Count;
            float lineLength = decisionNode.NodeWidth - 35;
            float spacing = dotCount > 1 ? lineLength / (dotCount + 1) : 0;
            float startPos = (lineLength - (dotCount - 1) * spacing) / 2;
            float yPos = decisionNode.NodeHeight - 20;

            for (int i = 0; i < dotCount; i++)
            {
                float xPos = startPos + i * spacing + widthConnectionPoint / 2;
                Rect bounds = new Rect(xPos, yPos, widthConnectionPoint, heightConnectionPoint);
                
                //RS TODO Move bounds assignment to command eventually?
                Node.PointOut[i].LocalBounds = bounds;

                DrawPoint(bounds, Node.PointOut[i].ConnectedTo != null);

                Rect labelRect = bounds;
                labelRect.x += 6; // Adjust it to be at the center of the point
                GUI.Label(labelRect, (i + 1).ToString(), decisionOptionNumber);
            }
        }
        public void Clear()
        {
            decisionCommand.CustomCameraCommand.ClearCameraSceneObject();
        }
    }
}
