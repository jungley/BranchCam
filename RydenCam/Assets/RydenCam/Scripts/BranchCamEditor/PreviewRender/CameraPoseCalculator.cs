using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.BranchCam;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender
{
    public class CameraPoseCalculator
    {

        CameraCalculator cameraCalculator;


        public CameraPoseCalculator()
        {
            cameraCalculator = new CameraCalculator();
        }


        public Vector3 CalculateMidPreviewPoint()
        {
            return cameraCalculator.CalculateMidPoint();
        }


        public Pose CalculateCameraPose(CamShotConfig shotConfig, Transform actorTransform)
        {
            var actorPosition = actorTransform.position;

            var initialCamPose = new CameraCalculator().CalculatePlacement(shotConfig);

            // Calculate the relative position to the actor, ignoring y-axis differences.
            Vector3 relativePosition = new Vector3(actorPosition.x - initialCamPose.position.x, 0, actorPosition.z - initialCamPose.position.z);

            // Adjust the camera's position to maintain the initial y-position.
            Vector3 finalPosition = relativePosition + new Vector3(0, initialCamPose.position.y, 0);

            // Rotate the camera to face the actor.
            Quaternion finalRotation = Quaternion.Euler(initialCamPose.rotation.eulerAngles + new Vector3(0, 180f, 0));

            return new Pose(finalPosition, finalRotation);
        }

    }
}