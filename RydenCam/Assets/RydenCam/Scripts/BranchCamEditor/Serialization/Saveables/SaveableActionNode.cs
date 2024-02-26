using RydenCam.BranchCamEditor.Nodes;
using RydenCam.BranchCamEditor.Serialization.Saveables;
using RydenCam.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static RydenCam.BranchCamEditor.Nodes.EditorActionNode;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Serialization.Saveables
{
    [System.Serializable]
    public class SaveableActionNode : Saveable
    {
        [SerializeField]
        public List<MethodInfoContainer> methodInfoConatiners;

        [SerializeField]
        private NodeType typeOfNode = NodeType.ActionNode;

        public new NodeType TypeOfNode
        {
            get { return typeOfNode; }
        }

        public SaveableActionNode(EditorActionNode node) : base(node)
        {
            methodInfoConatiners = node.methodContainers;

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
