using System.IO;
using UnityEngine;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System;
using UnityEditor;
using RydenCam.BranchCamEditor.Serialization.Saveables;

namespace RydenCam.BranchCamEditor.Serialization
{
    [ExecuteAlways]
    public static class SaveFile
    {

        public static void SaveConversation()
        {
            try
            {
                if (NodeManager.Instance.Length == 0)
                {
                    BranchLog.Log("Cannot save an empty file!");
                    return;
                }
                EditorStartNode startNodeRef = (EditorStartNode)NodeManager.Instance.StartNode;
                string name = string.IsNullOrWhiteSpace(startNodeRef.SequenceName) ? "NewDialogueFile" : startNodeRef.SequenceName;
                string defaultPath = $"Assets/RydenCam/DialogueFiles/";
                string directoryPath = Directory.Exists(BranchCamEditorPreferences.GetLastFileFolderPath())
                    ? BranchCamEditorPreferences.GetLastFileFolderPath()
                    : defaultPath;

                string directoryPathWithName = directoryPath += "/" + name;

                if (Directory.Exists(directoryPathWithName))
                {
                    Directory.Delete(directoryPathWithName, true);
                }

                Directory.CreateDirectory(directoryPathWithName);

                BranchCamEditorPreferences.SetLastFilePath(directoryPathWithName);

                List<Saveable> saveableList = NodeSerializer.SerializeNodes(NodeManager.Instance.GetList());

                List<string> jsonStrings = new List<string>();
                foreach (Saveable save in saveableList)
                {
                    string result = JsonUtility.ToJson(save);
                    jsonStrings.Add(result);
                }

                SaveDataContainer saveDataContainer = new SaveDataContainer(jsonStrings);
                string combinedJson = JsonUtility.ToJson(saveDataContainer);

                var finalPath = $"{directoryPathWithName}/{name}.json";

                File.WriteAllText(finalPath, combinedJson);

                AssetDatabase.Refresh();

                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(finalPath);

                EditorGUIUtility.PingObject(obj);

                BranchLog.Log("Saved File");
            }
            catch (Exception)
            {
                BranchLog.Error("An error with Saving occured");

            }
        }
    }
}


