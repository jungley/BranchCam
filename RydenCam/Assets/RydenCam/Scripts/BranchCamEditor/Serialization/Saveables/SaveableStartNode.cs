using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.BranchCamEditor.Serialization;
using RydenCam.BranchCamEditor.Serialization.Saveables;
using RydenCam.Common;
using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RydenCam.BranchCamEditor.Serialization
{
    [System.Serializable]
    public class SaveableStartNode : Saveable
    {
        [SerializeField]
        public string SequenceName;
        [SerializeField]
        public Side CameraSide;
        [SerializeField]
        public List<ActorInfo> ActorsInScene;
        [SerializeField]
        public bool startPositionsEnabled;
        [SerializeField]
        public bool overrideRotation;
        [SerializeField]
        public bool returnToOriginalPositions;
        [SerializeField]
        public string unitySceneName;

        [SerializeField]
        private NodeType typeOfNode = NodeType.StartNode;

        public new NodeType TypeOfNode
        {
            get { return typeOfNode; }
        }

        public SaveableStartNode(EditorStartNode node) : base(node)
        {
            SequenceName = node.SequenceName;
            CameraSide = node.CameraSide;
            ActorsInScene = EditorController.Instance.ActorsInScene;

            //predefind start variables
            unitySceneName = node.UnitySceneName;
            overrideRotation = node.OverrideRotation;
            startPositionsEnabled = node.StartPositionsEnabled;
            returnToOriginalPositions = node.ReturnToOriginalPositions;

            if (node.PointOut[0].ConnectedTo != null)
            {
                OUT_connTo = new List<string>();
                //OUT_connTo.Add(node.PointOut[0].ConnectedTo.node.node_id);
            }
        }
    }
}
