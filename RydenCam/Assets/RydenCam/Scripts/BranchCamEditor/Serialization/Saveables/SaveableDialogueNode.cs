using RydenCam.BranchCamEditor.Nodes;
using RydenCam.BranchCamEditor.Serialization;
using RydenCam.Common;
using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RydenCam.BranchCamEditor.Serialization.Saveables
{
    [System.Serializable]
    public class SaveableDialogueNode : Saveable
    {
        [SerializeField]
        public ConversationData NodeConvodata;
        [SerializeField]
        private NodeType typeOfNode = NodeType.DialogueNode;

        [SerializeField]
        public new NodeType TypeOfNode
        {
            get { return typeOfNode; }
        }
        public SaveableDialogueNode(EditorDialogueNode node) : base(node)
        {
            NodeConvodata = node.NodeConvodata;

            IN_connTo = new List<string>();
            if (node.PointIn.ConnectedTo != null)
            {
                //IN_connTo.Add(node.PointIn.ConnectedTo.node.node_id);
            }

            if (node.PointOut[0].ConnectedTo != null)
            {
                OUT_connTo = new List<string>();
                //OUT_connTo.Add(node.PointOut[0].ConnectedTo.node.node_id);
            }

            //Saving Camera Info Here
            var cameraShot = NodeConvodata.ShotConfig;
            oppositeActor = cameraShot.oppositeActor;
            goal_type = cameraShot.GoalType;
            goal_dist = cameraShot.GoalDistance;
            goal_angle = cameraShot.GoalAngle;
            goal_customtype = cameraShot.GoalCustomType;
            CamPositon = (cameraShot.CustomCamPos != null) ? cameraShot.CustomCamPos.Value : Vector3.zero;
            CamRotation = (cameraShot.CustomCamRot != null) ? cameraShot.CustomCamRot.Value : Quaternion.identity;
            LocalActorPos = cameraShot.LocalRelativeActorPos;
            LocalActorRot = cameraShot.LocalRelativeActorRot;
        }
    }
}
