using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using RydenCam.BranchCamEditor.Controllers;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Serialization;
using RydenCam.BranchCamEditor.Serialization.Saveables;
using Assets.RydenCam.Scripts.BranchCamEditor.Serialization.Saveables;

namespace RydenCam.BranchCamEditor.Nodes
{

    [Serializable]
    public class EditorActionNode : EditorBaseNode
    {
        public List<MethodInfoContainer> methodContainers = new List<MethodInfoContainer>();

        private GUIStyle labelStyle;

        private Vector2 scrollPos;
        private Vector2 scrollPosInspector;

        public override NodeType TypeOfNode => NodeType.ActionNode;

        public EditorActionNode(Vector2 mousePos) : base()
        {
            nodeWidth = 150;
            nodeHeight = 100;

            windowRect = new Rect(mousePos.x, mousePos.y, nodeWidth, nodeHeight);
            ColorUtility.TryParseHtmlString("#FE1010", out nodeColor);

            //Instantiate ConnectionPoints
            //PointIn = new ConnectionPoint(this, ConnectionPointType.In);
            PointOut = new List<ConnectionPoint>();
            //PointOut.Add(new ConnectionPoint(this, ConnectionPointType.Out));
        }

        public override void DrawContent()
        {
#if UNITY_EDITOR
            GUI.DrawTextureWithTexCoords(new Rect(0, 0, 200.0f, 25.0f), HeaderTexture, new Rect(0, 0, 1, 1.0f));
            EditorGUI.LabelField(new Rect(4, 4, nodeWidth, nodeHeight), "Action", labelStyleHead_Node);

            List<string> displayedMethodNames = new List<string>();
            foreach (MethodInfoContainer result in methodContainers)
            {
                if (!string.IsNullOrEmpty(result.methodName))
                {
                    displayedMethodNames.Add(result.methodName);
                    nodeHeight += 18;
                }
            }

            //Formats/Displays each decision option and text in node
            foreach (string name in displayedMethodNames)
            {
                //Repaint is ocurring
                try { EditorGUILayout.LabelField(name, GUILayout.Width(100), GUILayout.Height(18)); }
                catch (Exception) { }
            }


            //PointIn.Draw();
            //PointOut[0].Draw();
#endif
        }
        public override bool isOverPoint(Vector2 mousePos)
        {
            //Convert mousepos to local over the window rect
            //Detect Out ConnectionPoint
            float xPoint = mousePos.x - windowRect.x;
            float yPoint = mousePos.y - windowRect.y;

            Vector2 localPoint = new Vector2(xPoint, yPoint);
            return false;

            //If mouseposition is over point
            //return (PointIn.Bounds.Contains(localPoint) || PointOut[0].Bounds.Contains(localPoint));
        }


        public override void DrawForInspector()
        {
#if UNITY_EDITOR
            base.DrawForInspector();
            EditorGUILayout.LabelField("Action Info", labelStyleHead_Panel);
            EditorGUILayout.Space();

            //Change this to an array
            if(!methodContainers.Any())
            {
                methodContainers.Add(new MethodInfoContainer());
            }


            if (GUILayout.Button("Invoke Method(s)", GUILayout.Width(135), GUILayout.Height(30)))
            {
                InvokeAction();
            }
            if (GUILayout.Button("Add A Method"))
            {
                methodContainers.Add(new MethodInfoContainer());
            }

            scrollPosInspector = EditorGUILayout.BeginScrollView(scrollPosInspector, GUILayout.Width(250), GUILayout.Height(680));

            foreach (MethodInfoContainer methodInformation in methodContainers)
            {

                if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    methodContainers.RemoveAt(methodContainers.IndexOf(methodInformation));
                    EditorController.Instance.RedrawAll();
                    break;
                }

                // Use the ObjectField() method to allow the user to select a game object
                GameObject gameObject = GameObject.Find(methodInformation.gameObjectName);
                gameObject = (GameObject)EditorGUILayout.ObjectField("Select GameObject", gameObject, typeof(GameObject), true);

                if (gameObject != null)
                {
                    methodInformation.gameObjectName = gameObject.name;

                    MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
                    foreach (MonoBehaviour component in components)
                    {
                        MethodInfo[] methods = component.GetType().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                        string[] methodNames = methods.Select(x => x.Name).ToArray();

                        methodInformation.selectedMethodIndex = EditorGUILayout.Popup("Select an method:", methodInformation.selectedMethodIndex, methodNames);
                        methodInformation.methodName = methodNames[methodInformation.selectedMethodIndex];

                        ParameterInfo[] parameters = methods[methodInformation.selectedMethodIndex].GetParameters();
                        methodInformation.paramInfo = parameters;

                        if (methodInformation.argValues == null || methodInformation.argValues.Count() != parameters.Count())
                        {
                            methodInformation.argValues = new string[parameters.Length];
                        }

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            Type type = parameters[i].ParameterType;
                            GUIStyle boldStyle = new GUIStyle(EditorStyles.label);
                            boldStyle.fontStyle = FontStyle.Bold;
                            boldStyle.fontSize = 14;

                            EditorGUILayout.LabelField(parameters[i].Name, boldStyle);

                            if (type == typeof(string))
                            {
                                methodInformation.argValues[i] = EditorGUILayout.TextField("Set Value", methodInformation.argValues[i]);
                            }
                            else if (type == typeof(bool))
                            {
                                if (string.IsNullOrEmpty(methodInformation.argValues[i]))
                                    methodInformation.argValues[i] = false.ToString();

                                methodInformation.argValues[i] = EditorGUILayout.Toggle("Set Boolean Value", methodInformation.argValues[i].ConvertToBool()).ToString();
                            }
                            else if (type == typeof(int) || type == typeof(double) || type == typeof(float))
                            {
                                methodInformation.argValues[i] = EditorGUILayout.TextField("Set Numerical Value", methodInformation.argValues[i]);
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

#endif
        }

        public void InvokeAction()
        {
            foreach (MethodInfoContainer methodInformation in methodContainers)
            {
                try
                {
                    // Convert the arguments from strings to the appropriate types
                    object[] methodArguments = new object[methodInformation.argValues.Count()];
                    for (int i = 0; i < methodInformation.argValues.Length; i++)
                    {
                        methodArguments[i] = Convert.ChangeType(methodInformation.argValues[i], methodInformation.paramInfo[i].ParameterType);
                    }

                    GameObject gameObject = GameObject.Find(methodInformation.gameObjectName);
                    MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
                    MethodInfo[] methodInfos = components.SelectMany(x => x.GetType().GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)).ToArray();
                    MethodInfo method = methodInfos.Where(x => x.Name == methodInformation.methodName).FirstOrDefault();


                    method.Invoke(null, methodArguments);

                }
                catch (Exception e)
                {
                    BranchLog.Error("Error with calling method", e);
                }
            }
        }


        public void defineStyles()
        {
            //Style HEADERS for shots
            labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.black;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 10;
        }

        public override ConnectionPoint getConPoint(Vector2 mousePos)
        {    
            //Convert mousepos to local over the window rect
            //Detect Out ConnectionPoint
            float xPoint = windowRect.x - mousePos.x;
            float yPoint = windowRect.y - mousePos.y;
            Vector2 localPoint = new Vector2(mousePos.x - windowRect.x, mousePos.y - windowRect.y);

            return null;
            /*
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
                BranchLog.Log("This shouldnt have happened");
                return null;
            }
            */
        }

        [Serializable]
        public class MethodInfoContainer
        {
            [SerializeField]
            public string gameObjectName;
            [SerializeField]
            public string methodName;
            [SerializeField]
            public int selectedMethodIndex;
            [SerializeField]
            public string[] argValues;

            [JsonIgnore]
            public ParameterInfo[] paramInfo;

            public MethodInfoContainer()
            {

            }
        }

        public EditorActionNode(Saveable savenode ) : base()
        {
            //Cast it down
            SaveableActionNode actnode = (SaveableActionNode)savenode;
            defineStyles();
            nodeWidth = 150;
            nodeHeight = 100;
            //Saveable info
            windowRect = actnode.windowRect;
            node_id = actnode.node_id;

            methodContainers = actnode.methodInfoConatiners;

            ColorUtility.TryParseHtmlString("#FE1010", out nodeColor);

            //Instantiate ConnectionPoints
            //PointIn = new ConnectionPoint(this, ConnectionPointType.In);
            PointOut = new List<ConnectionPoint>();
            //PointOut.Add(new ConnectionPoint(this, ConnectionPointType.Out));
        }
    }
}
