using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Managers;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup;
using Assets.RydenCam.Scripts.Editor;
using Assets.RydenCam.Scripts.Editor.CameraShotEditor;
using Assets.RydenCam.Scripts.Editor.NodeDrawers;
using Assets.RydenCam.Scripts.NodeCommands;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Serialization;
using RydenCam.Editor.Ribbon;
using RydenCam.Editor.Styling;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
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
        public bool IsDrawingConnectionHandle => viewModel != null && viewModel.IsDrawingHandle;

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
        private float zoomScale = 1f;
        private const float MinZoomScale = 0.05f;
        private const float MaxZoomScale = 1.35f;
        private const float HardPanLimit = 50000f;
        private const float FramePadding = 120f;
        private bool useZoomTransform = true;
        private Rect lastEditorWindowPos;
        private bool isPanningCanvas;
        private bool windowStateInitialized;
        private Action pendingGraphPopup;

        public void ShowGraphPopup(GenericMenu menu, Rect graphAnchor)
        {
            float scale = useZoomTransform ? zoomScale : 1f;
            Rect viewport = GraphViewportArea;
            Rect windowAnchor = new Rect(
                viewport.x + (graphAnchor.x + SnapToPixel(panX)) * scale,
                viewport.y + (graphAnchor.y + SnapToPixel(panY)) * scale,
                graphAnchor.width * scale, graphAnchor.height * scale);
            // Native popup windows must be opened after restoring window coordinates.
            pendingGraphPopup = () => menu.DropDown(windowAnchor);
        }

        private static readonly MethodInfo GetTopClipRect = typeof(GUI).Assembly
            .GetType("UnityEngine.GUIClip")
            .GetMethod("GetTopRect", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        
        private List<NodeDrawer> NodeDrawers { get; set; } = new List<NodeDrawer>();
        private List<ConnectionDrawer> ConnectionDrawers { get; set; } = new List<ConnectionDrawer>();



        //Panel Styles
        private static GUIStyle panelstyle_inspector;
        
        //Define fixed UI areas in window space (do not move with graph pan)
        private Rect ButtonPanelArea => new Rect(0, 0, position.width, RibbonRenderer.Height);
        private Rect GraphViewportArea => new Rect(0, ButtonPanelArea.height, position.width, Mathf.Max(0f, position.height - ButtonPanelArea.height));

        public Rect InspectorPanelArea => new Rect(0, RibbonRenderer.Height, ActiveNodeDrawView != null ? ActiveNodeDrawView.InspectorWidth : 230, Mathf.Max(0f, position.height - RibbonRenderer.Height));

        //Text Style
        private static GUIStyle inspectorText;

        private static bool resourcesInitalized { get; set; } = false;


        void OnGUI()
        {
            if (!windowStateInitialized || viewModel == null || ribbonRenderer == null)
                InitializeWindowState();

            if(!resourcesInitalized)
            {
                InitializeStaticResources();
            }

            Vector2 mouseWindowPosition = Event.current.mousePosition;
            HandleZoom(Event.current);
            float effectiveZoom = useZoomTransform ? zoomScale : 1f;
            Rect graphViewport = GraphViewportArea;
            SanitizeViewState(graphViewport, effectiveZoom);

            float snappedPanX = SnapToPixel(panX);
            float snappedPanY = SnapToPixel(panY);
            Vector2 graphMousePosition = GetGraphMousePosition(mouseWindowPosition, graphViewport, snappedPanX, snappedPanY, effectiveZoom);

            // Hit-test in graph space, but open context menus in the original GUI
            // coordinate system so Unity anchors them at the actual mouse position.
            bool isMouseInFixedUi = ButtonPanelArea.Contains(mouseWindowPosition) || InspectorPanelArea.Contains(mouseWindowPosition);
            if (!isMouseInFixedUi)
                viewModel.HandleInputClicks(graphMousePosition);

            EditorGUI.DrawRect(graphViewport, BranchCamEditorTheme.CanvasBackground);
            Matrix4x4 previousMatrix = GUI.matrix;
            // EditorWindow supplies an outer clip before OnGUI. At zoom < 1,
            // that clip otherwise cuts the graph off at windowSize * zoom.
            // Expand it in graph units, preserving Unity's actual tab/dock offset.
            Rect editorClip = (Rect)GetTopClipRect.Invoke(null, null);
            Rect zoomClip = editorClip;
            zoomClip.width /= Mathf.Min(1f, effectiveZoom);
            zoomClip.height /= Mathf.Min(1f, effectiveZoom);
            GUI.EndClip();
            GUI.BeginClip(zoomClip);
            GUI.matrix = previousMatrix * Matrix4x4.Scale(new Vector3(effectiveZoom, effectiveZoom, 1f));
            // Establish the clip in scaled coordinates. Pan belongs to the clip's
            // content offset so GUI.Window uses the same bounds as the graph.
            GUI.BeginClip(new Rect(graphViewport.x / effectiveZoom, graphViewport.y / effectiveZoom,
                graphViewport.width / effectiveZoom, graphViewport.height / effectiveZoom),
                new Vector2(snappedPanX, snappedPanY), Vector2.zero, false);
            try
            {
                DrawNodes();
                DrawUserDragConnectionCurve(graphMousePosition);
                DrawConnections();
            }
            finally
            {
                GUI.EndClip();
                GUI.matrix = previousMatrix;
                GUI.EndClip();
                GUI.BeginClip(editorClip);
            }

            // Event.delta must be read outside the scaled clip; inside it Unity
            // has already divided by zoom, which would scale pan twice.
            MousePan(mouseWindowPosition, graphMousePosition);

            // Ensure graph rendering cannot leak global GUI state into fixed UI panels.
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;
            

            ribbonRenderer.Draw(position.width);

            DrawInspector();
            Action popup = pendingGraphPopup;
            pendingGraphPopup = null;
            popup?.Invoke();
        }

        private void MousePan(Vector2 mouseWindowPosition, Vector2 graphMousePosition)
        {
            Event e = Event.current;

            //Prevent panning in Inspector Panel and Button Ribbon 
            if (InspectorPanelArea.Contains(mouseWindowPosition))
            {
                if (e.rawType == EventType.MouseUp)
                {
                    isPanningCanvas = false;
                }
                return;
            }

            if(ButtonPanelArea.Contains(mouseWindowPosition))
            {
                if (e.rawType == EventType.MouseUp)
                {
                    isPanningCanvas = false;
                }
                return;
            }


            // Left click drag pans only when starting on empty canvas.
            if (e.type == EventType.MouseDown)
            {
                bool isLeftButton = e.button == 0;
                bool clickedNode = viewModel.GetNodeFromMousePosition(graphMousePosition) != null;
                bool startPan = isLeftButton && !clickedNode;
                if (startPan)
                {
                    isPanningCanvas = true;
                    e.Use();
                    return;
                }
            }

            // rawType catches mouse-up even when the event was already consumed elsewhere
            if (e.rawType == EventType.MouseUp)
            {
                isPanningCanvas = false;
                return;
            }

            if (isPanningCanvas && e.type == EventType.MouseDrag && e.button == 0)
            {
                
                //The EditorWindow is not being dragged
                if (lastEditorWindowPos == position)
                {
                    //Weird Jumping Check
                    int difference = 70;
                    if ((e.delta.x > -difference && e.delta.x < difference)
                        && (e.delta.y > -difference && e.delta.y < difference))
                    {
                        float panDeltaDivisor = useZoomTransform ? zoomScale : 1f;
                        panX += e.delta.x / panDeltaDivisor;
                        panY += e.delta.y / panDeltaDivisor;
                        Repaint();
                        e.Use();
                    }
                }
                
                
            } 
        }

        private static float SnapToPixel(float value)
        {
            float pixelsPerPoint = Mathf.Max(1f, EditorGUIUtility.pixelsPerPoint);
            return Mathf.Round(value * pixelsPerPoint) / pixelsPerPoint;
        }

        private Vector2 GetGraphMousePosition(Vector2 mouseWindowPosition, Rect graphViewport, float snappedPanX, float snappedPanY, float effectiveZoom)
        {
            return new Vector2(
                ((mouseWindowPosition.x - graphViewport.x) / effectiveZoom) - snappedPanX,
                ((mouseWindowPosition.y - graphViewport.y) / effectiveZoom) - snappedPanY
            );
        }

        private void HandleZoom(Event e)
        {
            if (e.type != EventType.ScrollWheel)
            {
                return;
            }

            if (!useZoomTransform)
            {
                return;
            }

            Vector2 mouseWindowPosition = e.mousePosition;
            if (InspectorPanelArea.Contains(mouseWindowPosition) || ButtonPanelArea.Contains(mouseWindowPosition) || !GraphViewportArea.Contains(mouseWindowPosition))
            {
                return;
            }

            Rect graphViewport = GraphViewportArea;
            float oldZoom = zoomScale;
            // A constant percentage per wheel step stays smooth after Frame All,
            // even when fitting a large graph requires a very small scale.
            zoomScale = Mathf.Clamp(oldZoom * Mathf.Exp(-e.delta.y * 0.02f), MinZoomScale, MaxZoomScale);

            if (Mathf.Approximately(oldZoom, zoomScale))
            {
                return;
            }

            // Zoom toward cursor: keep graph-space point under cursor fixed.
            float graphX = ((mouseWindowPosition.x - graphViewport.x) / oldZoom) - panX;
            float graphY = ((mouseWindowPosition.y - graphViewport.y) / oldZoom) - panY;

            panX = ((mouseWindowPosition.x - graphViewport.x) / zoomScale) - graphX;
            panY = ((mouseWindowPosition.y - graphViewport.y) / zoomScale) - graphY;

            e.Use();
            Repaint();
        }

        private void OnInspectorUpdate()
        {
            //If window was resized or moved
            lastEditorWindowPos = position;

            if(viewModel != null && viewModel.IsDrawingHandle)
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

            var fileResult = FilePathSaveManager.Instance.GetLastFilePathSaved(FilePathSaveManager.LastOpened_NodeGraphKey);
            NodeGraphSettingsManager.Load(fileResult);

            InitializeStaticResources();

            window.CreateInitialNodeDrawers();

            window.Show();

        }

        private static void InitializeStaticResources()
        {
            Instance = EditorWindow.GetWindow<NodeGraphEditorWindow>();

            panelstyle_inspector = new GUIStyle();
            panelstyle_inspector.normal.background = BranchCamEditorTheme.GetSolidTexture(BranchCamEditorTheme.PanelBackground);
            panelstyle_inspector.padding = new RectOffset(10, 10, 12, 10);

            //Text
            inspectorText = new GUIStyle();
            inspectorText.normal.textColor = BranchCamEditorTheme.TextSecondary;
            inspectorText.fontSize = BranchCamEditorTheme.FontBody;

            //------------------------ RS TODO
            // If Settings has any saved shots, load them

            //Else
            /*
            CameraShotsManager.Instance.CameraShots.Clear();
            CameraShotsManager.Instance.CameraShots.Add(new CameraShotConfiguration(shotName: "Default") { IsDefault = true });
            CameraShotsManager.Instance.CameraShots.Add(new CameraShotConfiguration(shotName: "Shot 1"));
            CameraShotsManager.Instance.CameraShots.Add(new CameraShotConfiguration(shotName: "Shot 2"));
            CameraShotsManager.Instance.CameraShots.Add(new CameraShotConfiguration(shotName: "Shot 3"));
            */

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
            titleContent = new GUIContent("BranchCam");
            // Node drawers construct EditorStyles and must be initialized inside
            // OnGUI, after Unity's GUI skin is available following a reload.
            windowStateInitialized = false;
            Repaint();
        }

        private void InitializeWindowState()
        {
            // Hot reload can preserve the EditorWindow while clearing nonserialized
            // helpers. Rebuild them on demand and make subscriptions idempotent.
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            //Handles events in the NodeGraphWindow
            viewModel = new NodeGraphViewModel();

            // Static graph state is lost during a domain reload while the EditorWindow
            // itself survives. Restore the last saved graph before rebuilding drawers.
            if (NodeManager.Instance.Nodes.Count == 0)
            {
                string lastGraphPath = FilePathSaveManager.Instance
                    .GetLastFilePathSaved(FilePathSaveManager.LastOpened_NodeGraphKey);
                if (!string.IsNullOrEmpty(lastGraphPath))
                    NodeGraphSettingsManager.Load(lastGraphPath);
            }

            var ribbonDefinition = new RibbonDefinitionBuilder()
            .AddDropdown("File")
                .AddDropdownOption("File", "New", viewModel.NewFile)
                .AddDropdownOption("File", "Open", viewModel.Open)
                .AddDropdownOption("File", "Save", viewModel.Save)
                .AddDropdownOption("File", "Save As", viewModel.SaveAs)
             .AddButton("Open", viewModel.Open)
             .AddButton("Save", viewModel.Save)
             .AddButton("Toggle Preview", () => viewModel.ToggleNodePreviewRender(), width: 105)
             .AddButton("Frame All", FrameAllNodes, width: 72)
             .AddButton("PlayMode Settings", () => viewModel.LocateGlobalSettings(), width: 120)
            .Build();

            ribbonRenderer = new RibbonRenderer(ribbonDefinition);

            NodeManager.Instance.Nodes.CollectionChanged -= OnNodesChanged;
            NodeManager.Instance.Nodes.CollectionChanged += OnNodesChanged;
            NodeManager.Instance.PropertyChanged -= OnActiveNodeUpdated;
            NodeManager.Instance.PropertyChanged += OnActiveNodeUpdated;
            ConnectionManager.Instance.Connections.CollectionChanged -= OnConnectionsChanged;
            ConnectionManager.Instance.Connections.CollectionChanged += OnConnectionsChanged;

            //Draw Nodes & connections
            CreateInitialNodeDrawers();
            UpdateConnectionDrawers();
            windowStateInitialized = true;
            EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    viewModel.OpenDefaultPanels();
                    FrameAllNodes();
                }
            };
        }

        // Called when the window is disabled or closed
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            NodeManager.Instance.Nodes.CollectionChanged -= OnNodesChanged;
            NodeManager.Instance.PropertyChanged -= OnActiveNodeUpdated;
            ConnectionManager.Instance.Connections.CollectionChanged -= OnConnectionsChanged;

            //Save Editor Settings
            //SaveFile.SaveEditorSettings();
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
                    GUILayout.Label("Right click to add a node", inspectorText, GUILayout.Width(190));
                }
                else
                {
                    ActiveNodeDrawView?.DrawNodeInspector();
                }
            }
        }

        private void DrawNodes()
        {
            if (NodeDrawers == null) return;

            BeginWindows();
            for(int index = 0; index < NodeDrawers.Count; index++)
            {
                NodeDrawer drawer = NodeDrawers[index];
                if (drawer == null) continue;
                drawer.DrawNode(index);
            }
            EndWindows();
        }

        public void DrawConnections()
        {
            if (ConnectionDrawers == null) return;

            foreach (var connectionDrawer in ConnectionDrawers)
            {
                connectionDrawer?.Draw();
            }
        }

        private void DrawUserDragConnectionCurve(Vector2 graphMousePoint)
        {
            if (viewModel.IsDrawingHandle)
            {
                var selectedPoint = viewModel.SelectedConnectionPoint;

                var drawer = new ConnectionDrawer();
                drawer.DrawUserHandle(selectedPoint, graphMousePoint);

                Node node = viewModel.GetNodeFromMousePosition(graphMousePoint);
                if(node != null)
                {
                    NodeCommand command = NodeManager.Instance.GetNodeCommand(node);
                    command.CreateHighlightTexture(Color.green);
                    command.HighlightNode();
                    command.ClearHighlightTexture();

                    // Highlight hovered decision option text area while dragging a connection.
                    if (selectedPoint != null &&
                        command is DecisionNodeCommand decisionCommand &&
                        decisionCommand.TryGetHoveredDecisionRect(graphMousePoint, out Rect hoveredRect))
                    {
                        DrawRectOutline(hoveredRect, Color.green, 2f);
                    }
                }
            }
        }

        private static void DrawRectOutline(Rect rect, Color color, float thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        }

        private void SanitizeViewState(Rect graphViewport, float effectiveZoom)
        {
            bool invalidZoom = float.IsNaN(zoomScale) || float.IsInfinity(zoomScale);
            bool invalidPan = float.IsNaN(panX) || float.IsInfinity(panX) || float.IsNaN(panY) || float.IsInfinity(panY);
            if (invalidZoom || invalidPan)
            {
                ResetView();
                return;
            }

            zoomScale = Mathf.Clamp(zoomScale, MinZoomScale, MaxZoomScale);
            panX = Mathf.Clamp(panX, -HardPanLimit, HardPanLimit);
            panY = Mathf.Clamp(panY, -HardPanLimit, HardPanLimit);

            if (effectiveZoom <= 0f || graphViewport.width <= 0f || graphViewport.height <= 0f)
            {
                return;
            }
        }

        private bool TryGetNodeBounds(out Rect bounds)
        {
            bounds = default;
            var nodes = NodeManager.Instance.Nodes;
            if (nodes == null || nodes.Count == 0)
            {
                return false;
            }

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
                if (node == null) continue;
                minX = Mathf.Min(minX, node.EditorPosition.x);
                minY = Mathf.Min(minY, node.EditorPosition.y);
                maxX = Mathf.Max(maxX, node.EditorPosition.x + node.NodeWidth);
                maxY = Mathf.Max(maxY, node.EditorPosition.y + node.NodeHeight);
            }

            if (minX == float.MaxValue || minY == float.MaxValue)
            {
                return false;
            }

            bounds = Rect.MinMaxRect(minX, minY, maxX, maxY);
            return true;
        }

        public void FrameImportedGraph()
        {
            EditorApplication.delayCall += () => { if (this != null) { FrameAllNodes(); Repaint(); } };
        }

        private void FrameAllNodes()
        {
            Rect graphViewport = GraphViewportArea;
            if (graphViewport.width <= 0f || graphViewport.height <= 0f)
            {
                return;
            }

            if (!TryGetNodeBounds(out Rect bounds))
            {
                ResetView();
                return;
            }

            float contentWidth = Mathf.Max(1f, bounds.width + (FramePadding * 2f));
            float contentHeight = Mathf.Max(1f, bounds.height + (FramePadding * 2f));
            float inspectorWidth = InspectorPanelArea.width;
            float fitX = Mathf.Max(1f, graphViewport.width - inspectorWidth) / contentWidth;
            float fitY = graphViewport.height / contentHeight;

            zoomScale = Mathf.Clamp(Mathf.Min(fitX, fitY), MinZoomScale, MaxZoomScale);
            float effectiveZoom = useZoomTransform ? zoomScale : 1f;
            panX = (inspectorWidth + graphViewport.width) / (2f * effectiveZoom) - bounds.center.x;
            panY = graphViewport.height / (2f * effectiveZoom) - bounds.center.y;
            Repaint();
        }

        private void ResetView()
        {
            panX = 0f;
            panY = 0f;
            zoomScale = 1f;
            Repaint();
        }
    }
}
