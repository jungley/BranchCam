using UnityEngine;
using RydenCam.Common;
using RydenCam.BranchCamEditor.Extensions;
using System;

namespace RydenCam.BranchCamEditor.BranchCam
{
    [ExecuteAlways]
    [System.Serializable]
    public class CamShotConfig
    {
        public string ShotName;
        public string ShotId;


        public CameraGoal GoalType;
        public CameraDistance GoalDistance;
        public CameraAngle GoalAngle;

        public string OppositeActor;
        public string Actor;

        /// <summary>
        /// True if the custom camera position and rotation are set.
        /// </summary>
        public bool IsCustomSet;
        public bool TogglePreviewRenderSceneView;
        public Vector3 GlobalCustomCamPos;
        public Quaternion GlobalCustomCamRot;


        //Default Constructor
        public CamShotConfig(string shotName = "", string targetGameObjectName = "")
        {
            ShotName = shotName;
            ShotId = Guid.NewGuid().ToString(); 

            Actor = targetGameObjectName;
            GoalType = CameraGoal.Portrait;
            GoalDistance = CameraDistance.Mid;
            GoalAngle = CameraAngle.EyeLevel;
            
            GlobalCustomCamPos = Vector3.zero;
            GlobalCustomCamRot = Quaternion.identity;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CamShotConfig))
                return false;

            CamShotConfig other = (CamShotConfig)obj;

            return GoalType == other.GoalType &&
                   GoalDistance == other.GoalDistance &&
                   GoalAngle == other.GoalAngle &&
                   OppositeActor == other.OppositeActor &&
                   Actor == other.Actor &&
                   GlobalCustomCamPos.IsEqual(other.GlobalCustomCamPos);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(GoalType, GoalDistance, GoalAngle, OppositeActor, Actor, GlobalCustomCamPos);
        }
    }
}
