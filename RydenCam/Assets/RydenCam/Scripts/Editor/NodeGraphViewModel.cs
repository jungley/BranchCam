using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.Editor.NodeDrawer;
using RydenCam.BranchCamEditor;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.BranchCamEditor.Serialization;
using RydenCam.Common;
using System.Collections.ObjectModel;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;

public class NodeGraphViewModel : INotifyPropertyChanged
{
    private NodeCC _activeNode { get; set; }
    public NodeCC ActiveNode
    {
        get => _activeNode;
        set
        {
            _activeNode = value;
            OnPropertyChanged(nameof(ActiveNode));
        }
    }

    public bool IsDrawingHandle { get; set; }

    public ConnectionPoint SelectedConnectionPoint { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;


    public ObservableCollection<NodeCC> Nodes => NodeManager.Instance.Nodes;

    public NodeGraphViewModel()
    {

    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }




    public void NewFile()
    {
        bool shouldReset = EditorUtility.DisplayDialog("Confirmation", "Are you sure you want to reset everything?", "Yes", "No");
        if (shouldReset)
        {
            Nodes.Clear();

            BranchCamEditorPreferences.SetLastFilePath(string.Empty);
            StartNodeDrawer.StartNodeAdded = false;
        }
    }


    public void Save()
    {
        if (string.IsNullOrEmpty(BranchCamEditorPreferences.GetLastFilePath()))
        {
            if (!NodeManager.Instance.IsValidSequence()) return;

            if (LoadFile.IsSavePathValid(BranchConstants.SaveAsTitle, NodeManager.Instance.GetSequenceName()))
            {
                SaveFile.SaveConversation();
            }
        }
        else
        {
            SaveFile.SaveConversation();
        }
    }

    public void SaveAs()
    {
        if (!NodeManager.Instance.IsValidSequence()) return;

        if (LoadFile.IsSavePathValid(BranchConstants.SaveAsTitle, NodeManager.Instance.GetSequenceName()))
        {
            SaveFile.SaveConversation();
        }
    }

    public void Load()
    {
        if (LoadFile.HasDialogueFile(BranchConstants.LoadFolderPanelTitle, BranchConstants.LoadFolderPanelTitle))
        {
            EditorController.Instance.ResetEverything();
            LoadFile.LoadSaveables();
        }
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

    public void LocateGlobalSettings()
    {

        GlobalSettingsData globalSetting = FindGlobalSetting();

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

    public void AddNode(Vector2 position, NodeType nodeType)
    {
        NodeCC newNode = null;
        switch (nodeType)
        {
            case NodeType.StartNode:
                newNode = new StartNode(position);
                StartNodeDrawer.StartNodeAdded = true;
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
        ActiveNode = newNode;
    }

    public void CreateConnection(NodeCC startNode, NodeCC endNode)
    {

    }

    public void RemoveConnection(NodeCC startNode, NodeCC endNode)
    {

    }

}
