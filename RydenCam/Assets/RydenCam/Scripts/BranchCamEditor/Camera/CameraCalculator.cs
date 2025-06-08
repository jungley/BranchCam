using Assets.RydenCam.Scripts.BranchCamEditor.Camera;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup;
using RydenCam.BranchCamEditor.Extensions;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RydenCam.BranchCamEditor.BranchCam
{
    public class CameraCalculator
    {

        public CameraSettings CamSettings { get; set; }

        //RS TODO Revist this: Remove dependency on GameObject
        List<Transform> ActorsInScene
        {
            get
            {
                return NodeManager.Instance.ActorsInScene
                   .Where(item => item.ActorGO != null)
                   .Select(item => item.ActorGO.transform)
                   .ToList();
            }
        }

        public CameraCalculator()
        {
            CamSettings = new CameraSettings();
        }


        public Pose CalculatePlacement(CamShotConfig shot, ActorPositionWrapper actorPositionData, bool calculateInGame = false)
        {
            switch (shot.GoalType)
            {
                case CameraGoal.Portrait:
                    return CalculatePortrait(shot, actorPositionData);

                case CameraGoal.OverShoulder:
                    return CalculateOverShoulder(shot, actorPositionData, calculateInGame); 

                case CameraGoal.FrameShare:
                    return CalculateFrameShare(shot, actorPositionData, calculateInGame);

                case CameraGoal.Custom:
                    return CalculateCustom(shot);

                default:
                    return new Pose();
             }
        }

        private Pose CalculateCustom(CamShotConfig shot)
        {
            if (!shot.IsCustomSet) return new Pose();

            return new Pose(shot.GlobalCustomCamPos, shot.GlobalCustomCamRot);
        }
        private Pose CalculatePortrait(CamShotConfig shot, ActorPositionWrapper posData)
        {
            Vector3 targetPos = posData.ActorPosition;
            Vector3 forward = posData.ForwardN;
            float distance = CamSettings.GetDistance(shot);
            float angleHeight = CamSettings.GetAngle(shot);
            float biasX = CamSettings.DefaultBiasX;
            float orbitAngle = CamSettings.DefaultOrbitAngle;

            // Initial elevated camera position in front of the target
            Vector3 camPos = targetPos + forward * distance;
            camPos.y += angleHeight;

            // Rotate direction vector around Y axis
            Vector3 Orbit(Vector3 center, Vector3 dir, float angleDeg)
            {
                float rad = angleDeg * Mathf.Deg2Rad;
                float cos = Mathf.Cos(rad);
                float sin = Mathf.Sin(rad);

                Vector3 dirNorm = (dir - center).normalized;
                return center + new Vector3(
                    cos * dirNorm.x - sin * dirNorm.z,
                    dirNorm.y,
                    sin * dirNorm.x + cos * dirNorm.z
                ) * (dir - center).magnitude;
            }

            Vector3 option1 = Orbit(targetPos, camPos, orbitAngle);
            Vector3 option2 = Orbit(targetPos, camPos, -orbitAngle);
            Vector3 chosenPos = SetSide(ActorsInScene.Select(x => x.position).ToList()).GetClosest(option1, option2);

            Quaternion camRot = Quaternion.LookRotation(targetPos - chosenPos);
            chosenPos += camRot * Vector3.right * biasX;

            return new Pose(chosenPos, camRot);
        } 
        

        public ActorPositionWrapper GetOppositeActor(CamShotConfig shot, bool calculateInGame)
        {
            if (string.IsNullOrEmpty(shot.OppositeActor))
                return null;

            if (calculateInGame)
                return new ActorPositionWrapper(shot.OppositeActor);

            return SetupPreviewSceneData.PreviewActorDatas?
                .FirstOrDefault(x => x.ActorPositionData.ActorName == shot.OppositeActor)
                ?.ActorPositionData;
        }


        private Pose CalculateOverShoulder(CamShotConfig shot, ActorPositionWrapper posData, bool calculateInGame = false)
        {
            var oppActor = GetOppositeActor(shot, calculateInGame);
            if (oppActor == null) return new Pose();

            float distance = CamSettings.GetDistance(shot);
            float height = CamSettings.GetAngle(shot);

            Vector3 simulatedForward = (posData.ActorPosition - oppActor.ActorPosition).normalized;
            Vector3 rightN = Vector3.Cross(simulatedForward, Vector3.up).normalized;

            Vector3 baseCamPos = posData.ActorPosition - posData.ForwardN * distance;
            Vector3 option1 = baseCamPos + rightN * distance;
            Vector3 option2 = baseCamPos - rightN * distance;

            Vector3 chosenPos = SetSide(ActorsInScene.Select(x => x.position).ToList())
                .GetClosest(option1, option2);

            chosenPos.y += height;

            Vector3 midpoint = (posData.ActorPosition + oppActor.ActorPosition) * 0.5f;
            Quaternion rotation = Quaternion.LookRotation(midpoint - chosenPos);

            return new Pose(chosenPos, rotation);
        }

        private Pose CalculateFrameShare(CamShotConfig shot, ActorPositionWrapper posData, bool calculateInGame = false)
        {
            ActorPositionWrapper oppActorData = GetOppositeActor(shot, calculateInGame);
            if (oppActorData == null) return new Pose();

            float actorDistance = Vector3.Distance(posData.ActorPosition, oppActorData.ActorPosition);
            Vector3 actorADirN = (posData.ActorPosition - oppActorData.ActorPosition).normalized;
            Vector3 MidPoint = oppActorData.ActorPosition + actorADirN * (actorDistance / 2);

            Vector3 PDir1 = Quaternion.AngleAxis(90, Vector3.up) * actorADirN;
            Vector3 PDir2 = Quaternion.AngleAxis(-90, Vector3.up) * actorADirN;
            Vector3 option1 = MidPoint + PDir1 * (actorDistance + CamSettings.GetDistance(shot));
            Vector3 option2 = MidPoint + PDir2 * (actorDistance + CamSettings.GetDistance(shot));

            Vector3 ChosenSideMarker = SetSide(ActorsInScene.Select(x => x.position).ToList());
            Vector3 camPos = ChosenSideMarker.GetClosest(option1, option2);
            float angleHeight = CamSettings.GetAngle(shot);
            camPos = new Vector3(camPos.x, camPos.y + angleHeight, camPos.z);
            Quaternion camRot = Quaternion.LookRotation(MidPoint - camPos);

            return new Pose(camPos, camRot);
        }

        /// <summary>
        /// Calculate the midpoint of a list of focus targets.
        /// </summary>
        /// <param name="focusTargets"></param>
        /// <returns></returns>
        public Vector3 CalculateMidPoint(List<Vector3> focusTargets)
        {
            Vector3 vecCounter = Vector3.zero;
            foreach (var focusTarget in focusTargets)
            {
                vecCounter.x += focusTarget.x;
                vecCounter.y += focusTarget.y;
                vecCounter.z += focusTarget.z;
            }

            return vecCounter / focusTargets.Count;
        }

        public Vector3 SetSide(List<Vector3> actorPositions)
        {

            Side camSide = NodeManager.Instance.StartNode.CameraSide;

            if (actorPositions.Count == 1)
            {
                return actorPositions[0];
            }
            else if (actorPositions.Count == 2)
            {
                Vector3 posA = actorPositions[0];
                Vector3 posB = actorPositions[1];

                Vector3 midpoint = (posA + posB) / 2;

                Vector3 direction = (posB - posA).normalized;

                Vector3 rightDir = Quaternion.AngleAxis(90, Vector3.up) * direction;
                Vector3 leftDir = Quaternion.AngleAxis(-90, Vector3.up) * direction;

                Vector3 markerRight = midpoint + (rightDir * 10);
                Vector3 markerLeft = midpoint + (leftDir * 10);

                markerRight.y = posA.y;
                markerLeft.y = posA.y;

                // Select the appropriate marker based on the camera side
                return (camSide == Side.Right) ? markerRight : markerLeft;
            }

            return Vector2.zero;
        }
    }
}

