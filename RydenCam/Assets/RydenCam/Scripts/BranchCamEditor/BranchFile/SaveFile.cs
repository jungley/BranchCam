using System.IO;
using UnityEngine;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;

namespace RydenCam.BranchCamEditor.BranchFile
{
    // Constructs epic JSON Save Format
    [ExecuteAlways]
    public static class SaveFile
    {

        public static void SaveConversation()
        {
            if(NodeManager.Instance.Length == 0)
            {
                BranchLog.Log("Cannot save an empty file!");
                return;
            }
            EditorStartNode startNodeRef = (EditorStartNode)NodeManager.Instance.StartNode;
            string name = string.IsNullOrWhiteSpace(startNodeRef.SequenceName) ? "NewDialogueFile" : startNodeRef.SequenceName;
            string path = $"Assets/RydenCam/DialogueFiles/{name}/";

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
            BranchCamEditorPreferences.SetLastFilePath(path);

            for (int i = 0; i < NodeManager.Instance.Length; i++)
            {
                NodeType nodeType = NodeManager.Instance.GetNode(i).TypeOfNode;
                string jsonpath;

                switch (nodeType)
                {
                    case NodeType.StartNode:
                        jsonpath = $"{path}sta_{i}.json";
                        break;
                    case NodeType.DialogueNode:
                        jsonpath = $"{path}dia_{i}.json";
                        break;
                    case NodeType.DecisionNode:
                        jsonpath = $"{path}dec_{i}.json";
                        break;
                    case NodeType.ActionNode:
                        jsonpath = $"{path}act_{i}.json";
                        break;
                    case NodeType.GoToNode:
                        jsonpath = $"{path}got_{i}.json";
                        break;
                    default:
                        jsonpath = null;
                        Debug.LogError("An Error Occurred in Saving");
                        break;
                }

                if (jsonpath != null)
                {
                    Saveable savenode = NodeManager.Instance.GetNode(i).Saveable();
                    string json = JsonUtility.ToJson(savenode);
                    File.WriteAllText(jsonpath, json);
                }
            }
            BranchLog.Log("Saved File");
        }
    }
}
