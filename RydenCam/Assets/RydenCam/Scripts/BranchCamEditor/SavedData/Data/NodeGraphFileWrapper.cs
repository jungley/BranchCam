using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System.Collections.Generic;
using UnityEngine;

namespace RydenCam.BranchCamEditor.Serialization
{

    [System.Serializable]
    public class SerializedNode
    {
        public NodeType NodeType;
        public string JsonString;

        public SerializedNode(NodeType nodeType, string jsonString)
        {
            NodeType = nodeType;
            JsonString = jsonString;
        }
    }

    [System.Serializable]
    public class NodeGraphFileWrapper
    {
        [SerializeField]
        public string CameraShotJsonFilePath;

        [SerializeField]
        public List<SerializedNode> JsonList = new List<SerializedNode>();

        public NodeGraphFileWrapper()
        {

            //Get last fileoath OPENED
            string cameraShotPath = FilePathSaveManager.Instance.GetLastFilePathSaved(FilePathSaveManager.LastOpened_CameraShotsKey);

            CameraShotJsonFilePath = cameraShotPath;

            foreach (Node save in NodeManager.Instance.Nodes)
            {
                string jsonNode = JsonUtility.ToJson(save);
                JsonList.Add(new SerializedNode(save.TypeOfNode, jsonNode));
            }
        }
    }
}
