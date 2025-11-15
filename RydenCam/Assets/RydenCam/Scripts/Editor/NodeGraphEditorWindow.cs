using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Managers;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup;
using Assets.RydenCam.Scripts.Editor;
using Assets.RydenCam.Scripts.Editor.CameraShotEdtior;
using Assets.RydenCam.Scripts.Editor.NodeDrawers;
using Assets.RydenCam.Scripts.NodeCommands;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Serialization;
using RydenCam.Editor.CamersaShotEditor;
using RydenCam.Editor.Ribbon;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using UnityEditor;
using UnityEngine;

//NodeGraphEditorWindow is the View in MVVM
//NodeGraphViewModel is the View Model
//Nodes are the Model

namespace RydenCam.Editor
{
    public class NodeGraphEditorWindow : EditorWindow
    {
        public static NodeGraphEditorWindow Instance { get; private set; }

        private NodeGraphViewModel viewModel;

        private RibbonRenderer ribbonRenderer;

        private NodeDrawer activeNodeDrawView { get; set; }
        private NodeDrawer ActiveNodeDrawView
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
                activeNodeDrawView = value;
            }
        }

        //Window Properties
        public static float panX = 0;
        public static float panY = 0;
        private Rect lastEditorWindowPos;

        private static Texture2D _targetTextureInspector { get; set; }
        private static Texture2D TargetTextureInspector
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

      
        private List<NodeDrawer> NodeDrawers { get; set; } = new List<NodeDrawer>();
        private List<ConnectionDrawer> ConnectionDrawers { get; set; }



        //Panel Styles
        private static GUIStyle panelstyle_inspector;
        private static GUIStyle panelstyle_button;


        //Define areas for clicking 
        private Rect ButtonPanelArea => new Rect(Math.Abs(panX), Math.Abs(panY), Screen.width, 30);

        public Rect InspectorPanelArea => new Rect(Math.Abs(panX), Math.Abs(panY), ActiveNodeDrawView != null ? ActiveNodeDrawView.InspectorWidth : 230, Math.Abs(panY) + 1000);

        //Text Style
        private static GUIStyle inspectorText;

        private static bool resourcesInitalized { get; set; } = false;


        void OnGUI()
        {
            if(!resourcesInitalized)
            {
                InitializeStaticResources();
            }

            GUI.BeginGroup(new Rect(panX, panY, 100000, 100000));

            viewModel.HandleInputClicks();//Must be called before drawing to MGUI constraints

            DrawNodes();

            DrawUserDragConnectionCurve();

            DrawConnections();

            MousePan();

            GUI.EndGroup();

            ribbonRenderer.Draw();

            DrawInspector();
        }

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
                ActiveNodeDrawView = NodeDrawers.FirstOrDefault(x => x.Node.NodeId == NodeManager.Instance.ActiveNode?.NodeId);
            }
        }

        [MenuItem("BranchCam/Launch Editor")]
        public static void OpenWindow()
        {
            NodeGraphEditorWindow window = GetWindow<NodeGraphEditorWindow>();
            window.titleContent = new GUIContent("BranchCam");
            window.minSize = new Vector2(400f, 400f);

            // Set initial position and size
            Rect newPos = new Rect(100, 100, 1500, 800);
            window.position = newPos;

            window.autoRepaintOnSceneChange = true;

            LoadFile.LoadSaveables(EditorSettingsManager.Instance.LastUsedJsonPath);

            LoadFile.LoadEditorSettings();

            InitializeStaticResources();

            window.CreateInitialNodeDrawers();

            window.ShowUtility();

            //Dock Window
            CameraShotEditor editorWindow = EditorWindow.GetWindow<CameraShotEditor>();
            editorWindow.titleContent = new GUIContent("Camera Shot Editor View");
            editorWindow.NodeGraphViewModel = window.viewModel;
            Docker.Dock(window, editorWindow, Docker.DockPosition.Bottom);
        }

        private static void InitializeStaticResources()
        {
            Instance = EditorWindow.GetWindow<NodeGraphEditorWindow>();

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

            //------------------------ RS TODO
            // If Settings has any saved shots, load them

            //Else
            CameraShotsManager.Instance.CameraShots.Clear();
            CameraShotsManager.Instance.CameraShots.Add(new CamShotConfig(shotName: "Default") { IsDefault = true });
            CameraShotsManager.Instance.CameraShots.Add(new CamShotConfig(shotName: "Shot 1"));
            CameraShotsManager.Instance.CameraShots.Add(new CamShotConfig(shotName: "Shot 2"));
            CameraShotsManager.Instance.CameraShots.Add(new CamShotConfig(shotName: "Shot 3"));

            //------------------------

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

            var ribbonDefinition = new RibbonDefinitionBuilder()
            .AddDropdown("File")
                .AddDropdownOption("File", "New", viewModel.NewFile)
                .AddDropdownOption("File", "Open", viewModel.Open)
                .AddDropdownOption("File", "Save", viewModel.Save)
                .AddDropdownOption("File", "Save As", viewModel.SaveAs)
             .AddButton("Save", viewModel.Save)
             .AddButton("Toggle Preview", () => viewModel.ToggleNodePreviewRender(), width: 120)
             .AddButton("Shot Configuration", () => viewModel.OpenCameraShotEditor(), width:140)
            .Build();

            ribbonRenderer = new RibbonRenderer(ribbonDefinition);

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

            //Save Editor Settings
            SaveFile.SaveEditorSettings();
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
                NodeDrawers[index]?.DrawNode(index);
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

        private void DrawUserDragConnectionCurve()
        {
            if (viewModel.IsDrawingHandle)
            {
                var selectedPoint = viewModel.SelectedConnectionPoint;
                var globalMousePoint = Event.current.mousePosition;

                var drawer = new ConnectionDrawer();
                drawer.DrawUserHandle(selectedPoint, globalMousePoint);

                Node node = viewModel.GetNodeFromMousePosition(globalMousePoint);
                if(node != null)
                {
                    NodeCommand command = NodeManager.Instance.GetNodeCommand(node);
                    command.CreateHighlightTexture(Color.green);
                    command.HighlightNode();
                    command.ClearHighlightTexture();
                }
            }
        }
    }
}
