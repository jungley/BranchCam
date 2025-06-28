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
    public class SerializedNodeList
    {
        [SerializeField]
        public List<SerializedNode> JsonList = new List<SerializedNode>();

        public SerializedNodeList(List<SerializedNode> jsonList)
        {
            JsonList = jsonList;
        }
    }
}
