using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;
using System.Collections.Generic;
using UnityEngine;

namespace RydenCam.BranchCamEditor.Serialization.Saveables
{
    [System.Serializable]
    public class SaveableGotoNode : Saveable
    {
        [SerializeField]
        private NodeType typeOfNode = NodeType.GoToNode;
        [SerializeField]
        public new NodeType TypeOfNode
        {
            get { return typeOfNode; }
        }
        public SaveableGotoNode(EditorGotoNode node) : base(node)
        {
            IN_connTo = new List<string>();
            if (node.PointIn.connectedTo != null)
            {
                IN_connTo.Add(node.PointIn.connectedTo.node.node_id);
            }

            if (node.PointOut[0].connectedTo != null)
            {
                OUT_connTo = new List<string>();
                OUT_connTo.Add(node.PointOut[0].connectedTo.node.node_id);
            }
        }
    }
}
