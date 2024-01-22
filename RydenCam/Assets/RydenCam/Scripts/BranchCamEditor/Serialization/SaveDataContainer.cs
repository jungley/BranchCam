using Newtonsoft.Json;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;
using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static RydenCam.BranchCamEditor.Nodes.EditorActionNode;

namespace RydenCam.BranchCamEditor.Serialization
{
    //[CreateAssetMenu(fileName = "SaveData", menuName = "Create Save Data")]
    [System.Serializable]
    public class SaveDataContainer : ScriptableObject
    {
        [SerializeField]
        public List<Saveable> saveables;

        private void OnEnable()
        {
            Debug.Log("SaveDataContainer is being loaded or created.");
            Debug.Log($"Number of saveables: {saveables.Count}");
        }
    }

    [System.Serializable]
    public class SaveableDialogueNode : Saveable
    {
        public ConversationData NodeConvodata;
        public override NodeType TypeOfNode => NodeType.DialogueNode;

        public SaveableDialogueNode(EditorDialogueNode node) : base(node)
        {
            NodeConvodata = node.NodeConvodata;

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

    [System.Serializable]
    public class SaveableDecisionNode : Saveable
    {
        public ConversationData NodeConvodata;
        public List<string> DecisionOptions;
        public bool ShowPreviousDialog;

        public override NodeType TypeOfNode => NodeType.DecisionNode;

        public SaveableDecisionNode(EditorDecisionNode node) : base(node)
        {
            NodeConvodata = node.NodeConvodata;
            DecisionOptions = node.DecisionOptions;
            ShowPreviousDialog = node.ShowPreviousDialog;

            IN_connTo = new List<string>();
            if (node.PointIn.connectedTo != null)
            {
                IN_connTo.Add(node.PointIn.connectedTo.node.node_id);
            }

            //Loop Through out points
            OUT_connTo = new List<string>();
            for (int i = 0; i < DecisionOptions.Count; i++)
            {
                if (node.PointOut[i].connectedTo != null)
                {
                    OUT_connTo.Add(node.PointOut[i].connectedTo.node.node_id);
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

    [System.Serializable]
    public class SaveableGotoNode : Saveable
    {
        public override NodeType TypeOfNode => NodeType.GoToNode;
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

    [System.Serializable]
    public class SaveableActionNode : Saveable
    {
        [SerializeField]
        public List<MethodInfoContainer> methodInfoConatiners;

        public override NodeType TypeOfNode => NodeType.ActionNode;

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
