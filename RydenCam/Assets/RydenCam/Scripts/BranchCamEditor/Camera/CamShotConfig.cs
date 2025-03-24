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
        public CameraGoal GoalType;
        public CameraDistance GoalDistance;
        public CameraAngle GoalAngle;
        public CustomCameraType GoalCustomType;

        public string oppositeActor;
        public string actor;

        public bool IsCustomSet;

        public Vector3 GlobalCustomCamPos;
        public Quaternion GlobalCustomCamRot;
        public Vector3 LocalRelativeActorPos;
        public Quaternion LocalRelativeActorRot;

        //RS TODO
        //For Custom Shots
        /*
        public CamShotConfig(string a, CustomCameraType customtype, Vector3 pos, Quaternion rot)
        {
            actor = a;
            GoalType = CameraGoal.Custom;
            GoalCustomType = customtype;
            GlobalCustomCamPos = pos;
            GlobalCustomCamRot = rot;
        }
        */

        //Default Constructor
        public CamShotConfig(string targetGameObjectName = "")
        {
            actor = targetGameObjectName;
            GoalType = CameraGoal.Portrait;
            GoalDistance = CameraDistance.Mid;
            GoalAngle = CameraAngle.EyeLevel;
            GoalCustomType = CustomCameraType.None;
            
            GlobalCustomCamPos = Vector3.zero;
            GlobalCustomCamRot = Quaternion.identity;
            LocalRelativeActorPos = Vector3.zero;
            LocalRelativeActorRot = Quaternion.identity;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CamShotConfig))
                return false;

            CamShotConfig other = (CamShotConfig)obj;

                return GoalType == other.GoalType &&
                       GoalDistance == other.GoalDistance &&
                       GoalAngle == other.GoalAngle &&
                       GoalCustomType == other.GoalCustomType &&
                       oppositeActor == other.oppositeActor &&
                       actor == other.actor &&
                       GlobalCustomCamPos.IsEqual(other.GlobalCustomCamPos) &&
                       LocalRelativeActorPos.IsEqual(other.LocalRelativeActorPos);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(GoalType, GoalDistance, GoalAngle, GoalCustomType, oppositeActor, actor, GlobalCustomCamPos, LocalRelativeActorPos);
        }
    }
}
