using RydenCam.BranchCamEditor.BranchCam;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender
{

    public class PreviewCameraWrapper
    {

        private CameraCalculator cameraCalculator;

        private List<PreviewActorData> actorDatas;


        public PreviewCameraWrapper(List<PreviewActorData> _actorDatas)
        {
            cameraCalculator = new CameraCalculator();
            actorDatas = _actorDatas;
        }


        public Vector3 CalculateMidPreviewPoint(List<Vector3> previewFocusPositions)
        {
            return cameraCalculator.CalculateMidPoint(previewFocusPositions);
        }


        public Pose CalculateCameraShot(CamShotConfig shotConfig)
        {
            PreviewActorData actorData = actorDatas.Where(actorData => shotConfig.actor == actorData.ActorPositionData.ActorName).FirstOrDefault();  

            return cameraCalculator.CalculatePlacement(shotConfig, actorData.ActorPositionData); 

     

            //Im not sure why we have this here below
            /*

            // Calculate the relative position to the actor, ignoring y-axis differences.
            Vector3 relativePosition = new Vector3(actorFocusObjPosition.x - initialCamPose.position.x, 0, actorFocusObjPosition.z - initialCamPose.position.z);

            // Adjust the camera's position to maintain the initial y-position.
            Vector3 finalPosition = relativePosition + new Vector3(0, initialCamPose.position.y, 0);

            // Rotate the camera to face the actor.
            Quaternion finalRotation = Quaternion.Euler(initialCamPose.rotation.eulerAngles + new Vector3(0, 180f, 0));
            

            return new Pose(finalPosition, finalRotation);
            */
        }

    }
}