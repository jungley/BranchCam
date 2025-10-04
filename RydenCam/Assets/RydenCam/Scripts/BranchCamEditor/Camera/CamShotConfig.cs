using UnityEngine;
using RydenCam.Common;
using RydenCam.BranchCamEditor.Extensions;
using System;
using RydenCam.SequenceData;

namespace RydenCam.BranchCamEditor.BranchCam
{
    [ExecuteAlways]
    [System.Serializable]
    public class CamShotConfig
    {
        public string ShotName;
        public string ShotId;

        public bool IsDefault;

        public CameraGoal GoalType;
        public CameraDistance GoalDistance;
        public CameraAngle GoalAngle;

        /// <summary>
        /// True if the custom camera position and rotation are set.
        /// </summary>
        public bool IsCustomSet;
        public bool TogglePreviewRenderSceneView;
        public Vector3 GlobalCustomCamPos;
        public Quaternion GlobalCustomCamRot;


        //Default Constructor
        public CamShotConfig(string shotName)
        {
            ShotName = shotName;
            ShotId = Guid.NewGuid().ToString();

            IsDefault = false;

            GoalType = CameraGoal.Portrait;
            GoalDistance = CameraDistance.Mid;
            GoalAngle = CameraAngle.EyeLevel;
            
            GlobalCustomCamPos = Vector3.zero;
            GlobalCustomCamRot = Quaternion.identity;
        }

        public override bool Equals(object obj)
        {
            if (obj is CamShotConfig other)
                return ShotId == other.ShotId;
            return false;
        }

        public override int GetHashCode()
        {
            return ShotId != null ? ShotId.GetHashCode() : 0;
        }
    }
}
