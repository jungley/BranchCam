using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Assets.RydenCam.Scripts.Editor;
using Assets.RydenCam.Scripts.Editor.NodeDrawer;
using RydenCam.Common;
using RydenCam.BranchCamEditor;
using RydenCam.BranchCamEditor.Serialization;
using System.Linq;
using RydenCam.BranchCamEditor.Managers;
using System.Collections.Specialized;
using System.ComponentModel;
using RydenCam.BranchCamEditor.Nodes.Connections;
using System;
using RydenCam.BranchCamEditor.PreviewRender;
using Assets.RydenCam.Scripts.BranchCamCC;

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
    private Rect lastEditorWindowPos;
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

    private RibbonBuilder ribbonBuilder { get; set; }

    List<NodeDrawerBase> NodeDrawers { get; set; }
    List<ConnectionDrawer> ConnectionDrawers { get; set; }



    //Panels
    private static GUIStyle panelstyle_inspector;
    private static GUIStyle panelstyle_button;
    private static Rect ButtonPanelArea;
    private static Rect InspectorPanelArea;

    //Text Style
    private static GUIStyle inspectorText;

    static bool resourcesInitalized { get; set; } = false;


    void OnGUI()
    {
        if(!resourcesInitalized)
        {
            InitializeStaticResources();
        }


        DrawGrid(gridSpacing: 20f, gridOpacity: 0.5f, gridColor: Color.white);

        GUI.BeginGroup(new Rect(panX, panY, 100000, 100000));

        DrawUserDragConnectionCurve();

        HandleInputClicks();

        DrawNodes();

       // DrawPreviewWindows();

        DrawConnections();

        MousePan();

        GUI.EndGroup();

        ribbonBuilder.DrawRibbon();

        DrawInspector();


    }
    /*
    private void DrawPreviewWindows()
    {
        //draws the preview windows next to the nodes.
        if (NodeGraphViewModel.RedrawPreviewWindows)
        {
            Debug.Log("Redrawing.");
            dialoguePreviewWindow.DrawPreviewWindows(NodeManager.Instance.Nodes.ToList());
        }
        else
        {
            dialoguePreviewWindow.DrawCachedWindows(NodeManager.Instance.Nodes.ToList());
        }

        NodeGraphViewModel.RedrawPreviewWindows = false;
    }
    */

    private void MousePan()
    {
        var mousePosition = Event.current.mousePosition;
        
        if (Event.current.type == EventType.MouseDrag &&
            //mouse not over 
            !(InspectorPanelArea.Contains(mousePosition) || ribbonBuilder.ButtonPanelArea.Contains(mousePosition)))
        {
            //The EditorWindow is not being dragged
            if (lastEditorWindowPos == position)
            {

                //Weird Jumping Check
                int difference = 70;
                if ((Event.current.delta.x > -difference && Event.current.delta.x < difference)
                    && (Event.current.delta.y > -difference && Event.current.delta.y < difference))
                {
                    panX += Event.current.delta.x;
                    panY += Event.current.delta.y;
                    Repaint();
                }
            }
        }
    }



    private void OnInspectorUpdate()
    {
        //If window was resized or moved
        lastEditorWindowPos = position;

        if(viewModel.IsDrawingHandle)
        {
            Repaint();
        }
    }

    public void OnActiveNodeUpdated(object sender, PropertyChangedEventArgs e)
    {

        ActiveNodeDrawView?.DeSelect();
       
        //Update the Drawer
        if (e.PropertyName == nameof(NodeManager.Instance.ActiveNode))
        {
            ActiveNodeDrawView = NodeDrawerFactory.CreateNodeDrawer(NodeManager.Instance.ActiveNode);
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


        //PreviewRender stuff
       // dialoguePreviewWindow = DialoguePreview.CreateAndPopulateMeshes(NodeManager.Instance.Nodes.Where(x => x is ITalkable).ToArray());

        //OnNodePropertyChanged += editor.MarkForRedraw;
        
        //TODO:OnPropertyChanged should be on the node command
        /*
        for (int i = 0; i < NodeManager.Instance.Length; i++)
        {
            NodeManager.Instance.GetNode(i).OnPropertyChanged += (evt) => { OnNodePropertyChanged?.Invoke(evt); };
        }
        */

        //MarkForRedraw();
        NodeGraphViewModel.RedrawPreviewWindows = true;


        resourcesInitalized = true;
    }

    void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingPlayMode)
        {
            Repaint(); // Force a repaint when entering or exiting play mode
        }
    }
    


    // Called when the window is enabled or created
    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        //Event Handlers
        viewModel = new NodeGraphViewModel();

        ribbonBuilder = new RibbonBuilder(viewModel);

        NodeManager.Instance.Nodes.CollectionChanged += OnNodesChanged;
        NodeManager.Instance.PropertyChanged += OnActiveNodeUpdated;
        ConnectionManager.Instance.Connections.CollectionChanged += OnConnectionsChanged;

        //Draw Nodes
        UpdateNodeDrawers();
        UpdateConnectionDrawers();
    }

    // Called when the window is disabled or closed
    private void OnDisable()
    {
        NodeManager.Instance.Nodes.CollectionChanged -= OnNodesChanged;
        NodeManager.Instance.PropertyChanged -= OnActiveNodeUpdated;
        ConnectionManager.Instance.Connections.CollectionChanged -= OnConnectionsChanged;
    }

    private void OnConnectionsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateConnectionDrawers();
    }
    

    private void OnNodesChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateNodeDrawers();
    }

    private void UpdateConnectionDrawers()
    {
        ConnectionDrawers = ConnectionManager.Instance.Connections
            .Select(connection => new ConnectionDrawer(connection))
            .ToList();
    }

    private void UpdateNodeDrawers()
    {
        NodeDrawers = NodeManager.Instance.Nodes.Select(node => NodeDrawerFactory.CreateNodeDrawer(node)).ToList();
    }

    private void DrawInspector()
    {

        using (var verticalScope = new GUILayout.VerticalScope(panelstyle_inspector, GUILayout.Width(250), GUILayout.Height(this.position.height)))
        {
            if (ActiveNodeDrawView == null)
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

    public void DrawConnections()
    {
        foreach (var connectionDrawer in ConnectionDrawers)
        {
            connectionDrawer.Draw();
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
                Repaint();
                return;
            }

            // Set the active node
            NodeManager.Instance.ActiveNode = selectedNodeDrawer.Node;

            HandleConnectionPointSelected(e.mousePosition);
        }
    }

    //mayve move to viewModel?
    public void HandleConnectionPointSelected(Vector2 mousePos)
    {
        ConnectionPoint selectedPoint = ActiveNodeDrawView?.GetHandlePoint(mousePos);
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

                if (fromPoint.Type != viewModel?.SelectedConnectionPoint.Type)
                {
                    //Remove Connections the point its connected to is already connected.
                    if (ConnectionManager.Instance.IsOutConnected(fromPoint, viewModel.SelectedConnectionPoint))
                    {
                        ConnectionManager.Instance.RemoveConnectionsFromPoints(fromPoint, viewModel.SelectedConnectionPoint);
                    }

                    ConnectionManager.Instance.AddConnection(fromPoint, viewModel.SelectedConnectionPoint);
                    viewModel.IsDrawingHandle = false;
                }
            }
        }
    }

    private void DrawUserDragConnectionCurve()
    {
        if (viewModel.IsDrawingHandle)
        {
            var selectedPoint = viewModel.SelectedConnectionPoint;
            var globalPoint = Event.current.mousePosition;

            var drawer = new ConnectionDrawer();
            drawer.DrawFromUserHandle(selectedPoint, globalPoint);
        }
    }


    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        if (!NodeManager.StartNodeAdded)
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
