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
                string directoryPath = $"Assets/RydenCam/DialogueFiles/{name}/";


                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                }

                Directory.CreateDirectory(directoryPath);

                BranchCamEditorPreferences.SetLastFilePath(directoryPath);

                List<Saveable> saveableList = NodeSerializer.SerializeNodes(NodeManager.Instance.GetList());

                SaveDataContainer saveDataContainer = ScriptableObject.CreateInstance<SaveDataContainer>();
                saveDataContainer.saveables = saveableList;

                // Save the ScriptableObject
                //TODO PROCESS HERE WITH SAVING Need to weird stuff here TODO
                string path = directoryPath + name + ".asset";
                UnityEditor.AssetDatabase.CreateAsset(saveDataContainer, path);
                UnityEditor.AssetDatabase.SaveAssets();
                EditorUtility.SetDirty(saveDataContainer);

                BranchLog.Log("Saved File");
            }
            catch(Exception e)
            {
                BranchLog.Error("An error with Saving occured");

            }
        }
    }
}
