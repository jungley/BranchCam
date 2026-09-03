using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Camera;
using Cinemachine;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.SequenceData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Controllers
{

    /// <summary>
    /// Responsible for setting the actor positions and the camera placement in the scene.
    /// </summary>
    public class DirectorManager
    {
        private CameraCalculator CameraCalculator { get; set; }

        private CinemachineVirtualCamera dialogueCamera { get; set; }

        private Vector3 dialogueMidpoint { get; set; }

        public DirectorManager(CameraCalculator camCalc, CinemachineVirtualCamera camera)
        {
            CameraCalculator = camCalc;
            dialogueCamera = camera;


        }

        public void SetUpScene()
        {
            if(NodeManager.Instance.StartNode.StartPositionsEnabled)
            {
                SetPreDefinedActorPositions();
            }

            dialogueMidpoint = GetMidPoint();
        }


        public void SetCameraAndActorRotations(Node CurrentNode)
        {
            ActorsLookAtEachOther(CurrentNode as ITalkable);
            SetCamera(CurrentNode, dialogueCamera);
        }

        private void SetCamera(Node CurrentNode, CinemachineVirtualCamera dialogueCamera)
        {
            ITalkable posNode = CurrentNode as ITalkable;
            if (posNode?.NodeConvodata?.ShotConfig == null || dialogueCamera == null) return;

            GameObject actor = posNode.NodeConvodata.Actor?.ActorGO;
            if (actor == null)
            {
                Debug.LogWarning("[RydenCam] Cannot place the dialogue camera because the speaking actor was not found in the scene.");
                return;
            }

            ActorPositionData actorPosition = CreatePositionData(actor);
            GameObject oppositeActor = posNode.NodeConvodata.OppositeActor?.ActorGO;
            ActorPositionData oppositePosition = oppositeActor == null ? null : CreatePositionData(oppositeActor);
            Pose placement = CameraCalculator.CalculatePlacement(
                posNode.NodeConvodata.ShotConfig,
                actorPosition,
                oppositePosition);
            dialogueCamera.transform.SetPositionAndRotation(placement.position, placement.rotation);
        }

        private static ActorPositionData CreatePositionData(GameObject actor)
        {
            Transform actorTransform = actor.transform;
            return new ActorPositionData
            {
                ActorPosition = actorTransform.position,
                ActorRotation = actorTransform.rotation,
                ForwardN = actorTransform.forward
            };
        }

        /// <summary>
        /// Makes the actor look at the appropriate target depending on the number of actors in the scene.
        /// </summary>
        private void ActorsLookAtEachOther(ITalkable node)
        {
            if (node?.NodeConvodata?.Actor?.ActorGO == null) return;

            int actorCount = NodeManager.Instance.ActorsInScene.Count;
            if (actorCount <= 1) return;

            Transform actorTransform = node.NodeConvodata.Actor.ActorGO.transform.root;

            Vector3 lookTarget = new Vector3(dialogueMidpoint.x, actorTransform.position.y, dialogueMidpoint.z);
            actorTransform.LookAt(lookTarget);


            //RS TODO  - In KOTOR, the actors look at each other by turning their head if in a group
            //This actually affects camera positioning as well

            /*
            if (node.NodeConvodata.ShotConfig.GoalType == CameraGoal.Portrait || actorCount == 2)
            {
                Vector3 lookTarget1 = new Vector3(dialogueMidpoint.x, actorTransform.position.y, dialogueMidpoint.z);
                actorTransform.LookAt(lookTarget1);
            }
            else
            {                
                string oppositeActorName = node.NodeConvodata.ShotConfig.OppositeActor;
                                                //cameracalculator.GetOppositeActor...
                Transform otherActorTransform = GameObject.Find(oppositeActorName).transform.root;
                actorTransform.LookAt(otherActorTransform);
                otherActorTransform.LookAt(actorTransform);
                
            }
            */
            
        }


        private Vector3 GetMidPoint(List<Vector3> focusTargets = null)
        {
            var actors = NodeManager.Instance.ActorsInScene;
            if (actors == null || actors.Count == 0) return Vector3.zero;

            focusTargets = actors
                .Where(x => x?.ActorGO != null)
                .Select(x => x.ActorGO.transform.root.position)
                .ToList();
            if (focusTargets.Count == 0) return Vector3.zero;

            Vector3 midPoint = CameraCalculator.CalculateMidPoint(focusTargets);

            return midPoint;
        }

        public void SetPreDefinedActorPositions()
        {
            /*
            if (!startNode.StartPositionsEnabled) return;

            foreach (ActorInfo actorInfo in NodeManager.Instance.ActorsInScene)
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
            */
        }

        public void ReturnActorsToOriginalPositionsIfEnabled()
        {
            if (NodeManager.Instance.StartNode == null || !NodeManager.Instance.StartNode.ReturnToOriginalPositions) return;

            foreach (ActorInfo actor in NodeManager.Instance.StartNode.ActorsInScene)
            {
                if (actor?.ActorGO == null) continue;
                actor.ActorGO.transform.root.position = actor.OriginalPositionAtStartOfDialogue.position;
            }

            //ActorsLookAtMidPoint();
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
