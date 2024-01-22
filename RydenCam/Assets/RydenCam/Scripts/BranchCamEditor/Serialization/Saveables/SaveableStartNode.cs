using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.BranchCamEditor.Serialization;
using RydenCam.Common;
using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Serialization.Saveables
{
    [System.Serializable]
    public class SaveableStartNode : Saveable
    {
        public string SequenceName;
        public Side CameraSide;
        public List<ActorInfo> ActorsInScene;
        public bool startPositionsEnabled;
        public bool overrideRotation;
        public bool returnToOriginalPositions;
        public string unitySceneName;

        public override NodeType TypeOfNode => NodeType.StartNode;

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

            if (node.PointOut[0].connectedTo != null)
            {
                OUT_connTo = new List<string>();
                OUT_connTo.Add(node.PointOut[0].connectedTo.node.node_id);
            }
        }
    }
}
