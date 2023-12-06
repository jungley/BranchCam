using System;
using UnityEngine;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;

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

        public Vector3? CustomCamPos;
        public Quaternion? CustomCamRot;
        public Vector3 LocalRelativeActorPos;
        public Quaternion LocalRelativeActorRot;

        //For Custom Shots
        public CamShotConfig(string a, CustomCameraType customtype, Vector3 pos, Quaternion rot)
        {
            actor = a;
            GoalType = CameraGoal.Custom;
            GoalCustomType = customtype;
            CustomCamPos = pos;
            CustomCamRot = rot;
        }

        public CamShotConfig(string a, CameraGoal goal_t, CameraDistance goal_d, CameraAngle goal_a, CustomCameraType customtype)
        {
            actor = a;
            GoalType = goal_t;
            GoalDistance = goal_d;
            GoalAngle = goal_a;
            GoalCustomType = customtype;
        }
    }
}
