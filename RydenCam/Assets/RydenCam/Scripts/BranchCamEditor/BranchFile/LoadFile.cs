using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;
using RydenCam.BranchCamEditor.Managers;

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

        //Load Saveables into NodeManager
        public static void LoadSaveables()
        {
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
                return;
            }

            FileInfo[] info = directory.GetFiles("*.json");

            // Reorder files to have the start node file first
            info = ReorderStartFirst(info);

            List<Saveable> saveableList = new List<Saveable>();
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

            //Convert Saveables to Nodes
            NodeManager.Instance.Clear();

            foreach (Saveable saveNode in saveableList)
            {
                EditorBaseNode node = saveNode.ConvertToUnity();
                NodeManager.Instance.AddNode(node);
            }

            for (int i = 0; i < saveableList.Count; i++)
            {
                //Associate Connections
                EditorBaseNode node = NodeManager.Instance.GetNode(i);
                Saveable savenode = saveableList[i];
                //Check out Connection
                if (savenode.OUT_connTo.Count != 0)
                {
                    for (int y = 0; y < savenode.OUT_connTo.Count; y++)
                    {
                        EditorBaseNode node_OUT = NodeManager.Instance.FindNode(savenode.OUT_connTo[y]);
                        if (node_OUT != null)
                        {
                            node.PointOut[y].connectedTo = node_OUT.PointIn;
                            node_OUT.PointIn.connectedTo = node.PointOut[y];
                            ConnectionManager.Instance.AddConnection(node.PointOut[y], node_OUT.PointIn, EditorBaseNode.OnClickRemoveConnection);
                        }
                    }
                }
            }
        }
        
    }
}
