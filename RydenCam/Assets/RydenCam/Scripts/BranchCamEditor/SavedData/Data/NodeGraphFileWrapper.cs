using Assets.RydenCam.Scripts.BranchCamCC;
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
        public string CameraShotJsonPath;

        [SerializeField]
        public List<SerializedNode> JsonList = new List<SerializedNode>();

        public NodeGraphFileWrapper()
        {
            foreach (Node save in NodeManager.Instance.Nodes)
            {
                string jsonNode = JsonUtility.ToJson(save);
                JsonList.Add(new SerializedNode(save.TypeOfNode, jsonNode));
            }
        }
    }
}
