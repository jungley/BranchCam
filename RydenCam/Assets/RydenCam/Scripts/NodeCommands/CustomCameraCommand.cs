using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Camera;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using RydenCam.SequenceData;
using System;
using UnityEngine;

namespace Assets.RydenCam.Scripts.NodeCommands
{
    /*
    //Responsible for assigning the custom camera and 
    //setting the node camera information in the Graph editor
    public class CustomCameraCommand
    {

        //private ConversationData convoData { get; set; }

        private static GameObject _customCameraObject;
        public static event Action<GameObject> OnCustomCameraChanged;

        public static GameObject CustomCameraObject
        {
            get => _customCameraObject;
            set
            {
                if (_customCameraObject != value)
                {
                    _customCameraObject = value;
                    OnCustomCameraChanged?.Invoke(_customCameraObject);
                }
            }
        }

        public bool IsCustomSet
        {
            get => convoData.ShotConfig.IsCustomSet;
            set => convoData.ShotConfig.IsCustomSet = value;
        }
        
        /// <summary>
        /// If the custom camera is active in the scene
        /// </summary>
        public static bool IsCustomCameraActive
        {
            get
            {
                if (CustomCameraObject == null) return false;
                return CustomCameraObject.activeSelf;
            }
        }

        //Called when a node is selected
        public CustomCameraCommand(ITalkable node)
        {
            convoData = node.NodeConvodata;
        }


        public void AssignCustomCameraPosition()
        {
            if (CustomCameraObject == null) return;

            convoData.ShotConfig.GlobalCustomCamPos = CustomCameraObject.transform.position;
            convoData.ShotConfig.GlobalCustomCamRot = CustomCameraObject.transform.rotation;
        }


        public void ClearCameraSceneObject()
        {
            GameObject obj = GameObject.Find(BranchConstants.CustomCamera);
            if (obj != null) GameObject.DestroyImmediate(obj);

            CustomCameraObject = null;
        }

        public void ClearCamera()
        {
            ClearCameraSceneObject();

            convoData.ShotConfig.GlobalCustomCamPos = Vector3.zero;
            convoData.ShotConfig.GlobalCustomCamRot = Quaternion.identity;
            IsCustomSet = false;
        }

        public void PlaceCustomCam(ConversationData nodeConvodata)
        {
            if (nodeConvodata.ShotConfig.GoalType == CameraGoal.Custom && !IsCustomCameraActive)
            {
                UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(BranchConstants.CamPrefabPath, typeof(GameObject));
                UnityEngine.Object prefabObj = PrefabUtility.InstantiatePrefab(prefab);
                GameObject cameraObject = (GameObject)prefabObj;

                nodeConvodata.ShotConfig.IsCustomSet = true;

                if (prefabObj is GameObject gameObjectRef)
                {
                    gameObjectRef.transform.position = nodeConvodata.ShotConfig.GlobalCustomCamPos;
                    gameObjectRef.transform.rotation = nodeConvodata.ShotConfig.GlobalCustomCamRot;

                    // Add watcher and subscribe
                    var watcher = gameObjectRef.GetComponent<CustomCameraTransformWatcher>();
                    watcher.SetPose(nodeConvodata.ShotConfig.GlobalCustomCamPos, nodeConvodata.ShotConfig.GlobalCustomCamRot);
                    watcher.OnTransformChanged += pose =>
                    {
                        nodeConvodata.ShotConfig.GlobalCustomCamPos = pose.position;
                        nodeConvodata.ShotConfig.GlobalCustomCamRot = pose.rotation;
                    };
                    
                }

                Selection.activeObject = cameraObject;
                CustomCameraObject = cameraObject;
            }
        }
    }
    */
}
