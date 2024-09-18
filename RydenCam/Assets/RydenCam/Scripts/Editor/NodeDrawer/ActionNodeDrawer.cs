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



        public ActionNodeDrawer(NodeCC _node) : base(_node)
        {
            node = _node as ActionNode;
            command = new ActionNodeCommand(_node);

            selectedGameActionDatas = node.GameActionDatas;

            nodeWidth = 200;
            nodeHeight = 70; // Needs to dynamically change

            windowRect = new Rect(node.EditorPosition.x, node.EditorPosition.y, nodeWidth, nodeHeight);

            ColorUtility.TryParseHtmlString("#FE1010", out Color colorref);
            nodeColor = colorref;
        }

        public override void DrawNode(int index)
        {
            GUI.backgroundColor = Color.gray;

            windowRect = GUI.Window(index, new Rect(node.EditorPosition.x, node.EditorPosition.y, nodeWidth, nodeHeight),
                (windowId) =>
                {
                    GUI.DrawTextureWithTexCoords(new Rect(0, 0, 280.0f, 25f), HeaderTexture, new Rect(0, 0, 1, 1));
                    EditorGUI.LabelField(new Rect(4, 4, nodeWidth, nodeHeight), "Action", labelStyleHead_Node);


                    Rect deleteButtonRect = new Rect(nodeWidth - 20, 0, 20, 20);
                    if (GUI.Button(deleteButtonRect, "X"))
                    {
                        command.RemoveNode(node);
                    }

                    DrawConnectionPoints();

                    GUI.DragWindow();

                }, "");

            Node.EditorPosition = new Vector2(windowRect.x, windowRect.y);

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