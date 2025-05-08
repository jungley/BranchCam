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
using Assets.RydenCam.Scripts.BranchCamCC;
using System;

//NodeGraphEditorWindow is the View in MVVM
//NodeGraphViewModel is the View Model
//Nodes are the Model
public class NodeGraphEditorWindow : EditorWindow
{
    private NodeGraphViewModel viewModel;

    private NodeDrawerBase activeNodeDrawView { get; set; }
    private NodeDrawerBase ActiveNodeDrawView
    {
        get => activeNodeDrawView;
        set
        {
            if(value != activeNodeDrawView)
            {
                if(activeNodeDrawView is IClearable clearable)
                {
                    clearable.Clear();
                }
            }
            activeNodeDrawView = value;        }
    }

    //Window Properties
    static float panX = 0;
    static float panY = 0;
    private Rect lastEditorWindowPos;

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

    List<NodeDrawerBase> NodeDrawers { get; set; } = new List<NodeDrawerBase>();
    List<ConnectionDrawer> ConnectionDrawers { get; set; }



    //Panel Styles
    private static GUIStyle panelstyle_inspector;
    private static GUIStyle panelstyle_button;


    //Define areas for clicking 
    private Rect ButtonPanelArea => new Rect(Math.Abs(panX), Math.Abs(panY), Screen.width, 30);
    
    private Rect InspectorPanelArea => new Rect(Math.Abs(panX), Math.Abs(panY), ActiveNodeDrawView != null ? ActiveNodeDrawView.InspectorWidth : 230, Math.Abs(panY) + 1000);

    //Text Style
    private static GUIStyle inspectorText;

    static bool resourcesInitalized { get; set; } = false;


    void OnGUI()
    {
        if(!resourcesInitalized)
        {
            InitializeStaticResources();
        }



        GUI.BeginGroup(new Rect(panX, panY, 100000, 100000));

        DrawUserDragConnectionCurve();

        HandleInputClicks();

        DrawNodes();

        DrawConnections();

        MousePan();

        GUI.EndGroup();

        ribbonBuilder.DrawRibbon();

        DrawInspector();

        //For Debugging Purposes
        //GUI.Box(InspectorPanelArea, CreateSolidTextureFromRect(InspectorPanelArea, Color.red));

    }

    /*
     * For Debugging Purposes
    public Texture2D CreateSolidTextureFromRect(Rect rect, Color color)
    {
        // Create a new texture with width and height taken from the Rect
        int width = Mathf.FloorToInt(rect.width);
        int height = Mathf.FloorToInt(rect.height);

        Texture2D texture = new Texture2D(width, height);

        // Create an array of colors, fill it with the provided color
        Color[] colorArray = new Color[width * height];
        for (int i = 0; i < colorArray.Length; i++)
        {
            colorArray[i] = color;
        }

        // Set the pixels of the texture
        texture.SetPixels(colorArray);
        texture.Apply(); // Apply the changes to the texture

        return texture;
    }
    */
    private void MousePan()
    {
        var mousePosition = Event.current.mousePosition;

       //Prevent panning in Inspector Panel and Button Ribbon 
        if (InspectorPanelArea.Contains(mousePosition)) return;
        
        if(ButtonPanelArea.Contains(mousePosition)) return; 


        if (Event.current.type == EventType.MouseDrag)
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
        if (e.PropertyName == nameof(NodeManager.Instance.ActiveNode))
        {
            ActiveNodeDrawView = NodeDrawers.FirstOrDefault(x => x.Node == NodeManager.Instance.ActiveNode);
        }
    }

    [MenuItem("BranchCam/Launch Editor")]
    public static void OpenWindow()
    {
        NodeGraphEditorWindow window = GetWindow<NodeGraphEditorWindow>();
        window.titleContent = new GUIContent("Window/Node Graph Editor-(BranchCamCC)");
        window.minSize = new Vector2(400f, 400f);
        window.autoRepaintOnSceneChange = true;


        if(!string.IsNullOrEmpty(BranchCamEditorPreferences.GetLastFilePath()))
        {
            LoadFile.LoadSaveables();
        }

        InitializeStaticResources();

        window.CreateInitialNodeDrawers();

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

        panelstyle_button = new GUIStyle();
        panelstyle_button.normal.background = targetTextureButtonHeader;


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

        //Handles events in the NodeGraphWindow
        viewModel = new NodeGraphViewModel();

        ribbonBuilder = new RibbonBuilder(viewModel);

        NodeManager.Instance.Nodes.CollectionChanged += OnNodesChanged;
        NodeManager.Instance.PropertyChanged += OnActiveNodeUpdated;
        ConnectionManager.Instance.Connections.CollectionChanged += OnConnectionsChanged;

        //Draw Nodes & connections
        CreateInitialNodeDrawers();
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
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (var node in e.NewItems.Cast<Node>().ToList())
            {
                NodeDrawers.Add(NodeDrawerFactory.CreateNodeDrawer(node));
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (var node in e.OldItems.Cast<Node>().ToList())
            {
                NodeDrawers.Remove(NodeDrawers.FirstOrDefault(x => x.Node == node));
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            NodeDrawers?.Clear();
        }
    }

    private void UpdateConnectionDrawers()
    {
        ConnectionDrawers = ConnectionManager.Instance.Connections
            .Select(connection => new ConnectionDrawer(connection))
            .ToList();
    }

    
    private void CreateInitialNodeDrawers()
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
        if (InspectorPanelArea.Contains(e.mousePosition))
        {
            return;
        }

        if (e.type == EventType.MouseDown)
        {
            SetActiveNode(e.mousePosition);
        }

        // Check for right-click
        if (e.type == EventType.MouseDown && e.button == 1)
        {

            if(ActiveNodeDrawView is TalkableDrawerNode drawer)
            {
                drawer.ShowAddRemoveMenu(mousePos);
            }
            else
            {
                ShowContextMenu(mousePos);
                e.Use();
            }
        }

        // Check for left-click
        if (e.type == EventType.MouseDown && e.button == 0)
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

            HandleConnectionPointSelected(e.mousePosition);
        }
    }

    // Method to set the active node based on mouse position
    private void SetActiveNode(Vector2 mousePosition)
    {
        var selectedNodeDrawer = NodeDrawers
            .FirstOrDefault(nodeDrawer => nodeDrawer.WindowRect.Contains(mousePosition));

        NodeManager.Instance.ActiveNode = (selectedNodeDrawer != null) ? selectedNodeDrawer.Node : null;
        
        if(NodeManager.Instance.ActiveNode == null)
        {
            GUI.FocusControl(null);
            //Force a repaint to ensure the UI is updated
            GetWindow<NodeGraphEditorWindow>().Repaint();
        }
    }

    //maybe move to viewModel?
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
        else if(!NodeManager.Instance.ActorsInScene.Any())
        {
            menu.AddItem(new GUIContent("Must add an actor in the Start Node"), false, () => { });
        }
        else if (NodeManager.Instance.ActorsInScene.Any(actor => actor.ActorGO == null))
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

}
