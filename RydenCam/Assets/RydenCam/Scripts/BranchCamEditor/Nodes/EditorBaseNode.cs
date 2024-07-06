using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.SequenceData;
using RydenCam.Common;
using RydenCam.BranchCamEditor.Controllers;

namespace RydenCam.BranchCamEditor.Nodes
{


    [ExecuteAlways]
    [System.Serializable]
    public abstract class EditorBaseNode: INode
    {
        //windowRect contains the location of the node
        public Rect windowRect;
        public string windowTitle = "";
        public float nodeWidth;
        public float nodeHeight;
        protected GUIStyle labelStyleHead_Panel;
        protected GUIStyle labelStyleHead_Node;
        protected GUIStyle inspectorText;
        protected GUIStyle inspectorTextBold;

        public bool isCustomGlobal = false;

        public string Sel_ActorID;
        public GameObject SetCustomCameraPosition = null;

        public Color nodeColor;

        private Texture2D _headerTexture { get; set; }
        public Texture2D HeaderTexture
        {
            get
            {
                if(_headerTexture == null)
                {
                    _headerTexture = new Texture2D(1, 1);
                    _headerTexture.SetPixel(1, 1, nodeColor);
                    _headerTexture.Apply();
                }

                return _headerTexture;
            }
        }

        public ConnectionPoint PointIn;
        public List<ConnectionPoint> PointOut;

        public string node_id;
        public static string customCamLock_nodeId;

        public virtual NodeType TypeOfNode => NodeType.None;

        public virtual void DrawForInspector()
        {
            GUI.DrawTextureWithTexCoords(new Rect(0, 35, 250.0f, 25.0f), HeaderTexture, new Rect(0, 0, 1, 1.0f));
        }
        public abstract bool isOverPoint(Vector2 mousePos);
        public abstract ConnectionPoint getConPoint(Vector2 mousePos);
        public abstract void DrawContent();

        public EditorBaseNode()
        {
            //Instantiate Styles common a across all nodes
            labelStyleHead_Panel = new GUIStyle();
            labelStyleHead_Panel.normal.textColor = Color.white;
            labelStyleHead_Panel.fontStyle = FontStyle.Bold;
            labelStyleHead_Panel.fontSize = 15;

            labelStyleHead_Node = new GUIStyle();
            labelStyleHead_Node.normal.textColor = Color.white;
            labelStyleHead_Node.fontStyle = FontStyle.Bold;
            labelStyleHead_Node.fontSize = 15;

            nodeColor = Color.gray;

            inspectorText = new GUIStyle();
            inspectorText.normal.textColor = Color.white;

            inspectorTextBold = new GUIStyle();
            inspectorTextBold.normal.textColor = Color.white;
            inspectorTextBold.fontStyle = FontStyle.Bold;

            //Add Node to RuntimeDictionary
            Guid guidVal = Guid.NewGuid();
            node_id = guidVal.ToString();
        }

#if UNITY_EDITOR
        public void DrawUICamCompOptions(ConversationData nodeConvodata)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            //DRAW CAMERASHOT SELECTOR -- SAME CODE IN DIALOGUE
            EditorGUILayout.LabelField("Shot Composition", labelStyleHead_Panel);
            EditorGUILayout.Space();

            //DRAW CAMERASHOT SELECTOR -- SAME CODE IN DIALOGUE
            List<string> options_Distance = Enum.GetNames(typeof(CameraDistance)).ToList();
            List<string> options_Angle = Enum.GetNames(typeof(CameraAngle)).ToList();
            int index_dist = Array.IndexOf(Enum.GetValues(typeof(CameraDistance)), nodeConvodata.ShotConfig.GoalDistance);
            int index_angle = Array.IndexOf(Enum.GetValues(typeof(CameraAngle)), nodeConvodata.ShotConfig.GoalAngle);

            //DROPDOWN FOR TYPE 
            EditorGUILayout.LabelField("Type", inspectorText, GUILayout.Width(50));
            GUILayout.BeginHorizontal("box");
            CameraGoal selected_goal = (CameraGoal) EditorGUILayout.EnumPopup(nodeConvodata.ShotConfig.GoalType, GUILayout.Width(140));
            //check if it's been updated


            if (nodeConvodata.ShotConfig.GoalType != selected_goal)
            {
                nodeConvodata.ShotConfig.GoalType =selected_goal;
            }


            //If it's Exterior or Apex
            if (selected_goal == CameraGoal.OverShoulder || selected_goal == CameraGoal.FrameShare)
            {
                //Generate List Except of actor associated with node
                List<string> tmp = new List<string>();
                tmp.AddRange(EditorController.Instance.ActorsInScene.Select(x => x.ActorName));
                tmp.Remove(nodeConvodata.Actor.ActorName);
                int OppActorIndex = tmp.IndexOf(nodeConvodata.ShotConfig.oppositeActor);
                if (OppActorIndex == -1) { OppActorIndex = 0; }

                if (tmp.Count > 0)
                {
                    OppActorIndex = EditorGUILayout.Popup(OppActorIndex, tmp.ToArray(), GUILayout.Width(70));
                    nodeConvodata.ShotConfig.oppositeActor = tmp[OppActorIndex];
                }
            }
            GUILayout.EndHorizontal();

            //Everything has distance and Y angle except custom
            if (selected_goal != CameraGoal.Custom)
            {
                //DROPDOWN FOR DISTANCE
                EditorGUILayout.LabelField("Distance", inspectorText, GUILayout.Width(50));
                GUILayout.BeginHorizontal("box");
                index_dist = EditorGUILayout.Popup(index_dist, options_Distance.ToArray(), GUILayout.Width(140));
                if (index_dist == -1) { index_dist = 0; }
                //check if it's been updated
                if (nodeConvodata.ShotConfig.GoalDistance != (CameraDistance)Enum.GetValues(typeof(CameraDistance)).GetValue(index_dist))
                {
                    nodeConvodata.ShotConfig.GoalDistance = ((CameraDistance)Enum.GetValues(typeof(CameraDistance)).GetValue(index_dist));
                }
                GUILayout.EndHorizontal();

                //DROPDOWN FOR ANGLE
                EditorGUILayout.LabelField("Height", inspectorText, GUILayout.Width(50));
                GUILayout.BeginHorizontal("box");
                index_angle = EditorGUILayout.Popup(index_angle, options_Angle.ToArray(), GUILayout.Width(140));
                if (index_angle == -1) { index_angle = 0; }
                //check if it's been updated
                if (nodeConvodata.ShotConfig.GoalAngle != (CameraAngle)Enum.GetValues(typeof(CameraAngle)).GetValue(index_angle)) 
                {
                    nodeConvodata.ShotConfig.GoalAngle = ((CameraAngle)Enum.GetValues(typeof(CameraAngle)).GetValue(index_angle));
                }
                GUILayout.EndHorizontal();
            }
            //It is In Custom 
            else
            {
                GUILayout.BeginHorizontal("box");
                CustomCameraType selected_customType = (CustomCameraType)EditorGUILayout.EnumPopup(nodeConvodata.ShotConfig.GoalCustomType, GUILayout.Width(140));
                //check if it's been updated
                if (selected_customType != nodeConvodata.ShotConfig.GoalCustomType)
                {
                    nodeConvodata.ShotConfig.GoalCustomType = selected_customType;
                }
                if (nodeConvodata.ShotConfig.GoalCustomType == CustomCameraType.Local)
                {
                    EditorGUILayout.LabelField(" to " + nodeConvodata.Actor.ActorName, inspectorText, GUILayout.Width(50));
                }

                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                //Set Camera Position if it has not already been set
                //If user clicks from one custom node to another custom node, camera needs to be updated
                if (customCamLock_nodeId != node_id)
                {
                    if(customCamHasBeenSet(nodeConvodata))
                    {
                        SetCustomCameraPosition = ResetPosition(nodeConvodata);
                        /*
                        if (!GameObject.Find(BranchConstants.CustomCamera))
                        {
                            PlaceCustomCam(nodeConvodata);
                        }
                        SetCustomCameraPosition = ResetPosition(nodeConvodata);
                        */
                    }
                    else
                    {
                        DestroyCustomCamera();
                    }
                    customCamLock_nodeId = node_id;
                }


                //Custom Camera Buttons
                if (!GameObject.Find(BranchConstants.CustomCamera))
                {
                    if (nodeConvodata.ShotConfig.CustomCamPos != null && nodeConvodata.ShotConfig.CustomCamRot != null)
                    {
                        if (GUILayout.Button("ReCreate Custom Camera", GUILayout.Width(170), GUILayout.Height(30)))
                        {
                            SetCustomCameraPosition = PlaceCustomCam(nodeConvodata);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Create Custom Camera", GUILayout.Width(170), GUILayout.Height(30)))
                        {
                            SetCustomCameraPosition = PlaceCustomCam(nodeConvodata);
                        }
                    }
                }
                
                else if (GUILayout.Button("Clear Camera", GUILayout.Width(170), GUILayout.Height(30)))
                {
                    DestroyCustomCamera();
                    nodeConvodata.ShotConfig.CustomCamPos = null;
                    nodeConvodata.ShotConfig.CustomCamRot = null;
                }

                //Update Camera Position
                using (var horizontalScope33 = new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Drag to set Cam:", inspectorText, GUILayout.Width(60));
                    SetCustomCameraPosition = (GameObject)EditorGUILayout.ObjectField(SetCustomCameraPosition, typeof(GameObject), true);
                    if (SetCustomCameraPosition != null)
                    {
                        NodeManager.Instance.EnsureUniqueCustomCameraSelection(this);
                        nodeConvodata.ShotConfig.CustomCamPos = SetCustomCameraPosition.transform.position;
                        nodeConvodata.ShotConfig.CustomCamRot = SetCustomCameraPosition.transform.rotation;
                        GameObject target = GameObject.Find(nodeConvodata.Actor.ActorName);
                        nodeConvodata.ShotConfig.LocalRelativeActorPos = target.transform.position;
                        nodeConvodata.ShotConfig.LocalRelativeActorRot = target.transform.rotation;
                    }
                }

                //If Set Display the coordinates
                if (nodeConvodata.ShotConfig.CustomCamPos != null && nodeConvodata.ShotConfig.CustomCamRot != null)
                {
                    //This will eventually be replaced by preview area 
                    GUILayout.Label("The Custom Position has been set!");
                }

            }
        }
#endif


        public bool customCamHasBeenSet(ConversationData nodeConvodata)
        {
            return nodeConvodata.ShotConfig.CustomCamPos != null && nodeConvodata.ShotConfig.CustomCamRot != null;
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
                    gameObjectRef.transform.position = nodeConvodata.ShotConfig.CustomCamPos.Value + pos_result;
                    gameObjectRef.transform.rotation = nodeConvodata.ShotConfig.CustomCamRot.Value;
                }
                else if(nodeConvodata.ShotConfig.GoalCustomType == CustomCameraType.Global || nodeConvodata.ShotConfig.GoalCustomType == CustomCameraType.None)
                {
                    //Global
                    gameObjectRef.transform.position = (nodeConvodata.ShotConfig.CustomCamPos != null) ? nodeConvodata.ShotConfig.CustomCamPos.Value : Vector3.zero;
                    gameObjectRef.transform.rotation = (nodeConvodata.ShotConfig.CustomCamRot != null) ? nodeConvodata.ShotConfig.CustomCamRot.Value : Quaternion.identity;
                }
            }
        }

        public GameObject PlaceCustomCam(ConversationData nodeConvodata)
        {
#if UNITY_EDITOR
            //Instantiate the CustomCamera Prefab
            UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(BranchConstants.CamPrefabPath, typeof(GameObject));
            UnityEngine.Object obj = PrefabUtility.InstantiatePrefab(prefab);
            GameObject cameraObject = (GameObject)obj;

            //Place the Camera
            PlaceCamera(nodeConvodata, cameraObject);
            Selection.activeObject = cameraObject;
            return cameraObject;
#else
            return null;
#endif

        }

        public GameObject ResetPosition(ConversationData nodeConvodata)
        {
            GameObject camReference = GameObject.Find(BranchConstants.CustomCamera);
            PlaceCamera(nodeConvodata, camReference);
            return camReference;
        }

        public void DestroyCustomCamera()
        {
            GameObject obj = GameObject.Find(BranchConstants.CustomCamera);
            if(obj != null)  GameObject.DestroyImmediate(obj);
        }

        public bool containsPoint(ConnectionPoint point)
        {
            return (point == PointIn || PointOut.Contains(point));
        }

        public static void OnClickRemoveConnection(Connection connection)
        {
            ConnectionManager.Instance.Remove(connection);
        }

        public virtual EditorBaseNode GetNextNode()
        {
            return PointOut[0]?.connectedTo?.node;
        }
    }
}