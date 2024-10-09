using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Assets.RydenCam.Scripts.Editor;
using Assets.RydenCam.Scripts.BranchCamCC;
using UnityEngine.UIElements;
using Assets.RydenCam.Scripts.Editor.NodeDrawer;
using RydenCam.Common;
using RydenCam.BranchCamEditor;
using RydenCam.BranchCamEditor.Serialization;
using RydenCam.BranchCamEditor.Controllers;
using System.Linq;
using System.Threading;
using RydenCam.BranchCamEditor.Managers;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using RydenCam.BranchCamEditor.Nodes.Connections;

//NodeGraphEditorWindow is the View in MVVM
//NodeGraphViewModel is the View Model
//Nodes are the Model
public class NodeGraphEditorWindow : EditorWindow
{
    private NodeGraphViewModel viewModel;

    private NodeDrawerBase ActiveNodeDrawView { get; set; }

    //Window Properties
    static float panX = 0;
    static float panY = 0;
    static Texture2D _tt { get; set; }
    static Texture2D tex
    {
        get
        {
            if (_tt == null)
            {
                _tt = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                _tt.SetPixel(0, 0, new Color(0f, 0f, 0f));
                _tt.Apply();
            }
            return _tt;
        }
    }
    static Texture2D _arrowImage { get; set; }
    static Texture2D arrowImage
    {
        get
        {
            if(_arrowImage == null)
            {
                _arrowImage = Resources.Load("arrowImage2") as Texture2D;
            }
            return _arrowImage;
        }
    }
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

    List<NodeDrawerBase> NodeDrawers { get; set; }



    //Panels
    private static GUIStyle panelstyle_inspector;
    private static GUIStyle panelstyle_button;
    private static Rect ButtonPanelArea;
    private static Rect InspectorPanelArea;

    //Text Style
    private static GUIStyle inspectorText;


    //Ribbon Properties
    private bool showDropdown = false;

    void OnGUI()
    {
        GUI.BeginGroup(new Rect(panX, panY, 100000, 100000));

        DrawGrid(gridSpacing: 20f, gridOpacity: 0.5f, gridColor: Color.white);

        DrawRibbon();

        DrawConnectionCurve();

        HandleInputClicks();

        DrawNodes();

        ConnectionManager.Instance.DrawConnections();

        DrawInspector();

        GUI.EndGroup();

    }

    private void OnInspectorUpdate()
    {
        if(viewModel.IsDrawingHandle)
        {
            Repaint();
        }
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(viewModel.ActiveNode))
        {
            ActiveNodeDrawView = NodeDrawerFactory.CreateNodeDrawer(viewModel.ActiveNode);
        }
    }



    [MenuItem("Window/Node Graph Editor-(BranchCamCC)")]
    public static void OpenWindow()
    {
        //SetUp UI
        NodeGraphEditorWindow window = GetWindow<NodeGraphEditorWindow>();
        window.titleContent = new GUIContent("Window/Node Graph Editor-(BranchCamCC)");
        window.minSize = new Vector2(400f, 400f);
        window.autoRepaintOnSceneChange = true;


        if(!string.IsNullOrEmpty(BranchCamEditorPreferences.GetLastFilePath()))
        {
            LoadFile.LoadSaveables();
        }

        InitializeStaticResources();

        window.UpdateNodeDrawers();

        window.ShowUtility();
    }

    private static void InitializeStaticResources()
    {
        //Background for Inspector
        panelstyle_inspector = new GUIStyle();
        panelstyle_inspector.normal.background = TargetTextureInspector;

        //Button Header Texture
        Texture2D targetTextureButtonHeader = new Texture2D(1, 1);
        targetTextureButtonHeader.SetPixel(0, 0, Color.gray);
        targetTextureButtonHeader.Apply();

        //Text
        inspectorText = new GUIStyle();
        inspectorText.normal.textColor = Color.white;
        inspectorText.fontSize = 15;

        //Define areas for clicking
        ButtonPanelArea = new Rect(0, 0, 1000, 30);
        InspectorPanelArea = new Rect(0, 0, 250, 1000);

        panelstyle_button = new GUIStyle();
        panelstyle_button.normal.background = targetTextureButtonHeader;
    }


    // Called when the window is enabled or created
    private void OnEnable()
    {
        //Event Handlers
        viewModel = new NodeGraphViewModel();
        viewModel.Nodes.CollectionChanged += OnNodesChanged;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        
        //Draw Nodes
        UpdateNodeDrawers();
    }

    // Called when the window is disabled or closed
    private void OnDisable()
    {
        viewModel.Nodes.CollectionChanged -= OnNodesChanged;
        viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }
    

    private void OnNodesChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateNodeDrawers();
    }

    private void UpdateNodeDrawers()
    {
        NodeDrawers = viewModel.Nodes.Select(node => NodeDrawerFactory.CreateNodeDrawer(node)).ToList();
    }

    private void DrawInspector()
    {

        using (var verticalScope = new GUILayout.VerticalScope(panelstyle_inspector, GUILayout.Width(250), GUILayout.Height(this.position.height)))
        {
            if (viewModel.ActiveNode == null)
            {
                GUILayout.Label("Right click to add a node", inspectorText, GUILayout.Width(90));
            }
            else
            {
                ActiveNodeDrawView?.DrawNodeInspector();
            }
        }
    }

    private void DrawNodes()
    {
        BeginWindows();
        for(int index = 0; index < NodeDrawers.Count; index++)
        {
            NodeDrawers[index]?.DrawNode( index);
        }
        EndWindows();
    }

    private void DrawRibbon()
    {
        GUILayoutOption[] horizontalLayoutOptions = new GUILayoutOption[]
        {
                  GUILayout.Width(EditorGUIUtility.currentViewWidth),
                  GUILayout.Height(30)
        };

        using (var horizontalScope = new GUILayout.HorizontalScope(panelstyle_button, horizontalLayoutOptions))
        {

            void HandleFileDropdownOption(string option)
            {
                switch (option)
                {
                    case "New":
                        viewModel.NewFile();
                        showDropdown = false;
                        break;
                    case "Save As":
                        viewModel.SaveAs();
                        showDropdown = false;
                        break;
                    default:
                        break;
                }
            }

            using (var scope = new GUILayout.VerticalScope(GUILayout.Width(100)))
            {
                if (GUILayout.Button("File", GUILayout.Width(100), GUILayout.Height(30))) showDropdown = !showDropdown;
                if (showDropdown)
                {
                    foreach (var option in BranchConstants.FileDropdownOptions)
                        if (GUILayout.Button(option, GUILayout.Width(100))) HandleFileDropdownOption(option);
                }
            }

            if (GUILayout.Button("Save", GUILayout.Width(65), GUILayout.Height(30)))
            {
                viewModel.Save();
            }
            if (GUILayout.Button("Load", GUILayout.Width(65), GUILayout.Height(30)))
            {
                viewModel.Load();
            }

            if (GUILayout.Button("Inkle Script View", GUILayout.Width(120), GUILayout.Height(30)))
            {
                //The next epic part of this tool
            }

            if (GUILayout.Button("Locate Global Settings", GUILayout.Width(140), GUILayout.Height(30)))
            {
                viewModel.LocateGlobalSettings();
            }
        }
    }


    //Because of event lifecycle, clicking has to be checked before DrawNodes()-> GUI.DragWindow() in NodeDrawer
    private void HandleInputClicks()
    {
        Event e = Event.current;
        Vector2 mousePos = e.mousePosition;

        if (e.type == EventType.MouseDown && e.button == 1) //Right Click
        {
            ShowContextMenu(mousePos);
            e.Use();
        }

        if (e.button == 0 && e.type == EventType.MouseDown) //Left Click
        {

            // Check if any node contains the mouse position
            var selectedNodeDrawer = NodeDrawers
                .FirstOrDefault(nodeDrawer => nodeDrawer.WindowRect.Contains(e.mousePosition));

            if (selectedNodeDrawer == null)
            {
                viewModel.SelectedConnectionPoint = null;
                viewModel.IsDrawingHandle = false;
                return;
            }

            // Set the active node
            viewModel.ActiveNode = selectedNodeDrawer.Node;

            HandleConnectionPointSelected(e.mousePosition);
        }
    }


    public static void OnClickRemoveConnection(Connection connection)
    {
        ConnectionManager.Instance.Remove(connection);
    }

    //mayve move to viewModel?
    public void HandleConnectionPointSelected(Vector2 mousePos)
    {
        ConnectionPoint selectedPoint = ActiveNodeDrawView.GetHandlePoint(mousePos);
        if(selectedPoint != null)
        {
            //Clicked on the connection point start to draw Handle
            if (!viewModel.IsDrawingHandle)
            {
                viewModel.SelectedConnectionPoint = selectedPoint;
                viewModel.IsDrawingHandle = true;
                return;
            }
            //Already Drawing a curve, point been selected now a second one is
            else
            {
                ConnectionPoint fromPoint = ActiveNodeDrawView.GetHandlePoint(mousePos);
                //Opposite type and not of of the current node

                if ((fromPoint.Type != viewModel?.SelectedConnectionPoint.Type) && !ActiveNodeDrawView.Node.ContainsPoint(viewModel?.SelectedConnectionPoint))
                {
                    //Remove Connections
                    if (ConnectionManager.Instance.IsOutConnected(fromPoint, viewModel.SelectedConnectionPoint))
                    {
                        ConnectionManager.Instance.Remove(fromPoint, viewModel.SelectedConnectionPoint);
                    }
                    fromPoint.ConnectedTo = viewModel.SelectedConnectionPoint;

                    viewModel.SelectedConnectionPoint.ConnectedTo = fromPoint;

                    ConnectionManager.Instance.AddConnection(fromPoint, viewModel.SelectedConnectionPoint, OnClickRemoveConnection);
                    viewModel.IsDrawingHandle = false;
                }
            }
        }
    }


    private void DrawConnectionCurve()
    {
        Event e = Event.current;
        Vector2 mousePos = e.mousePosition;

        if (viewModel.IsDrawingHandle)
        {
            Vector2 hpoint = viewModel.SelectedConnectionPoint.GetGlobalPoint();
            Vector3 startPos = new Vector3(hpoint.x, hpoint.y, 0);
            Vector3 endPos = new Vector3(mousePos.x, mousePos.y, 0);

            //Goto Curve
            //If making line above the point
            if (viewModel.SelectedConnectionPoint.Type == ConnectionPointType.Out
                && hpoint.y > endPos.y)
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
    }




    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        if (!StartNodeDrawer.StartNodeAdded)
        {
            menu.AddItem(new GUIContent("Add Start Node"), false, () => 
            {
                viewModel.AddNode(mousePosition, NodeType.StartNode);
            });
        }
        //Needs to Add an Actor
        else if(!NodeManager.Instance.ActorsInScene().Any())
        {
            menu.AddItem(new GUIContent("Must add an actor in the Start Node"), false, () => { });
        }
        else if (NodeManager.Instance.ActorsInScene().Any(actor => actor.ActorGO == null))
        {
            menu.AddItem(new GUIContent("One of the actors have not been assigned in the Start Node."), false, () => { });
        }
        else
        {
            menu.AddItem(new GUIContent("Add Dialogue Node"), false, () => 
            { 
                viewModel.AddNode(mousePosition, NodeType.DialogueNode);
            });
            menu.AddItem(new GUIContent("Add Decision Node"), false, () => 
            {
                viewModel.AddNode(mousePosition, NodeType.DecisionNode);
            });
            menu.AddItem(new GUIContent("Add Action Node"), false, () => 
            { 
                viewModel.AddNode(mousePosition, NodeType.ActionNode);
            });
        }
        menu.ShowAsContext();
    }



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

//CURVE CLASS FROM
//https://answers.unity.com/questions/392606/line-drawing-how-can-i-interpolate-between-points.html
public static class Curver
{
    //arrayToCurve is original Vector3 array, smoothness is the number of interpolations. 
    public static Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve, float smoothness)
    {
        List<Vector3> points;
        List<Vector3> curvedPoints;
        int pointsLength = 0;
        int curvedLength = 0;

        if (smoothness < 1.0f) smoothness = 1.0f;

        pointsLength = arrayToCurve.Length;

        curvedLength = (pointsLength * Mathf.RoundToInt(smoothness)) - 1;
        curvedPoints = new List<Vector3>(curvedLength);

        float t = 0.0f;
        for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
        {
            t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

            points = new List<Vector3>(arrayToCurve);

            for (int j = pointsLength - 1; j > 0; j--)
            {
                for (int i = 0; i < j; i++)
                {
                    points[i] = (1 - t) * points[i] + t * points[i + 1];
                }
            }

            curvedPoints.Add(points[0]);
        }

        return (curvedPoints.ToArray());
    }
}
