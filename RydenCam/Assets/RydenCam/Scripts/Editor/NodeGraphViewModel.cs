using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Managers;
using Assets.RydenCam.Scripts.NodeCommands;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.BranchCamEditor.Serialization;
using RydenCam.Common;
using RydenCam.Editor;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class NodeGraphViewModel
{
    private Vector2 clickStartPos { get; set; }
    private bool isPanning { get; set; }
    private const float dragThreshold = 0.001f;

    //Need a reference to the window
    private NodeGraphEditorWindow editorWindow { get; set; }


    public bool IsDrawingHandle { get; set; }

    public static bool RedrawPreviewWindows { get; set; }

    public ConnectionPoint SelectedConnectionPoint { get; set; }

    public NodeGraphViewModel()
    {
        editorWindow = EditorWindow.GetWindow<NodeGraphEditorWindow>();
    }


    public void NewFile()
    {
        bool shouldReset = EditorUtility.DisplayDialog("Confirmation", "Are you sure you want to reset everything?", "Yes", "No");
        if (shouldReset)
        {
            ResetEverything();
            FilePathSaveManager.Instance.ClearLastFilePath(FilePathSaveManager.LastOpened_NodeGraphKey);
        }
    }

    public void Save()
    {
        var fileResult = FilePathSaveManager.Instance.GetLastFilePathSaved(FilePathSaveManager.LastOpened_NodeGraphKey);

        if (string.IsNullOrEmpty(fileResult))
        {
            NodeGraphSettingsManager.SaveAs();
        }
        else
        {
            NodeGraphSettingsManager.Save(fileResult);
        }
    }

    public void SaveAs()
    {
        NodeGraphSettingsManager.SaveAs();
    }

    public void Open()
    {
        NodeGraphSettingsManager.OpenAndLoad();
    }

    public void ResetEverything()
    {
        NodeManager.Instance.ClearActorsInScene();
        NodeManager.Instance.Clear();
        ConnectionManager.Instance.Clear();
        NodeManager.Instance.ActiveNode = null;
    }

    private GlobalSettingsData FindGlobalSetting()
    {
        string[] guids = AssetDatabase.FindAssets("t:GlobalSettingsData");

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GlobalSettingsData globalSetting = AssetDatabase.LoadAssetAtPath<GlobalSettingsData>(assetPath);
            if (globalSetting != null)
            {
                return globalSetting;
            }
        }

        return null;
    }

    public void OpenCameraShotEditor()
    {
        if (EditorWindow.HasOpenInstances<CameraShotEditor>()) return;

        CameraShotEditor camShotEditor = EditorWindow.GetWindow<CameraShotEditor>();
        camShotEditor.titleContent = new GUIContent("Camera Shot Editor View");

        var nodeGraphWindow = EditorWindow.GetWindow<NodeGraphEditorWindow>();

        Docker.Dock(nodeGraphWindow, camShotEditor, Docker.DockPosition.Bottom);
    }

    public void LocateGlobalSettings()
    {
        GlobalSettingsData globalSetting = FindGlobalSetting();

        //Focus on the Project Tab
        System.Type projectType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
        EditorWindow projectWindow = EditorWindow.GetWindow(projectType);
        projectWindow?.Focus();


        if (globalSetting != null)
        {
            // Asset exists, ping it
            EditorGUIUtility.PingObject(globalSetting);
        }
        else
        {
            // Asset doesn't exist, create it
            globalSetting = ScriptableObject.CreateInstance<GlobalSettingsData>();
            AssetDatabase.CreateAsset(globalSetting, "Assets/Resources/Global Settings.asset");
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(globalSetting);
        }
    }

    public void ToggleNodePreviewRender()
    {
        FilePathSaveManager.Instance.SettingsData.IsNodePreviewEnabled 
            = !FilePathSaveManager.Instance.SettingsData.IsNodePreviewEnabled;
    }

    public void HandleInputClicks(Vector2? mousePosOverride = null)
    {
        Event e = Event.current;
        Vector2 mousePos = mousePosOverride ?? e.mousePosition;

        switch (e.type)
        {
            case EventType.MouseDown:
                HandleMouseDown(e.button, mousePos);
                break;
       
            case EventType.MouseDrag:
                if (e.button == 0)
                    UpdateDragState(mousePos);
                break;
                
            case EventType.MouseUp:
                if (e.button == 0)
                {
                    if (IsDrawingHandle)
                    {
                        HandleConnectionPointSelected(mousePos);
                    }
                    HandleLeftMouseUp(mousePos);
                }
                break;
        }
    }

    private void HandleMouseDown(int button, Vector2 mousePos)
    {
        if (button == 0)
            HandleLeftMouseDown(mousePos);
        else if (button == 1)
            HandleRightMouseDown(mousePos);
    }

    private void HandleLeftMouseDown(Vector2 mousePos)
    {
        clickStartPos = mousePos;
        isPanning = false;

        var node = GetNodeFromMousePosition(mousePos);
        if (node == null)
            return;


        NodeManager.Instance.ActiveNode = node;
        HandleConnectionPointSelected(mousePos);
    }



    private void HandleLeftMouseUp(Vector2 mousePos)
    {
        if (isPanning)
        {
            isPanning = false;
            return;
        }

        //Click over inspector area
        //RS TODO Move InspectorPanelArea to VM?
        if (editorWindow.InspectorPanelArea.Contains(new Vector2(Math.Abs(mousePos.x), Math.Abs(mousePos.y))))
        {
            return;
        }

        NodeManager.Instance.ActiveNode = GetNodeFromMousePosition(mousePos);

        if (NodeManager.Instance.ActiveNode == null)
        {
            ClearConnectionSelection();
            GUI.FocusControl(null);
            editorWindow.Repaint();
        }
    }
   
    private void UpdateDragState(Vector2 currentMousePos)
    {
        if (Vector2.Distance(clickStartPos, currentMousePos) > dragThreshold)
        {
            isPanning = true;
        }
    }
    
    private void HandleRightMouseDown(Vector2 mousePos)
    {
        Node node = GetNodeFromMousePosition(mousePos);

        if (node is ITalkable talkableNode)
        {
            NodeManager.Instance.NodeCommandLookup.GetByKey(node, out NodeCommand nodeCommand);
            TalkableCommand talkableCommand = nodeCommand as TalkableCommand;
            talkableCommand.ShowAddRemoveMenu(mousePos);
        }
        else
        {
            ShowContextMenu(mousePos);
        }

        Event.current.Use();
    }

    private void ClearConnectionSelection()
    {
        SelectedConnectionPoint = null;
        IsDrawingHandle = false;
    }

    /// <summary>
    /// Returns the node from the NodeManager based on the mouse position.
    /// </summary>
    /// <param name="mousePosition"></param>
    public Node GetNodeFromMousePosition(Vector2 mousePosition)
    {

        NodeCommand command = NodeManager.Instance.NodeCommandLookup.Values.Where(x => x.WindowRect.Contains(mousePosition)).FirstOrDefault();
        if (command != null)
        {
            NodeManager.Instance.NodeCommandLookup.GetByValue(command, out Node node);
            return node;
        }
        return null;
    }

    
    public void HandleConnectionPointSelected(Vector2 mousePosition)
    {
        Node node = GetNodeFromMousePosition(mousePosition);

        if (node == null) return;

        NodeManager.Instance.ActiveNode = node;

        NodeCommand nodeCommand = NodeManager.Instance.GetNodeCommand(node);


        ConnectionPoint selectedStartPoint = nodeCommand.GetSelectedStartPoint(mousePosition);
        if (selectedStartPoint != null)
        {
            //Clicked on the connection point start to draw Handle
            if (!IsDrawingHandle)
            {
                SelectedConnectionPoint = selectedStartPoint;
                IsDrawingHandle = true;
                return;
            }
        } 
        //Already Drawing a line, point been selected now a second one is
        else
        {
            if (SelectedConnectionPoint == null) return;

            ConnectionPoint fromPoint = nodeCommand.SelectedEndPointFromNode(SelectedConnectionPoint.Type, mousePosition);

            if (fromPoint == null) return;

            //Remove Connections the point its connected to is already connected.
            if (ConnectionManager.Instance.IsOutConnected(fromPoint, SelectedConnectionPoint))
            {
                ConnectionManager.Instance.RemoveConnectionsFromPoints(fromPoint, SelectedConnectionPoint);
            }

            ConnectionManager.Instance.AddConnection(fromPoint, SelectedConnectionPoint);
            IsDrawingHandle = false;
            SelectedConnectionPoint = null;
            Event.current?.Use();
            editorWindow?.Repaint();
        }
    }
    
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        if (!NodeManager.Instance.StartNodeAdded)
        {
            menu.AddItem(new GUIContent("Add Start Node"), false, () =>
            {
                AddNode(mousePosition, NodeType.StartNode);
            });
        }
        //Needs to Add an Actor
        else if (!NodeManager.Instance.ActorsInScene.Any())
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
                AddNode(mousePosition, NodeType.DialogueNode);
            });
            menu.AddItem(new GUIContent("Add Decision Node"), false, () =>
            {
                AddNode(mousePosition, NodeType.DecisionNode);
            });
            menu.AddItem(new GUIContent("Add Action Node"), false, () =>
            {
                AddNode(mousePosition, NodeType.ActionNode);
            });
        }
        menu.ShowAsContext();
    }

    public void AddNode(Vector2 position, NodeType nodeType)
    {
        Node newNode = null;
        switch (nodeType)
        {
            case NodeType.StartNode:
                newNode = new StartNode(position);
                break;
            case NodeType.DialogueNode:
                newNode = new DialogueNode(position);
                break;
            case NodeType.DecisionNode:
                newNode =  new DecisionNode(position);
                break;
            case NodeType.ActionNode:
                newNode =  new ActionNode(position);
                break;
            default:
                return;
        }

        NodeManager.Instance.Nodes.Add(newNode);
        NodeManager.Instance.ActiveNode = newNode;
    }
}
