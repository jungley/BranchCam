using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.SequenceData;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Controllers
{
    public class SequenceSetupManager
    {
        private CameraCalculator CameraCalculator { get; set; }

        public SequenceSetupManager(CameraCalculator camCalc)
        {
            CameraCalculator = camCalc;
        }

        public void ActorsLookAtEachOther()
        {
            Vector3 midPoint = CameraCalculator.CalculateMidPoint();
            foreach (ActorInfo actorInfo in NodeManager.Instance.ActorsInScene())
            {
                actorInfo.ActorGO.transform.root.LookAt(new Vector3(midPoint.x, actorInfo.ActorGO.transform.root.position.y, midPoint.z));
            }
        }

        public void SetPreDefinedActorPositions(StartNode startNode)
        {
            if (!startNode.StartPositionsEnabled) return;

            foreach (ActorInfo actorInfo in NodeManager.Instance.ActorsInScene())
            {
                if (startNode.ReturnToOriginalPositions)
                {
                    actorInfo.OriginalPositionAtStartOfDialogue = new Pose(actorInfo.ActorGO.transform.root.position, actorInfo.ActorGO.transform.root.rotation);
                }
                actorInfo.ActorGO.transform.root.position = actorInfo.PreDefinedStartPosition.position;

                if (!startNode.OverrideRotation)
                {
                    actorInfo.ActorGO.transform.root.rotation = actorInfo.PreDefinedStartPosition.rotation;
                }
            }
        }

        public void ReturnActorsToOriginalPositionsIfEnabled()
        {
            if (NodeManager.Instance.StartNode == null || !NodeManager.Instance.StartNode.ReturnToOriginalPositions) return;

            foreach (ActorInfo actorInfo in NodeManager.Instance.ActorsInScene())
            {
                actorInfo.ActorGO.transform.root.position = actorInfo.OriginalPositionAtStartOfDialogue.position;
            }
            ActorsLookAtEachOther();
        }

        //RS TODO Automatically setting the depth of field
        public void SetDepthOfField(bool enabled)
        {
            /*
            PostProcessVolume volume = cameraBrain.GetComponent<PostProcessVolume>();

            if (volume.profile.TryGetSettings(out DepthOfField depth))
            {
             depth.enabled.value = depthEnabled;
                if (depthEnabled)
                {
                    depth.focusDistance.value = 50.0f; // Calculate based on distance
                }
            }
            */
        }
    }
}
