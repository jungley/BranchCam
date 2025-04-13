using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.Common;
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
            PreviewActorData actorData = actorDatas
                .Where(actorData => shotConfig.Actor == actorData.ActorPositionData.ActorName)
                .FirstOrDefault();

            if (actorData == null) return new Pose();
            
            return cameraCalculator.CalculatePlacement(shotConfig, actorData.ActorPositionData);
            
        }

    }
}