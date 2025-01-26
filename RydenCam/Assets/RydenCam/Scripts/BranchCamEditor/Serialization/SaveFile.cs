using System.IO;
using UnityEngine;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using Assets.RydenCam.Scripts.BranchCamCC;

namespace RydenCam.BranchCamEditor.Serialization
{
    [ExecuteAlways]
    public static class SaveFile
    {

        public static void SaveConversation()
        {
            try
            {
                if (!NodeManager.Instance.IsValidSequence()) return;

                string name = NodeManager.Instance.GetSequenceName();
                string defaultPath = $"Assets/RydenCam/DialogueFiles";

                string directoryPath = Directory.Exists(BranchCamEditorPreferences.GetLastFileFolderPath())
                    ? BranchCamEditorPreferences.GetLastFileFolderPath()
                    : defaultPath;
                //Name is stripped and readded to ensure the file and the folder have the same name. 
                string directoryPathWithName = $"{directoryPath}/{name}";

                if (Directory.Exists(directoryPathWithName))
                {
                    Directory.Delete(directoryPathWithName, true);
                }

                Directory.CreateDirectory(directoryPathWithName);

                BranchCamEditorPreferences.SetLastFilePath(directoryPathWithName);

                List<Node> nodeList = NodeManager.Instance.Nodes.ToList();

                List<NodeData> nodeDatas = new List<NodeData>();
                foreach (Node save in nodeList)
                {
                    string jsonNode = JsonUtility.ToJson(save);
                    NodeType type = save.TypeOfNode;

                    nodeDatas.Add(new NodeData(type, jsonNode));
                }

                SaveDataContainer saveDataContainer = new SaveDataContainer(nodeDatas);
                string combinedJson = JsonUtility.ToJson(saveDataContainer);

                var finalPath = $"{directoryPathWithName}/{name}.json";

                File.WriteAllText(finalPath, combinedJson);

                AssetDatabase.Refresh();

                PingObject(finalPath);

                BranchLog.Log("Saved File");
            }
            catch (Exception)
            {
                BranchLog.Error("An error with Saving occured");

            }

            void PingObject(string path)
            {
                path = path.Replace("\\", "/");

                if (!path.StartsWith("Assets/")) return;

                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

                EditorGUIUtility.PingObject(obj);
            }
        }
    }
}


