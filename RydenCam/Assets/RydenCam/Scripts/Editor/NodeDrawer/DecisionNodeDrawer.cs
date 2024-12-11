using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.NodeCommands;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.RydenCam.Scripts.Editor.NodeDrawer
{
    internal class DecisionNodeDrawer : NodeDrawerBase
    {
        private DecisionNode node  { get; set; }
        private DecisionNodeCommand command { get; set; }

        private NodeCameraOptionsDrawer nodeCameraOptionsDrawer { get; set; }


        private int ActorEditorDropdownIndex { get; set; }

        private Vector2 scrollPosInspector { get; set; }

        private GUIStyle decisionOptionNumber { get; set; }


        public DecisionNodeDrawer(Node _node) : base(_node)
        {
            node = _node as DecisionNode;

            command = new DecisionNodeCommand(node);

            nodeCameraOptionsDrawer = new NodeCameraOptionsDrawer(node, inspectorText, labelStyleHead_Panel);

            WindowRect = new Rect(node.EditorPosition.x, node.EditorPosition.y, node.NodeWidth, node.NodeHeight);

            ColorUtility.TryParseHtmlString("#990099", out Color colorref);
            NodeColor = colorref;

            decisionOptionNumber = new GUIStyle(labelStyleHead_Node);
            decisionOptionNumber.fontSize = 13;
            decisionOptionNumber.normal.textColor = Color.black;

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
                     GUI.DrawTextureWithTexCoords(new Rect(0, 0, 200.0f, 25.0f), HeaderTexture, new Rect(0, 0, 1, 1.0f));
                     EditorGUI.LabelField(new Rect(4, 4, node.NodeWidth, node.NodeHeight), "Decision", labelStyleHead_Node);

                     EditorGUILayout.LabelField(node.NodeConvodata.Actor == null
                        ? BranchConstants.UnAssignedActor
                        : node.NodeConvodata.Actor.ActorName,
                        labelStyleHead_Node);

                     for (int decisionIndex = 0; decisionIndex < node.DecisionOptions.Count; decisionIndex++)
                     {
                         EditorGUILayout.BeginHorizontal();

                         GUILayout.Label("" + (decisionIndex + 1), labelStyleHead_Node, GUILayout.Width(30));

                         node.DecisionOptions[decisionIndex] = EditorGUILayout.TextField(node.DecisionOptions[decisionIndex], GUILayout.Width(150));

                         EditorGUILayout.EndHorizontal();

                         GUILayout.Space(5); 
                     }


                     Rect deleteButtonRect = new Rect(node.NodeWidth - 20, 0, 20, 20);
                     if (GUI.Button(deleteButtonRect, "X"))
                     {
                         command.RemoveNode(node);
                     }

                     DrawConnectionPoints();

                     GUI.DragWindow();

                 }, "");

            Node.EditorPosition = new Vector2(WindowRect.x, WindowRect.y);
        }

        public override void DrawNodeInspector()
        {
            EditorGUILayout.LabelField("Decision Info", labelStyleHead_Panel);
            EditorGUILayout.Space();
            GUILayout.Label("Actor", inspectorText, GUILayout.Width(150));

            int indexx = EditorGUILayout.Popup(ActorEditorDropdownIndex,  NodeManager.Instance.ActorsInScene().Select(x => x.ActorName).ToArray(), GUILayout.Width(200));

            if(indexx != ActorEditorDropdownIndex)
            {
                command.AssignNewActor(indexx);
                ActorEditorDropdownIndex = indexx;
            }

            using (var horizontalScopeShowPreviewOption = new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Show Previous Dialog", inspectorText, GUILayout.Width(150));
                node.ShowPreviousDialog = EditorGUILayout.Toggle(node.ShowPreviousDialog);
            }

            EditorGUILayout.Space();


            //Add Choice button
            //Cap at 9 Choices
            if (GUILayout.Button("Add Choice", GUILayout.Width(80), GUILayout.Height(25)) && node.PointOut.Count < 9)
            {
                command.AddDecisionOption();
            }

            scrollPosInspector = EditorGUILayout.BeginScrollView(scrollPosInspector, GUILayout.Width(250), GUILayout.Height(280));

            //Loop through choices
            for (int i = 0; i < node.DecisionOptions.Count; i++)
            {
                using (var decisionChoiceListings = new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Choice " + (i + 1), inspectorText, GUILayout.Width(195));
                    if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        ConnectionManager.Instance.Remove(node.PointOut[i]);
                        command.RemoveDecisionOption(i);
                        break;
                    }
                }
                node.DecisionOptions[i] = EditorGUILayout.TextArea(node.DecisionOptions[i], textareaStyle, GUILayout.Width(200), GUILayout.Height(60));
            }
            EditorGUILayout.EndScrollView();

            GUI.DrawTextureWithTexCoords(new Rect(0, 443, 250.0f, 25.0f), HeaderTexture, new Rect(0, 0, 1, 1.0f));

            nodeCameraOptionsDrawer.DrawUICamCompOptions();

        }

        //Draws and recalculates the spacing of the decision out points based on
        //the adding or removing of decision options
        protected override void DrawOutPoint()
        {
            int dotCount = Node.PointOut.Count;
            float lineLength = node.NodeWidth - 35;
            float spacing = dotCount > 1 ? lineLength / (dotCount + 1) : 0;
            float startPos = (lineLength - (dotCount - 1) * spacing) / 2;
            float yPos = node.NodeHeight - 20;

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
    }
}
