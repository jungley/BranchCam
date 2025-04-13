using Assets.RydenCam.Scripts.BranchCamEditor.Camera;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender;
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


        public Pose CalculatePlacement(CamShotConfig shot, PreviewActorPositionData actorPositionData, bool calculateInGame = false)
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

        //RS TODO HERE fix global and local
        private Pose CalculateCustom(CamShotConfig shot)
        {
            if (!shot.IsCustomSet) return new Pose();

            if (shot.GoalCustomType == CustomCameraType.Local)
            {
                GameObject target = GameObject.Find(shot.Actor);
                Vector3 pos_result = target.transform.position - shot.LocalRelativeActorPos;
                Vector3 localCamPos = (shot.GlobalCustomCamPos + pos_result);
                return new Pose(localCamPos, shot.GlobalCustomCamRot);
            }

            if(shot.GoalCustomType == CustomCameraType.Global)
            {
                return new Pose(shot.GlobalCustomCamPos, shot.GlobalCustomCamRot);
            }

            return new Pose();
        }


        // Helper function to calculate orbit position around a target
        private Vector3 CalculateOrbitPosition(Vector3 targetPos, Vector3 camPos, float angle)
        {
            Vector3 direction = camPos - targetPos;
            float distance = direction.magnitude;
            direction.Normalize();

            // Rotation matrix for Y-axis rotation
            float cosAngle = Mathf.Cos(angle * Mathf.Deg2Rad);
            float sinAngle = Mathf.Sin(angle * Mathf.Deg2Rad);

            // Rotate the direction vector
            Vector3 rotatedDirection = new Vector3(
                cosAngle * direction.x - sinAngle * direction.z,
                direction.y,
                sinAngle * direction.x + cosAngle * direction.z
            );

            return targetPos + rotatedDirection * distance;
        }


        private Pose CalculatePortrait(CamShotConfig shot, PreviewActorPositionData posData)
        {
            // Retrieve shot parameters
            Vector3 targPos = posData.ActorPosition;
            Vector3 forwardN = posData.ForwardN;
            float distance = CamSettings.GetDistance(shot);
            float angleHeight = CamSettings.GetAngle(shot);
            float biasX = CamSettings.DefaultBiasX;
            float orbitAngle = CamSettings.DefaultOrbitAngle;

            // Calculate initial camera position
            Vector3 camPos = targPos + forwardN * distance;
            camPos.y += angleHeight;

            // Compute two possible camera positions based on orbit angle
            Vector3 option1 = CalculateOrbitPosition(targPos, camPos, orbitAngle);
            Vector3 option2 = CalculateOrbitPosition(targPos, camPos, -orbitAngle);

            Vector3 ChosenSideMarker = SetSide(ActorsInScene.Select(x => x.position).ToList());
            camPos = ChosenSideMarker.GetClosest(option1, option2);

            // Calculate camera rotation
            Quaternion camRot = Quaternion.LookRotation(targPos - camPos);

            // Apply bias
            camPos += camRot * Vector3.right * biasX;

            return new Pose(camPos, camRot);
        }

        public PreviewActorPositionData AssignOppositeActor(CamShotConfig shot, bool calculateInGame)
        {
            PreviewActorPositionData oppPreviewActorPosData = null;
            if (shot.OppositeActor != string.Empty)
            {

                if (calculateInGame)
                {
                    oppPreviewActorPosData = new PreviewActorPositionData(shot.OppositeActor);             
                }
                else
                {
                    oppPreviewActorPosData = SetupPreviewSceneData.PreviewActorDatas
                        .FirstOrDefault(x => x.ActorPositionData.ActorName == shot.OppositeActor)
                        ?.ActorPositionData;
                }
            }
            return oppPreviewActorPosData;
        }


        private Pose CalculateOverShoulder(CamShotConfig shot, PreviewActorPositionData posData, bool calculateInGame = false)
        {
            PreviewActorPositionData oppActorData = AssignOppositeActor(shot, calculateInGame);
            if (oppActorData == null) return new Pose();

            Vector3 targPos = oppActorData.ActorPosition;
            Vector3 camPos = posData.ActorPosition;
            float angleHeight = CamSettings.GetAngle(shot);
            camPos -= posData.ForwardN * CamSettings.GetDistance(shot);

            Vector3 rightN = Vector3.Cross(oppActorData.ForwardN, Vector3.up).normalized; 
            Vector3 option1 = camPos + rightN * CamSettings.GetDistance(shot);
            Vector3 option2 = camPos - rightN * CamSettings.GetDistance(shot);
            
            
            Vector3 ChosenSideMarker = SetSide(ActorsInScene.Select(x => x.position).ToList());
            camPos = ChosenSideMarker.GetClosest(option1, option2);

            camPos = new Vector3(camPos.x, camPos.y + angleHeight, camPos.z);

            Vector3 midpoint = (posData.ActorPosition + oppActorData.ActorPosition) / 2;
            Quaternion camRot = Quaternion.LookRotation(midpoint - camPos);

            return new Pose(camPos, camRot);
        }

        private Pose CalculateFrameShare(CamShotConfig shot, PreviewActorPositionData posData, bool calculateInGame = false)
        {
            PreviewActorPositionData oppActorData = AssignOppositeActor(shot, calculateInGame);
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





        private void DestroyTempCamCalcObject(GameObject cam)
        {
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(cam);
            else
                UnityEngine.Object.DestroyImmediate(cam);
        }
    }
}

