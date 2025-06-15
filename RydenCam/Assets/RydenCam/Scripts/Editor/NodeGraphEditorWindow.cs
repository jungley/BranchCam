using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Assets.RydenCam.Scripts.Editor;
using Assets.RydenCam.Scripts.Editor.NodeDrawers;
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
using System.Net;
using Assets.RydenCam.Scripts.NodeCommands;

//NodeGraphEditorWindow is the View in MVVM
//NodeGraphViewModel is the View Model
//Nodes are the Model

namespace RydenCam.Editor
{
    public class NodeGraphEditorWindow : EditorWindow
    {
        private NodeGraphViewModel viewModel;

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

        private RibbonBuilder ribbonBuilder { get; set; }

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


            viewModel.HandleInputClicks();

            DrawNodes();

            DrawUserDragConnectionCurve();

            DrawConnections();

            MousePan();

            GUI.EndGroup();

            ribbonBuilder.DrawRibbon();

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
