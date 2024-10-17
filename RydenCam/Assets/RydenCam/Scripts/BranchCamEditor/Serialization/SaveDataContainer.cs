using RydenCam.Common;
using System.Collections.Generic;
using UnityEngine;

namespace RydenCam.BranchCamEditor.Serialization
{

    [System.Serializable]
    public class NodeData
    {
        public NodeType NodeType;
        public string JsonString;

        public NodeData(NodeType nodeType, string jsonString)
        {
            NodeType = nodeType;
            JsonString = jsonString;
        }
    }

    [System.Serializable]
    public class SaveDataContainer
    {
        [SerializeField]
        public List<NodeData> JsonList = new List<NodeData>();

        public SaveDataContainer(List<NodeData> jsonList)
        {
            JsonList = jsonList;
        }
    }
}
