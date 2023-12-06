using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Extensions;
using RydenCam.Common;
using RydenCam.SequenceData;

using UnityEngine;

namespace RydenCam.BranchCamEditor.BranchCam
{
    public class CameraCalculator
    {
        public Side CameraSide;
        public Vector3 ChosenSideMarker;
        public Vector3 markerRight;
        public Vector3 markerLeft;
        public Vector3 MidPoint;

        public CameraSettings CamSettings { get; set; }
        public SequenceController Controller { get; set; }

        public CameraCalculator(SequenceController controller)
        {
            CamSettings = new CameraSettings();
            Controller = controller;
        }


        //TODO Find out WHY weird if statement thing has to be called before this.
        //Probably for height calculations
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
            GameObject targetObj = GameObject.Find(shot.actor);

            if (targetObj == null)
            {
                Debug.Log("CANNOT FIND ACTOR in CameraShot Constructor");
                return new Pose();
            }

            Vector3 targPos = targetObj.transform.position;
            Vector3 camPos = targPos;
            Vector3 forwardN = targetObj.transform.forward.normalized;
            float distance = CamSettings.GetDistance(shot);
            float angleHeight = CamSettings.GetAngle(shot);
            float biasX = CamSettings.DefaultBiasX;
            camPos += forwardN * distance;
            camPos = new Vector3(camPos.x, camPos.y + angleHeight, camPos.z);

            GameObject cam = new GameObject();
            cam.transform.position = camPos;

            cam.transform.RotateAround(targPos, Vector3.up, CamSettings.DefaultOrbitAngle);
            Vector3 option1 = cam.transform.position;
            cam.transform.position = camPos;
            cam.transform.RotateAround(targPos, Vector3.up, -CamSettings.DefaultOrbitAngle);
            Vector3 option2 = cam.transform.position;

            //sidemarker here
            camPos = ChosenSideMarker.GetClosest(option1, option2);
            Quaternion camRot = Quaternion.LookRotation(targPos - camPos);

            //Offset Calculation
            cam.transform.position = camPos;
            cam.transform.rotation = camRot;
            cam.transform.position += cam.transform.right * CamSettings.DefaultBiasX;
            camPos = cam.transform.position;
            camRot = cam.transform.rotation;
            DestroyTempCamCalcObject(cam);
            return new Pose(camPos, camRot);
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

            Vector3 camPos = ChosenSideMarker.GetClosest(option1, option2);
            float angleHeight = CamSettings.GetAngle(shot);
            camPos = new Vector3(camPos.x, camPos.y + angleHeight, camPos.z);
            Quaternion camRot = Quaternion.LookRotation(MidPoint - camPos);

            return new Pose(camPos, camRot);
        }


        public Vector3 CalculateMidPoint()
        {
            Vector3 vecCounter = Vector3.zero;
            foreach (ActorInfo actor in Controller.ActorsInScene)
            {
                vecCounter.x += actor.ActorGO.transform.position.x;
                vecCounter.y += actor.ActorGO.transform.position.y;
                vecCounter.z += actor.ActorGO.transform.position.z;
            }

            return vecCounter / Controller.ActorsInScene.Count;
        }


        //Sets the sides marker objects for camerashot calculation
        //Knows which side to orient on
        public void SetSide(Side camSide)
        {
            CameraSide = camSide;

            if (Controller.ActorsInScene.Count == 1)
            {
                markerLeft = markerRight = ChosenSideMarker = Controller.ActorsInScene[0].ActorGO.transform.position;
            }

            if (Controller.ActorsInScene.Count >= 2)
            {
                //Find the Centroid/MidPoint
                //Vector3 smallest = Vector3.zero;
                //Vector3 biggest = Vector3.zero;
                MidPoint = CalculateMidPoint();

                //Brute Force Find furthest distnace
                float maxDist = 0;
                Vector3 pointa = Vector3.zero;
                Vector3 pointb = Vector3.zero;
                for (int j = 0; j < Controller.ActorsInScene.Count - 1; j++)
                {
                    for (int u = 1; u < Controller.ActorsInScene.Count; u++)
                    {
                        float tmpDist = Vector3.Distance(Controller.ActorsInScene[j].ActorGO.transform.position, Controller.ActorsInScene[u].ActorGO.transform.position);
                        if (tmpDist > maxDist)
                        {
                            maxDist = tmpDist;
                            pointa = Controller.ActorsInScene[j].ActorGO.transform.position;
                            pointb = Controller.ActorsInScene[u].ActorGO.transform.position;
                        }
                    }
                }
                Vector3 actorADirN = (pointa - pointb).normalized;
                Vector3 PDirRight = Quaternion.AngleAxis(90, Vector3.up) * actorADirN;
                Vector3 PDirLeft = Quaternion.AngleAxis(-90, Vector3.up) * actorADirN;


                markerRight = MidPoint + (PDirRight * 10);
                markerLeft = MidPoint + (PDirLeft * 10);

                //Patch
                markerRight.y = pointa.y;
                markerLeft.y = pointa.y;

                ChosenSideMarker = (CameraSide == Side.Right) ? markerRight : markerLeft;

               //var right = GameObject.Instantiate(new GameObject("markerRIGHT"));
               //right.transform.position = markerRight;

                //var left = GameObject.Instantiate(new GameObject("markerLeft"));
               // left.transform.position = markerLeft;

            }
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

