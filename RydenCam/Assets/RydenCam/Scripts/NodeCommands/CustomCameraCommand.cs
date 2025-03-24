using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using RydenCam.SequenceData;
using UnityEditor;
using UnityEngine;

namespace Assets.RydenCam.Scripts.NodeCommands
{
    //Responsible for assigning the custom camera and 
    //setting the node camera information in the Graph editor
    public class CustomCameraCommand
    {
        public Node Node { get; }

        private ConversationData convoData { get; set; }

        public static GameObject tempCustomCamera { get; set; }
        
        public static string customCamLock_nodeId;

        //Called when a node is selected
        public CustomCameraCommand(Node node)
        {
            Node = node;
            var talkable = node as ITalkable;
            convoData = talkable.NodeConvodata; 
        }

        public void Update()
        {

            if (tempCustomCamera != null)
            {
                convoData.ShotConfig.GlobalCustomCamPos = tempCustomCamera.transform.position;
                convoData.ShotConfig.GlobalCustomCamRot = tempCustomCamera.transform.rotation;
            }
        }


        public GameObject ResetPosition(ConversationData nodeConvodata)
        {
            GameObject camReference = GameObject.Find(BranchConstants.CustomCamera);
            PlaceCamera(nodeConvodata, camReference);
            return camReference;
        }

        //This is for when the user clicks off or selects another node.
        //If the other node is also a node that uses custom camera, it will not use the position of the previously
        //created custom camera.
        public void EnsureUniqueCustomCameraSelection(Node curr)
        {
            /*
            foreach (Node node in nodes)
            {
                if (node != curr)
                {
                    node.SetCustomCameraPosition = null;
                }
            }
            */
        }

        public void SetCustomCameraPosition()
        {
            if (tempCustomCamera == null) return;

            EnsureUniqueCustomCameraSelection(Node);
            convoData.ShotConfig.GlobalCustomCamPos = tempCustomCamera.transform.position;
            convoData.ShotConfig.GlobalCustomCamRot = tempCustomCamera.transform.rotation;
            GameObject target = GameObject.Find(convoData.Actor.ActorName);
            convoData.ShotConfig.LocalRelativeActorPos = target.transform.position;
            convoData.ShotConfig.LocalRelativeActorRot = target.transform.rotation;
        }



        public void UpdateCustomCamera()
        {
            ITalkable talk = Node as ITalkable;
           
            PlaceCustomCam(talk.NodeConvodata);
        }


        public void ClearCameraSceneObject()
        {
            GameObject obj = GameObject.Find(BranchConstants.CustomCamera);
            if (obj != null) GameObject.DestroyImmediate(obj);

            tempCustomCamera = null;
        }

        public void ClearCamera()
        {
            ClearCameraSceneObject();

            convoData.ShotConfig.GlobalCustomCamPos = Vector3.zero;
            convoData.ShotConfig.GlobalCustomCamRot = Quaternion.identity;
            convoData.ShotConfig.IsCustomSet = false;
        }

        public void PlaceCustomCam(ConversationData nodeConvodata)
        {
            if (nodeConvodata.ShotConfig.GoalType == CameraGoal.Custom && tempCustomCamera == null)
            {

                //Instantiate the CustomCamera Prefab
                UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(BranchConstants.CamPrefabPath, typeof(GameObject));
                UnityEngine.Object obj = PrefabUtility.InstantiatePrefab(prefab);
                GameObject cameraObject = (GameObject)obj;

                nodeConvodata.ShotConfig.IsCustomSet = true;

                //Place the Camera
                PlaceCamera(nodeConvodata, cameraObject);
                Selection.activeObject = cameraObject;
                tempCustomCamera = cameraObject;
            }


        }

        public void PlaceCamera(ConversationData nodeConvodata, GameObject obj)
        {
            if (obj is GameObject gameObjectRef)
            {
                if (nodeConvodata.ShotConfig.GoalCustomType == CustomCameraType.Local)
                {
                    //Local
                    GameObject target = GameObject.Find(nodeConvodata.Actor.ActorName);
                    Vector3 pos_result = target.transform.position - nodeConvodata.ShotConfig.LocalRelativeActorPos;
                    gameObjectRef.transform.position = nodeConvodata.ShotConfig.LocalRelativeActorPos + pos_result;
                    gameObjectRef.transform.rotation = nodeConvodata.ShotConfig.LocalRelativeActorRot;
                }
                else if (nodeConvodata.ShotConfig.GoalCustomType == CustomCameraType.Global || nodeConvodata.ShotConfig.GoalCustomType == CustomCameraType.None)
                {
                    //Global
                    gameObjectRef.transform.position = nodeConvodata.ShotConfig.GlobalCustomCamPos;
                    gameObjectRef.transform.rotation = nodeConvodata.ShotConfig.GlobalCustomCamRot;
                }
            }
        }

    }
}
