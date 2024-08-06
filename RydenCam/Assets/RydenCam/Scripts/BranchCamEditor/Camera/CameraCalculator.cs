using Cinemachine.Utility;
using Ink.Parsed;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Extensions;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using RydenCam.SequenceData;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RydenCam.BranchCamEditor.BranchCam
{
    public class CameraCalculator
    {

        public CameraSettings CamSettings { get; set; }
        List<Transform> ActorsInScene
        {
            get
            {
                return EditorController.Instance.ActorsInScene
                   .Where(item => item.ActorGO != null)
                   .Select(item => item.ActorGO.transform)
                   .ToList();
            }
        }

        public CameraCalculator()
        {
            CamSettings = new CameraSettings();
        }


        public Pose CalculatePlacement(CamShotConfig shot)
        {

            switch (shot.GoalType)
            {
                case CameraGoal.Portrait:
                    return CalculatePortrait(shot);

                case CameraGoal.OverShoulder:
                    return CalculateOverShoulder(shot);

                case CameraGoal.FrameShare:
                    return CalculateFrameShare(shot); 

                case CameraGoal.Custom:
                    return CalculateCustom(shot);

                default:
                    return new Pose();
             }
        }

        //TODO HERE fix global and local
        private Pose CalculateCustom(CamShotConfig shot)
        {
            if (shot.GoalCustomType == CustomCameraType.Local)
            {
                GameObject target = GameObject.Find(shot.actor);
                Vector3 pos_result = target.transform.position - shot.LocalRelativeActorPos;
                Vector3 localCamPos = (shot.CustomCamPos.Value + pos_result);
                return new Pose(localCamPos, shot.CustomCamRot.Value);
            }

            if(shot.GoalCustomType == CustomCameraType.Global)
            {
                return new Pose(shot.CustomCamPos.Value, shot.CustomCamRot.Value);
            }

            return new Pose();
        }


        private Pose CalculatePortrait(CamShotConfig shot)
        {
            // Retrieve shot parameters
            Vector3 targPos = GetPositionFromShot(shot.actor);
            Vector3 forwardN = GetForwardDirectionFromShot(shot.actor);
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

        // Placeholder functions to simulate shot parameters retrieval
        private Vector3 GetPositionFromShot(string actorName)
        {
            GameObject targetObj = GameObject.Find(actorName);
            return targetObj.transform.position;    

            // Replace with actual logic to get actor position
            //TODO
            //return Vector3.zero;
        }

        private Vector3 GetForwardDirectionFromShot(string actorName)
        {
            GameObject targetObj = GameObject.Find(actorName);
            Vector3 forwardN = targetObj.transform.forward.normalized;
            return forwardN;

            // Replace with actual logic to get actor forward direction
            //TODO
            //return Vector3.forward;
        }


        private Pose CalculateOverShoulder(CamShotConfig shot)
        {
            GameObject targetObj_Opp = GameObject.Find(shot.oppositeActor);
            GameObject targetObj_Act = GameObject.Find(shot.actor);

            if (targetObj_Act == null || targetObj_Opp == null)
            {
                Debug.LogError("CANNOT FIND ACTORS in CameraShot Constructor");
                return new Pose();
            }

            Vector3 targPos = targetObj_Opp.transform.position;
            Vector3 camPos = targPos;
            Vector3 forwardN = targetObj_Opp.transform.forward.normalized;
            float angleHeight = CamSettings.GetAngle(shot);
            camPos -= forwardN * CamSettings.GetDistance(shot);

            //float distBetweenActors = Vector3.Distance(targetObj_Act.transform.position, targetObj_Opp.transform.position);
            Vector3 rightN = targetObj_Opp.transform.right.normalized;
            Vector3 option1 = camPos + rightN * CamSettings.GetDistance(shot);
            Vector3 option2 = camPos - rightN * CamSettings.GetDistance(shot);
            Vector3 ChosenSideMarker = SetSide(ActorsInScene.Select(x => x.position).ToList());
            camPos = ChosenSideMarker.GetClosest(option1, option2);

            camPos = new Vector3(camPos.x, camPos.y + angleHeight, camPos.z);

            Vector3 midpoint = (targetObj_Act.transform.position + targetObj_Opp.transform.position) / 2;
            Quaternion camRot = Quaternion.LookRotation(midpoint - camPos);

            return new Pose(camPos, camRot);
        }

        private Pose CalculateFrameShare(CamShotConfig shot)
        {
            GameObject targetObj_Act = GameObject.Find(shot.actor);
            GameObject targetObj_Opp = GameObject.Find(shot.oppositeActor);

            if (targetObj_Act == null || targetObj_Opp == null)
            {
                Debug.LogError("An actor is not associated");
                return new Pose();
            }

            float actorDistance = Vector3.Distance(targetObj_Act.transform.position, targetObj_Opp.transform.position);
            Vector3 actorADirN = (targetObj_Act.transform.position - targetObj_Opp.transform.position).normalized;
            Vector3 MidPoint = targetObj_Opp.transform.position + actorADirN * (actorDistance / 2);

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


        public Vector3 CalculateMidPoint()
        {
            Vector3 vecCounter = Vector3.zero;
            foreach (var actor in ActorsInScene)
            {
                vecCounter.x += actor.position.x;
                vecCounter.y += actor.position.y;
                vecCounter.z += actor.position.z;
            }

            return vecCounter / ActorsInScene.Count;
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

