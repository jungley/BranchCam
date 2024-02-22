using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RydenCam.BranchCamEditor.Serialization.Saveables
{
    [System.Serializable]
    public abstract class Saveable : INode
    {
        [SerializeField]
        private NodeType typeOfNode = NodeType.None;

        public NodeType TypeOfNode
        {
            get { return typeOfNode; }
        }

        [SerializeField]
        public string node_id;
        [SerializeField]
        public Rect windowRect;
        [SerializeField]
        public CameraGoal goal_type;
        [SerializeField]
        public CameraDistance goal_dist;
        [SerializeField]
        public CameraAngle goal_angle;
        [SerializeField]
        public CustomCameraType goal_customtype;
        [SerializeField]
        public List<string> OUT_connTo;
        [SerializeField]
        public List<string> IN_connTo;
        [SerializeField]
        public string oppositeActor;
        [SerializeField]
        public Vector3 CamPositon;
        [SerializeField]
        public Quaternion CamRotation;
        [SerializeField]
        public Vector3 LocalActorPos;
        [SerializeField]
        public Quaternion LocalActorRot;

        public Saveable(EditorBaseNode node)
        {
            node_id = node.node_id;
            windowRect = node.windowRect;
            OUT_connTo = new List<string>();
            IN_connTo = new List<string>();
            typeOfNode = node.TypeOfNode;
        }
    }
}
