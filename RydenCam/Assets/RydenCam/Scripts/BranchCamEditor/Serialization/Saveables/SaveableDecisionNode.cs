using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;
using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RydenCam.BranchCamEditor.Serialization.Saveables
{

    [System.Serializable]
    public class SaveableDecisionNode : Saveable
    {
        [SerializeField]
        public ConversationData NodeConvodata;
        [SerializeField]
        public List<string> DecisionOptions;
        [SerializeField]
        public bool ShowPreviousDialog;

        [SerializeField]
        private NodeType typeOfNode = NodeType.DecisionNode;
        [SerializeField]
        public new NodeType TypeOfNode
        {
            get { return typeOfNode; }
        }
        public SaveableDecisionNode(EditorDecisionNode node) : base(node)
        {
            NodeConvodata = node.NodeConvodata;
            DecisionOptions = node.DecisionOptions;
            ShowPreviousDialog = node.ShowPreviousDialog;

            IN_connTo = new List<string>();
            if (node.PointIn.ConnectedTo != null)
            {
                //IN_connTo.Add(node.PointIn.ConnectedTo.node.node_id);
            }

            //Loop Through out points
            OUT_connTo = new List<string>();
            for (int i = 0; i < DecisionOptions.Count; i++)
            {
                if (node.PointOut[i].ConnectedTo != null)
                {
                    //OUT_connTo.Add(node.PointOut[i].ConnectedTo.node.node_id);
                }
                else
                {
                    OUT_connTo.Add("blank");
                }
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
