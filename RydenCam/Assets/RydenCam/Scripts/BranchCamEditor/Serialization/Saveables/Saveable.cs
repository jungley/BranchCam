using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RydenCam.BranchCamEditor.Serialization
{
    [System.Serializable]
    public abstract class Saveable : INode
    {
        public virtual NodeType TypeOfNode => NodeType.None;

        public string node_id;
        public Rect windowRect;
        public CameraGoal goal_type;
        public CameraDistance goal_dist;
        public CameraAngle goal_angle;
        public CustomCameraType goal_customtype;
        public List<string> OUT_connTo;
        public List<string> IN_connTo;
        public string oppositeActor;
        public Vector3 CamPositon;
        public Quaternion CamRotation;
        public Vector3 LocalActorPos;
        public Quaternion LocalActorRot;

        public Saveable(EditorBaseNode node)
        {
            node_id = node.node_id;
            windowRect = node.windowRect;
            OUT_connTo = new List<string>();
            IN_connTo = new List<string>();
        }
    }
}
