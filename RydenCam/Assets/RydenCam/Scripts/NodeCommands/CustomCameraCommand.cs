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


        public static string customCamLock_nodeId;

        public CustomCameraCommand(Node node)
        {
            Node = node;
            var talkable = node as ITalkable;
            convoData = talkable.NodeConvodata; 

            //RS TODO Update Camera Information here

        }

        public void Update(GameObject tempCustomCameraPosition)
        {


            if (tempCustomCameraPosition != null)
            {
                //Update the position
                convoData.ShotConfig.GlobalCustomCamPos = tempCustomCameraPosition.transform.position;
                convoData.ShotConfig.GlobalCustomCamRot = tempCustomCameraPosition.transform.rotation;
                Debug.Log("Setting cam position");
            }


            /*

            if (convoData.ShotConfig.IsCustomSet)
            {
                if (tempCustomCameraPosition != null)
                {
                    //Update the position
                    convoData.ShotConfig.GlobalCustomCamPos = tempCustomCameraPosition.transform.position;
                    convoData.ShotConfig.GlobalCustomCamRot = tempCustomCameraPosition.transform.rotation;
                    Debug.Log("Setting cam position");
                }

                /*
                else if (!GameObject.Find(BranchConstants.CustomCamera))
                {
                    PlaceCustomCam(convoData);
                }
                */
                //tempCustomCameraPosition = ResetPosition(convoData);
            /*
            else
            {
                ClearCamera();
            }
            */
            
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

        public void SetCustomCameraPosition(GameObject customCameraPosition)
        {
            EnsureUniqueCustomCameraSelection(Node);
            convoData.ShotConfig.GlobalCustomCamPos = customCameraPosition.transform.position;
            convoData.ShotConfig.GlobalCustomCamRot = customCameraPosition.transform.rotation;
            GameObject target = GameObject.Find(convoData.Actor.ActorName);
            convoData.ShotConfig.LocalRelativeActorPos = target.transform.position;
            convoData.ShotConfig.LocalRelativeActorRot = target.transform.rotation;
        }

        public void ClearCamera()
        {
            GameObject obj = GameObject.Find(BranchConstants.CustomCamera);
            if (obj != null) GameObject.DestroyImmediate(obj);

            convoData.ShotConfig.GlobalCustomCamPos = Vector3.zero;
            convoData.ShotConfig.GlobalCustomCamRot = Quaternion.identity;
            convoData.ShotConfig.IsCustomSet = false;
        }

        public GameObject PlaceCustomCam(ConversationData nodeConvodata)
        {
            //Instantiate the CustomCamera Prefab
            UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(BranchConstants.CamPrefabPath, typeof(GameObject));
            UnityEngine.Object obj = PrefabUtility.InstantiatePrefab(prefab);
            GameObject cameraObject = (GameObject)obj;

            nodeConvodata.ShotConfig.IsCustomSet = true;

            //Place the Camera
            PlaceCamera(nodeConvodata, cameraObject);
            Selection.activeObject = cameraObject;
            return cameraObject;
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
