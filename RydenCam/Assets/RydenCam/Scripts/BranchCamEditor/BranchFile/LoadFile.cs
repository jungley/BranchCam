using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;

namespace RydenCam.BranchCamEditor.BranchFile
{
    [ExecuteAlways]
    public static class LoadFile
    {
        private static string path;

        public static void SelectDialogueWindow()
        {
#if UNITY_EDITOR
            string path = EditorUtility.OpenFolderPanel("Choose a folder containing Dialogue JSON files only", BranchConstants.DialogueFolder, "Choose a folder containing Dialogue JSON files only");
            BranchCamEditorPreferences.SetLastFilePath(path);
#endif

        }

        //TODO: Make more extensive check?
        public static bool IsValidEditorPath()
        {
            string filepath = BranchCamEditorPreferences.GetLastFilePath();
            return !string.IsNullOrEmpty(filepath);
        }

        public static bool IsValidDialogueTriggerPath(string path)
        {
            return !string.IsNullOrEmpty(path);
        }

        public static FileInfo[] ReorderStartFirst(FileInfo[] info)
        {
            List<FileInfo> reordered = new List<FileInfo>();
            FileInfo startFile = null;

            foreach (FileInfo file in info)
            {
                if (file.Name.StartsWith("sta"))
                    startFile = file;
                else
                    reordered.Add(file);
            }

            // Put the start node file at the beginning
            reordered.Insert(0, startFile);
            return reordered.ToArray();
        }

        public static List<Saveable> LoadSaveables()
        {
            List<Saveable> saveableList = new List<Saveable>();

            // Get all JSON files in the folder
            string path = BranchCamEditorPreferences.GetLastFilePath();
            DirectoryInfo directory = null;
            if (!string.IsNullOrEmpty(path))
            {
                directory = new DirectoryInfo(path);
            }
            if (directory != null && !directory.Exists)
            {
                Debug.LogError("Trying to read files but path does not exist! Resave the dialogue file and reassign it to the Collider");
                return saveableList;
            }

            FileInfo[] info = directory.GetFiles("*.json");

            // Reorder files to have the start node file first
            info = ReorderStartFirst(info);

            // Load each file based on its name prefix
            foreach (FileInfo file in info)
            {
                using (StreamReader reader = file.OpenText())
                {
                    string fileNamePrefix = file.Name.Substring(0, 3);
                    Saveable saveableNode;

                    switch (fileNamePrefix)
                    {
                        case "sta":
                            saveableNode = JsonUtility.FromJson<EditorStartNode.SaveableStartNode>(reader.ReadToEnd());
                            break;
                        case "dia":
                            saveableNode = JsonUtility.FromJson<EditorDialogueNode.SaveableDialogueNode>(reader.ReadToEnd());
                            break;
                        case "dec":
                            saveableNode = JsonUtility.FromJson<EditorDecisionNode.SaveableDecisionNode>(reader.ReadToEnd());
                            break;
                        case "act":
                            saveableNode = JsonUtility.FromJson<EditorActionNode.SaveableActionNode>(reader.ReadToEnd());
                            break;
                        case "got":
                            saveableNode = JsonUtility.FromJson<EditorGotoNode.SaveableGotoNode>(reader.ReadToEnd());
                            break;
                        default:
                            Debug.LogError("Invalid file prefix: " + fileNamePrefix);
                            continue;
                    }

                    saveableList.Add(saveableNode);
                }
            }

            return saveableList;
        }
    }
}
