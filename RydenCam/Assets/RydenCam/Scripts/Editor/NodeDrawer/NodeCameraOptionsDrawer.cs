using Assets.RydenCam.Scripts.NodeCommands;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.RydenCam.Scripts.Editor.NodeDrawer
{
    //Used In Dialogue and Decision Nodes
    //The View for the Camera Options on nodes
    public class NodeCameraOptionsDrawer
    {

        private GUIStyle inspectorText { get; set; }
        private GUIStyle labelStyleHead_Panel { get; set; }


        private GameObject SetCustomCameraPosition { get; set; }
        public string node_id { get; set; }

        public static string customCamLock_nodeId;
    
        public NodeCameraOptionsDrawer(GUIStyle _inspectorText, GUIStyle _labelStyleHead_Panel)
        {
            inspectorText = _inspectorText;
            labelStyleHead_Panel = _labelStyleHead_Panel;
        }

        public void DrawUICamCompOptions(ConversationData nodeConvodata, INodeCommand command)
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
            CameraGoal selected_goal = (CameraGoal)EditorGUILayout.EnumPopup(nodeConvodata.ShotConfig.GoalType, GUILayout.Width(140));
            //check if it's been updated


            if (nodeConvodata.ShotConfig.GoalType != selected_goal)
            {
                nodeConvodata.ShotConfig.GoalType = selected_goal;
            }


            if (selected_goal == CameraGoal.OverShoulder || selected_goal == CameraGoal.FrameShare)
            {
                var actors = NodeManager.Instance.ActorsInScene()
                    .Where(x => x.ActorName != nodeConvodata.Actor.ActorName)
                    .Select(x => x.ActorName)
                    .ToList();

                int OppActorIndex = actors.IndexOf(nodeConvodata.ShotConfig.oppositeActor);
                if (OppActorIndex == -1) OppActorIndex = 0;

                if (actors.Count > 0)
                {
                    OppActorIndex = EditorGUILayout.Popup(OppActorIndex, actors.ToArray(), GUILayout.Width(70));
                    nodeConvodata.ShotConfig.oppositeActor = actors[OppActorIndex];
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
                /*
                if (customCamLock_nodeId != node_id)
                {
                    if (customCamHasBeenSet(nodeConvodata))
                    {
                        SetCustomCameraPosition = ResetPosition(nodeConvodata);
                        /*
                        if (!GameObject.Find(BranchConstants.CustomCamera))
                        {
                            PlaceCustomCam(nodeConvodata);
                        }
                        SetCustomCameraPosition = ResetPosition(nodeConvodata);
                        
                    }
                    else
                    {
                        DestroyCustomCamera();
                    }
                    customCamLock_nodeId = node_id;
                }
                */


                //Custom Camera Buttons
                if (!GameObject.Find(BranchConstants.CustomCamera))
                {
                    if (nodeConvodata.ShotConfig.CustomCamPos != null && nodeConvodata.ShotConfig.CustomCamRot != null)
                    {
                        if (GUILayout.Button("ReCreate Custom Camera", GUILayout.Width(170), GUILayout.Height(30)))
                        {
                            //SetCustomCameraPosition = PlaceCustomCam(nodeConvodata);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Create Custom Camera", GUILayout.Width(170), GUILayout.Height(30)))
                        {
                            //SetCustomCameraPosition = PlaceCustomCam(nodeConvodata);
                        }
                    }
                }

                else if (GUILayout.Button("Clear Camera", GUILayout.Width(170), GUILayout.Height(30)))
                {
                    /*
                    DestroyCustomCamera();
                    nodeConvodata.ShotConfig.CustomCamPos = null;
                    nodeConvodata.ShotConfig.CustomCamRot = null;
                    */
                }

                //Update Camera Position
                using (var horizontalScope33 = new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Drag to set Cam:", inspectorText, GUILayout.Width(60));
                    SetCustomCameraPosition = (GameObject)EditorGUILayout.ObjectField(SetCustomCameraPosition, typeof(GameObject), true);
                    if (SetCustomCameraPosition != null)
                    {
                        //NodeManager.Instance.EnsureUniqueCustomCameraSelection(this);
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
    }
}
