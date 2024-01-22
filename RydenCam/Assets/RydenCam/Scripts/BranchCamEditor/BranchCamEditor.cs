using UnityEngine;
using UnityEditor;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes.Connections;
using System.Linq;
using RydenCam.Common;
using RydenCam.BranchCamEditor.Serialization;
using RydenCam.BranchCamEditor.Controllers;

namespace RydenCam.BranchCamEditor
{

#if UNITY_EDITOR
    /* The Window that holds all the branching dialogue nodes */
    [ExecuteAlways]
    public class BranchCamEditor : EditorWindow
    {
        private static EditorBaseNode activeNode;
        public static EditorBaseNode ActiveNode
        {
            get { return activeNode; }
            set
            {
                activeNode = value;
                if (activeNode != null)
                {
                    SetHighlightTexture(activeNode.windowRect);
                }
            }
        }
        private Vector2 mousePos;
        private static Rect InspectorPanelArea;
        private static Rect ButtonPanelArea;
        public static bool startNodeAdded = false;
        static float panX = 0;
        static float panY = 0;
        private Rect lastEditorWindowPos;
        bool IsDrawingHandle = false;
        ConnectionPoint handlePoint;


        static Texture2D _targetTextureInspector { get; set; }
        static Texture2D TargetTextureInspector
        {
            get
            {
                if (_targetTextureInspector == null)
                {
                    _targetTextureInspector = new Texture2D(1, 1);
                    _targetTextureInspector.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 0.8f));
                    _targetTextureInspector.Apply();
                }
                return _targetTextureInspector;
            }
        }

        static Texture2D _tt { get; set; }
        static Texture2D tex
        {
            get
            {
                if(_tt == null)
                {
                    _tt = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    _tt.SetPixel(0, 0, new Color(0f, 0f, 0f));
                    _tt.Apply();
                }
                return _tt;
            }
        }
        static Texture2D highlightTex;
        static BranchCamEditor editor;

        static Texture2D arrowImage;

        //Panels
        private static GUIStyle panelstyle_inspector;
        private static GUIStyle panelstyle_button;

        //Text Style
        private static GUIStyle inspectorText;
        private static bool initHasBeenCalled;

        [MenuItem(BranchConstants.MainWindowName)]
        public static void OpenWindow()
        {
            Initialize();
        }


        //BranchCamEditor Functions
        public static void Initialize(bool doOverride = false)
        {
            if (doOverride || !initHasBeenCalled)
            {
                EditorController.Instance.ResetEverything();
                initHasBeenCalled = true;
            }

            //Setup UI Editor
            editor = (BranchCamEditor)EditorWindow.GetWindow(typeof(BranchCamEditor), false, BranchConstants.WindowTitle);
            SetPanelBackgrounds();
            editor.minSize = new Vector2(400f, 400f);
            editor.autoRepaintOnSceneChange = true;
            editor.Show();
            arrowImage = Resources.Load("arrowImage2") as Texture2D;

            string lastFilePath = BranchCamEditorPreferences.GetLastFilePath();
            if (lastFilePath != string.Empty)
            {
                EditorController.Instance.ResetEverything();

                LoadFile.LoadSaveables();
            }

        }


        static void SetHighlightTexture(Rect bounds)
        {
            //Create Highlight Texture2D
            highlightTex = new Texture2D((int)bounds.width, (int)bounds.height);
            int borderwidth = 5;
            for (int y = 0; y < highlightTex.height; y++)
            {
                for (int x = 0; x < highlightTex.width; x++)
                {
                    Color colResult = (x >= (highlightTex.width - borderwidth) || x <= borderwidth || y <= borderwidth || y >= (highlightTex.height - borderwidth)) ? Color.blue : Color.black;
                    highlightTex.SetPixel(x, y, colResult);
                }
            }
            highlightTex.Apply();
        }

        public static void SetPanelBackgrounds()
        {
            /*------------------------------------*/
            //Background for Inspector
            panelstyle_inspector = new GUIStyle();
            panelstyle_inspector.normal.background = TargetTextureInspector;


            /*------------------------------------*/
            //Button Header Texture
            Texture2D targetTextureButtonHeader = new Texture2D(1, 1);
            targetTextureButtonHeader.SetPixel(0, 0, Color.gray);
            targetTextureButtonHeader.Apply(); 

            panelstyle_button = new GUIStyle();
            panelstyle_button.normal.background = targetTextureButtonHeader;

            /*------------------------------------*/

            //Text
            inspectorText = new GUIStyle();
            inspectorText.normal.textColor = Color.white;
            inspectorText.fontSize = 15;

            //Define areas for clicking
            ButtonPanelArea = new Rect(0, 0, 1000, 30);
            InspectorPanelArea = new Rect(0, 0, 250, 1000);
        }

        void OnInspectorUpdate()
        {
            if (editor == null)
            {
                //Editor is not defined yet
                return;
            }

            //If window was resized or moved
            lastEditorWindowPos = editor.position;

            if (IsDrawingHandle)
            {
                Repaint();
            }
        }

        public bool isOverPanels(Vector2 mousepos)
        {
            return (InspectorPanelArea.Contains(mousepos) || ButtonPanelArea.Contains(mousepos));
        }

        public void handleMouseClick(Vector2 mousePos)
        {
            for (int i = 0; i < NodeManager.Instance.Length; i++)
            {
                //Get the Selected Node
                if (NodeManager.Instance.GetNode(i).windowRect.Contains(mousePos))
                {
                    //Fill column with node data
                    ActiveNode = NodeManager.Instance.GetNode(i);


                    //If mouse is over a point
                    if (ActiveNode.isOverPoint(mousePos))
                    {
                        //Clicked on the connection point start to draw Handle
                        if (!IsDrawingHandle)
                        {
                            handlePoint = ActiveNode.getConPoint(mousePos);
                            IsDrawingHandle = true;
                            return;
                        }
                        //Already Drawing a curve, point been selected now a second one is
                        else
                        {
                            ConnectionPoint fromPoint = ActiveNode.getConPoint(mousePos);
                            //Opposite type and not of of the current node
                            if ((fromPoint.type != handlePoint.type) && !ActiveNode.containsPoint(handlePoint))
                            {
                                //Remove Connections
                                if (ConnectionManager.Instance.IsOutConnected(fromPoint, handlePoint))
                                {
                                    ConnectionManager.Instance.Remove(fromPoint, handlePoint);
                                }
                                fromPoint.connectedTo = handlePoint;
                                handlePoint.connectedTo = fromPoint;
                                ConnectionManager.Instance.AddConnection(fromPoint, handlePoint, OnClickRemoveConnection);
                            }
                        }
                    }
                }
            }
            //Clicked but not over a window
            handlePoint = null;
            IsDrawingHandle = false;
            return;
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingPlayMode)
            {
                Repaint(); // Force a repaint when entering or exiting play mode
            }
        }


        void OnGUI()
        {
            
            //Wrap Everything In Flag
            if(!initHasBeenCalled)
            {
                Initialize();
                return;
            }

            DrawGrid(20f, 0.5f, Color.white);
            GUI.BeginGroup(new Rect(panX, panY, 100000, 100000));

            Event e = Event.current;
            mousePos = e.mousePosition;

            if (IsDrawingHandle)
            {
                Vector2 hpoint = handlePoint.getGlobalPoint();
                Vector3 startPos = new Vector3(hpoint.x, hpoint.y, 0);
                Vector3 endPos = new Vector3(mousePos.x, mousePos.y, 0);

                //Goto Curve
                if (ActiveNode.TypeOfNode == NodeType.GoToNode && handlePoint.type == ConnectionPointType.Out)
                {
                    Vector3 center = new Vector3((startPos.x + endPos.x) / 2, (endPos.y + startPos.y) / 2);
                    float arc;
                    float dist = Vector3.Distance(endPos, startPos);
                    if (startPos.x <= endPos.x)
                    {
                        arc = -600.0f * Mathf.Clamp01(dist / 250.0f);
                    }
                    else
                    {
                        arc = 600.0f * Mathf.Clamp01(dist / 250.0f);
                    }
                    center.x += arc;
                    Vector3[] vector3array = new Vector3[] { startPos, center, endPos };
                    vector3array = Curver.MakeSmoothCurve(vector3array, 90.0f);
                    Handles.color = Color.green;
                    Handles.DrawAAPolyLine(5.0f, vector3array);
                }
                //Everything else (Dialogue Decision Nodes)
                else
                {
                    Handles.DrawBezier(startPos, endPos, startPos, endPos, Color.green, null, 5);
                    Handles.color = Color.green;

                    //Calculate rotation from out point to in point 
                    float angle = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * 180 / Mathf.PI;
                    angle -= 90;
                    GUIUtility.RotateAroundPivot(angle, endPos);
                    GUI.DrawTexture(new Rect(endPos.x - 10, endPos.y, 20, 20), arrowImage, ScaleMode.StretchToFill, true, 20.0F);
                    GUIUtility.RotateAroundPivot(-angle, endPos);
                }
            }

            //Left Click Select
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                handleMouseClick(mousePos);
            }

            //Right click
            if (e.button == 1)
            {
                if (e.type == EventType.MouseDown)
                {
                    bool clickedOnWindow = false;
                    int selectindex = -1;

                    for (int i = 0; i < NodeManager.Instance.Length; i++)
                    {
                        //Get the Selected Node
                        if (NodeManager.Instance.GetNode(i).windowRect.Contains(mousePos))
                        {
                            selectindex = i;
                            clickedOnWindow = true;
                            break;
                        }
                    }
                    //Open new node menu
                    if (!clickedOnWindow)
                    {
                        GenericMenu menu = new GenericMenu();
                        if (!startNodeAdded)
                        {
                            menu.AddItem(new GUIContent("Add Start Node"), false, ContextCallback, "startNode");
                        }
                        //Needs to Add an Actor
                        else if (EditorController.Instance.ActorsInScene.Count == 0)
                        {
                            menu.AddItem(new GUIContent("Must add an actor in the Start Node"), false, ContextCallback, "blank");
                        }
                        else if (EditorController.Instance.ActorsInScene.Any(t => t.ActorGO == null))
                        {
                            menu.AddItem(new GUIContent("One of the actors have not been assigned in the Start Node."), false, ContextCallback, "blank");
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Add Dialogue Node"), false, ContextCallback, "dialogueNode");
                            menu.AddItem(new GUIContent("Add Decision Node"), false, ContextCallback, "decisionNode");
                            menu.AddItem(new GUIContent("Add Action Node"), false, ContextCallback, "actionNode");
                            menu.AddItem(new GUIContent("Add GoTo Node"), false, ContextCallback, "gotoNode");
                        }
                        menu.ShowAsContext();
                        e.Use();
                    }
                }
            }

            //Draw Each Node
            Color saved = GUI.backgroundColor;
            BeginWindows();

            for (int i = 0; i < NodeManager.Instance.Length; i++)
            {
                EditorBaseNode nodeCur = NodeManager.Instance.GetNode(i);
                //Set Background Colors
                if (nodeCur.TypeOfNode == NodeType.GoToNode)
                {
                    GUI.backgroundColor = nodeCur.nodeColor;
                }
                else
                {
                    GUI.backgroundColor = Color.gray;
                }

                if (nodeCur == ActiveNode && ActiveNode != null)
                {
                    Color tempColor = GUI.backgroundColor;
                    tempColor.a = 0.75f;
                    GUI.backgroundColor = tempColor;
                    GUI.DrawTextureWithTexCoords(ActiveNode.windowRect, highlightTex, new Rect(0, 0, 1, 1.0f));
                }

                //Drawing each node
                NodeManager.Instance.GetNode(i).windowRect =
                    GUI.Window(i, NodeManager.Instance.GetNode(i).windowRect,
                    DrawNodeWindow, NodeManager.Instance.GetNode(i).windowTitle);
            }

            EndWindows();
            GUI.backgroundColor = saved;

            //Draw Connections
            ConnectionManager.Instance.DrawConnections();

            GUI.EndGroup();

            //A mousedrag is happening & not over panel
            if (Event.current.type == EventType.MouseDrag && !isOverPanels(Event.current.mousePosition))
            {
                //The EditorWindow is not being dragged
                if (lastEditorWindowPos == editor.position)
                {
                    //Weird Jumping Check
                    int scrollval = 70;
                    if ((Event.current.delta.x > -scrollval && Event.current.delta.x < scrollval)
                        && (Event.current.delta.y > -scrollval && Event.current.delta.y < scrollval))
                    {
                        panX += Event.current.delta.x;
                        panY += Event.current.delta.y;
                        Repaint();
                    }
                }
            }


            //BUTTON HEADER
            // Define common GUILayoutOptions
            GUILayoutOption[] horizontalLayoutOptions = new GUILayoutOption[]
            {
                 GUILayout.Width(EditorGUIUtility.currentViewWidth),
                 GUILayout.Height(30)
            };


            using (var horizontalScope = new GUILayout.HorizontalScope(panelstyle_button, horizontalLayoutOptions))
            {
                if (GUILayout.Button("NEW", GUILayout.Width(65), GUILayout.Height(30)))
                {
                    EditorController.Instance.ResetEverything();
                    EditorController.Instance.RedrawAll();
                    BranchCamEditorPreferences.SetLastFilePath(string.Empty);
                }

                if (GUILayout.Button("SAVE", GUILayout.Width(65), GUILayout.Height(30)))
                {
                    SaveFile.SaveConversation();
                }
                if (GUILayout.Button("LOAD", GUILayout.Width(65), GUILayout.Height(30)))
                {
                    LoadFile.SelectDialogueWindow();
                    if (LoadFile.IsValidEditorPath())
                    {
                        EditorController.Instance.ResetEverything();
                        LoadFile.LoadSaveables();
                    }
                }

                if (GUILayout.Button("Graph View", GUILayout.Width(85), GUILayout.Height(30)))
                {
    
                }

                if (GUILayout.Button("Inkle Script View", GUILayout.Width(120), GUILayout.Height(30)))
                {

                }

            }


            //INSPECTOR PANEL
            using (var verticalScope = new GUILayout.VerticalScope(panelstyle_inspector, GUILayout.Width(250), GUILayout.Height(editor.position.height)))
            {
                if (ActiveNode == null)
                {
                    GUILayout.Label("Right click to add a node", inspectorText, GUILayout.Width(90));
                }
                else
                {
                    ActiveNode.DrawForInspector();
                }
            }

        }

        public void ShowWindow(int id)
        {
            // Get existing open window or if none, make a new one:
            BeginWindows();
            //var window = GetWindow(typeof());
           // window.Show();
            EndWindows();
        }


        void DrawNodeWindow(int index)
        {
            EditorBaseNode Node = NodeManager.Instance.GetNode(index);

            // Button to delete
            Rect deleteButtonRect = new Rect(Node.nodeWidth - 20, 0, 20, 20);
            //Draw Content inside node
            Node.DrawContent();
            if (GUI.Button(deleteButtonRect, "X"))
            {
                ConnectionManager.Instance.RemoveAssocConnec(Node);
                NodeManager.Instance.RemoveNode(Node);
                ActiveNode = null;
            }
            GUI.DragWindow();
        }

        //FOR ADDING NEW node
        void ContextCallback(object obj)
        {
            string clb = obj.ToString();

            switch (clb)
            {
                case ("startNode"):
                    EditorBaseNode startNode = new EditorStartNode(mousePos);
                    NodeManager.Instance.AddNode(startNode);
                    ActiveNode = startNode;
                    startNodeAdded = true;
                    break;
                case ("dialogueNode"):
                    EditorBaseNode dialogueNode = new EditorDialogueNode(mousePos);
                    NodeManager.Instance.AddNode(dialogueNode);
                    ActiveNode = dialogueNode;
                    break;
                case ("decisionNode"):
                    EditorBaseNode decisionNode = new EditorDecisionNode(mousePos);
                    NodeManager.Instance.AddNode(decisionNode);
                    ActiveNode = decisionNode;
                    break;
                case ("actionNode"):
                    EditorBaseNode actionNode = new EditorActionNode(mousePos);
                    NodeManager.Instance.AddNode(actionNode);
                    ActiveNode = actionNode;
                    break;
                case ("gotoNode"):
                    EditorBaseNode gotoNode = new EditorGotoNode(mousePos);
                    NodeManager.Instance.AddNode(gotoNode);
                    ActiveNode = gotoNode;
                    break;
            }
        }

        public static void OnClickRemoveConnection(Connection connection)
        {
            ConnectionManager.Instance.Remove(connection);
        }

        //Make black background more efficiently
        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), tex, ScaleMode.StretchToFill);

            Vector2 offset = new Vector2(panX, panY);
            Vector2 drag = new Vector2(0, 0);

            int widthDivs = Mathf.CeilToInt((position.width + 1000) / gridSpacing);
            int heightDivs = Mathf.CeilToInt((position.height + 1000) / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            offset += drag * 0.5f;
            Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();

        }
    }
#endif
}