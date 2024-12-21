using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.NodeCommands;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private ITalkable currentNode { get; set; } 
        private CustomCameraCommand currentCommand { get; set; }

        public NodeCameraOptionsDrawer(ITalkable node, GUIStyle _inspectorText, GUIStyle _labelStyleHead_Panel)
        {
            currentNode = node;
            Node result = node as Node;
            currentCommand = new CustomCameraCommand(result);

            inspectorText = _inspectorText;
            labelStyleHead_Panel = _labelStyleHead_Panel;


            //When Load conversation happens, the tempcamera gameobject appears, dont want this to happen
            currentCommand.UpdateCustomCamera();
        }





        public void DrawUICamCompOptions()
        {

            ConversationData conversationData = currentNode.NodeConvodata;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shot Composition", labelStyleHead_Panel);
            EditorGUILayout.Space();

            List<string> options_Distance = Enum.GetNames(typeof(CameraDistance)).ToList();
            List<string> options_Angle = Enum.GetNames(typeof(CameraAngle)).ToList();
            int index_dist = Array.IndexOf(Enum.GetValues(typeof(CameraDistance)), conversationData.ShotConfig.GoalDistance);
            int index_angle = Array.IndexOf(Enum.GetValues(typeof(CameraAngle)), conversationData.ShotConfig.GoalAngle);

            EditorGUILayout.LabelField("Type", inspectorText, GUILayout.Width(50));
            GUILayout.BeginHorizontal("box");
            CameraGoal selected_goal = (CameraGoal)EditorGUILayout.EnumPopup(conversationData.ShotConfig.GoalType, GUILayout.Width(140));
            if (conversationData.ShotConfig.GoalType != selected_goal)
            {
                conversationData.ShotConfig.GoalType = selected_goal;
            }

            if (selected_goal == CameraGoal.OverShoulder || selected_goal == CameraGoal.FrameShare)
            {
                var actors = NodeManager.Instance.ActorsInScene()
                    .Where(x => x.ActorName != conversationData.Actor.ActorName)
                    .Select(x => x.ActorName)
                    .ToList();

                int OppActorIndex = actors.IndexOf(conversationData.ShotConfig.oppositeActor);
                if (OppActorIndex == -1) OppActorIndex = 0;

                if (actors.Count > 0)
                {
                    OppActorIndex = EditorGUILayout.Popup(OppActorIndex, actors.ToArray(), GUILayout.Width(70));
                    conversationData.ShotConfig.oppositeActor = actors[OppActorIndex];
                }
            }
            GUILayout.EndHorizontal();

            //Everything has distance and Y angle except custom
            if (selected_goal != CameraGoal.Custom)
            {
                EditorGUILayout.LabelField("Distance", inspectorText, GUILayout.Width(50));
                GUILayout.BeginHorizontal("box");
                index_dist = EditorGUILayout.Popup(index_dist, options_Distance.ToArray(), GUILayout.Width(140));
                if (index_dist == -1) { index_dist = 0; }
                if (conversationData.ShotConfig.GoalDistance != (CameraDistance)Enum.GetValues(typeof(CameraDistance)).GetValue(index_dist))
                {
                    conversationData.ShotConfig.GoalDistance = ((CameraDistance)Enum.GetValues(typeof(CameraDistance)).GetValue(index_dist));
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Height", inspectorText, GUILayout.Width(50));
                GUILayout.BeginHorizontal("box");
                index_angle = EditorGUILayout.Popup(index_angle, options_Angle.ToArray(), GUILayout.Width(140));
                if (index_angle == -1) { index_angle = 0; }
                if (conversationData.ShotConfig.GoalAngle != (CameraAngle)Enum.GetValues(typeof(CameraAngle)).GetValue(index_angle))
                {
                    conversationData.ShotConfig.GoalAngle = ((CameraAngle)Enum.GetValues(typeof(CameraAngle)).GetValue(index_angle));
                }
                GUILayout.EndHorizontal();
            }
            //It is In Custom 
            else
            {
                GUILayout.BeginHorizontal("box");
                CustomCameraType selected_customType = (CustomCameraType)EditorGUILayout.EnumPopup(conversationData.ShotConfig.GoalCustomType, GUILayout.Width(140));
                //check if it's been updated
                if (selected_customType != conversationData.ShotConfig.GoalCustomType)
                {
                    conversationData.ShotConfig.GoalCustomType = selected_customType;
                }
                if (conversationData.ShotConfig.GoalCustomType == CustomCameraType.Local)
                {
                    EditorGUILayout.LabelField(" to " + conversationData.Actor.ActorName, inspectorText, GUILayout.Width(50));
                }

                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                //Set Camera Position if it has not already been set
                //If user clicks from one custom node to another custom node, camera needs to be updated
                currentCommand.Update();


                string buttonText = string.Empty;

                // Custom Camera Buttons
                if (!GameObject.Find(BranchConstants.CustomCamera) && conversationData.ShotConfig.IsCustomSet)
                {
                    buttonText = "ReCreate Custom Camera";
                    if (GUILayout.Button(buttonText, GUILayout.Width(170), GUILayout.Height(30)))
                    {
                        currentCommand.PlaceCustomCam(conversationData);

                    }
                }
                else if(!GameObject.Find(BranchConstants.CustomCamera))
                {
                    buttonText = "Create Custom Camera";
                    if (GUILayout.Button(buttonText, GUILayout.Width(170), GUILayout.Height(30)))
                    {
                        currentCommand.PlaceCustomCam(conversationData);
                    }
                }


                else if (GUILayout.Button("Clear Camera", GUILayout.Width(170), GUILayout.Height(30)))
                {
                    currentCommand.ClearCamera();
                }


                //Update Camera Position
                using (var customCameraScope = new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Drag to set Cam:", inspectorText, GUILayout.Width(60));

                    CustomCameraCommand.tempCustomCamera = (GameObject)EditorGUILayout.ObjectField(CustomCameraCommand.tempCustomCamera, typeof(GameObject), true);
                    currentCommand.SetCustomCameraPosition();

                }

                //If Set Display the coordinates
                if (currentNode.NodeConvodata.ShotConfig.GlobalCustomCamPos != null  && currentNode.NodeConvodata.ShotConfig.GlobalCustomCamRot != null)
                {

                    var positionData = currentNode.NodeConvodata?.ShotConfig?.GlobalCustomCamPos ?? Vector3.zero; 
                    var rotationData = currentNode.NodeConvodata?.ShotConfig?.GlobalCustomCamRot ?? Quaternion.identity;

                    // Format the position components to two decimal places
                    float posX = Mathf.Round(positionData.x * 100) / 100;
                    float posY = Mathf.Round(positionData.y * 100) / 100;
                    float posZ = Mathf.Round(positionData.z * 100) / 100;

                    float rotX = Mathf.Round(rotationData.x * 100) / 100;
                    float rotY = Mathf.Round(rotationData.y * 100) / 100;
                    float rotZ = Mathf.Round(rotationData.z * 100) / 100;

                    // Create a formatted string with the position data
                    //GUILayout.Label( $"Position Set ✓ X:{x:0.00} Y:{y:0.00} Z:{z:0.00}");
                    GUILayout.Label($"Position Set ✓ X:{posX:0.00} Y:{posY:0.00} Z:{posZ:0.00}");
                    GUILayout.Label($"Rotation Set ✓ X:{rotX:0.00} Y:{rotY:0.00} Z:{rotZ:0.00}");
                }

            }
        }

    }
}
