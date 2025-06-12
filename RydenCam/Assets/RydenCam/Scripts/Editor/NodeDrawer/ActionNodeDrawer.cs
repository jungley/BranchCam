using Assets.RydenCam.Scripts.BranchCamCC;
using System;
using UnityEditor;
using UnityEngine;
using Assets.RydenCam.Scripts.NodeCommands;
using RydenCam.Common;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
using System.Collections.Generic;

namespace Assets.RydenCam.Scripts.Editor.NodeDrawer
{
    internal class ActionNodeDrawer : NodeDrawerBase
    {
        private ActionNode node { get; set; }
        private ActionNodeCommand command { get; set; }

        private Vector2 scrollPosInspector { get; set; }

        private List<GameActionData> selectedGameActionDatas { get; set; }

        public ActionNodeDrawer(Node _node) : base(_node)
        {
            node = _node as ActionNode;
            command = new ActionNodeCommand(_node);

            selectedGameActionDatas = node.GameActionDatas ?? new List<GameActionData>();

            WindowRect = new Rect(node.EditorPosition.x, node.EditorPosition.y, node.NodeWidth, node.NodeHeight);

            ColorUtility.TryParseHtmlString("#FE1010", out Color colorref);
            NodeColor = colorref;
        }

        public override void DrawNode(int index)
        {
            GUI.backgroundColor = Color.gray;

            WindowRect = GUI.Window(index, new Rect(node.EditorPosition.x, node.EditorPosition.y, node.NodeWidth, node.NodeHeight),
                (windowId) =>
                {
                    GUI.DrawTextureWithTexCoords(new Rect(0, 0, 280.0f, 25f), HeaderTexture, new Rect(0, 0, 1, 1));
                    EditorGUI.LabelField(new Rect(4, 4, node.NodeWidth, node.NodeHeight), "Action", labelStyleHead_Node);


                    //Display Selected Methods
                    foreach (GameActionData data in node.GameActionDatas)
                    {
                        EditorGUILayout.LabelField(data.SelectedMethodName, labelStyleHead_Node);
                    }

                    Rect deleteButtonRect = new Rect(node.NodeWidth - 20, 0, 20, 20);
                    if (GUI.Button(deleteButtonRect, "X"))
                    {
                        command.RemoveNode();
                    }

                    DrawConnectionPoints();

                    GUI.DragWindow();

                }, "");

            if(IsActive) HighlightNode();

            Node.EditorPosition = new Vector2(WindowRect.x, WindowRect.y);

        }

        public override void DrawNodeInspector()
        {
            EditorGUILayout.LabelField("Action Info", labelStyleHead_Panel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Invoke Command(s)", GUILayout.Width(135), GUILayout.Height(30)))
            {
                command.InvokeCommands();
            }
            if (GUILayout.Button("Add A Command"))
            {
                command.AddCommand();
            }

            scrollPosInspector = EditorGUILayout.BeginScrollView(scrollPosInspector, GUILayout.Width(250), GUILayout.Height(680));

            for(int index = 0; index < selectedGameActionDatas.Count; index++ )
            {

                if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    node.GameActionDatas.RemoveAt(node.GameActionDatas.IndexOf(selectedGameActionDatas[index]));
                    break;
                }

                GameObject checkNewObject = (GameObject) EditorGUILayout.ObjectField("Select GameObject", selectedGameActionDatas[index].GameObj, typeof(GameObject), true);
                if(checkNewObject!= null && checkNewObject?.name != selectedGameActionDatas[index]?.GameObjectName)
                {
                    command.AssignActionObject(checkNewObject, index);
                }

                //Displaying Parameters / Method info
                if (selectedGameActionDatas[index].GameObj != null)
                {
                    int checkSelectedIndex = EditorGUILayout.Popup("Select an method:", selectedGameActionDatas[index].SelectedMethodIndex, selectedGameActionDatas[index].MethodNames.ToArray());
                    if (checkSelectedIndex != selectedGameActionDatas[index].SelectedMethodIndex)
                    {
                        command.AssignMethod(index, checkSelectedIndex);
                    }

                    //A method is selected
                    if (selectedGameActionDatas[index].SelectedMethodIndex != -1 && selectedGameActionDatas[index].ParameterInfo != null)
                    {
                        //Move this into command / node later?
                        for (int i = 0; i < selectedGameActionDatas[index].ParameterInfo.Length; i++)
                        {
                            Type type = selectedGameActionDatas[index].ParameterInfo[i].ParameterType;

                            EditorGUILayout.LabelField(selectedGameActionDatas[index].ParameterInfo[i].Name, inspectorTextBold);

                            if (type == typeof(string))
                            {
                                selectedGameActionDatas[index].SelectedMethodArgValues[i] = EditorGUILayout.TextField("Set Value", selectedGameActionDatas[index].SelectedMethodArgValues[i]);
                            }
                            else if (type == typeof(bool))
                            {
                                if (string.IsNullOrEmpty(selectedGameActionDatas[index].SelectedMethodArgValues[i]))
                                    selectedGameActionDatas[index].SelectedMethodArgValues[i] = false.ToString();

                                selectedGameActionDatas[index].SelectedMethodArgValues[i] = EditorGUILayout.Toggle("Set Boolean Value", selectedGameActionDatas[index].SelectedMethodArgValues[i].ConvertToBool()).ToString();
                            }
                            else if (type == typeof(int) || type == typeof(double) || type == typeof(float))
                            {
                                selectedGameActionDatas[index].SelectedMethodArgValues[i] = EditorGUILayout.TextField("Set Numerical Value", selectedGameActionDatas[index].SelectedMethodArgValues[i]);
                            }
                            else
                            {
                                BranchLog.Log("Method assigned has an incompatible parameter");
                            }
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }


    }
}