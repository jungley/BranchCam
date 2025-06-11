using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
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
        public event Action UpdateShotRender;

        public NodeCameraOptionsDrawer(ITalkable node, GUIStyle _inspectorText, GUIStyle _labelStyleHead_Panel)
        {
            currentNode = node;
            currentCommand = new CustomCameraCommand(node);

            inspectorText = _inspectorText;
            labelStyleHead_Panel = _labelStyleHead_Panel;


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

            bool filteredEnabled = NodeManager.Instance.ActorsInScene.Count == 1;
            CameraGoal[] allowedGoals = new CameraGoal[] { CameraGoal.Portrait, CameraGoal.Custom };
            CameraGoal selected_goal = EnumPopupExtensions.EnumPopup(conversationData.ShotConfig.GoalType, filteredEnabled, width:140, allowedGoals);

            if (conversationData.ShotConfig.GoalType != selected_goal)
            {
                conversationData.ShotConfig.GoalType = selected_goal;
                UpdateShotRender?.Invoke();
            }

            if (selected_goal == CameraGoal.OverShoulder || selected_goal == CameraGoal.FrameShare)
            {
                var actors = NodeManager.Instance.ActorsInScene
                    .Where(x => x.ActorName != conversationData.Actor.ActorName)
                    .Select(x => x.ActorName)
                    .ToList();

                int OppActorIndex = actors.IndexOf(conversationData.ShotConfig.OppositeActor);
                if (OppActorIndex == -1) OppActorIndex = 0;

                if (actors.Count > 0)
                {
                    OppActorIndex = EditorGUILayout.Popup(OppActorIndex, actors.ToArray(), GUILayout.Width(70));
                    conversationData.ShotConfig.OppositeActor = actors[OppActorIndex];
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
                    UpdateShotRender?.Invoke();
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Height", inspectorText, GUILayout.Width(50));
                GUILayout.BeginHorizontal("box");
                index_angle = EditorGUILayout.Popup(index_angle, options_Angle.ToArray(), GUILayout.Width(140));
                if (index_angle == -1) { index_angle = 0; }
                if (conversationData.ShotConfig.GoalAngle != (CameraAngle)Enum.GetValues(typeof(CameraAngle)).GetValue(index_angle))
                {
                    conversationData.ShotConfig.GoalAngle = ((CameraAngle)Enum.GetValues(typeof(CameraAngle)).GetValue(index_angle));
                    UpdateShotRender?.Invoke();
                }
                GUILayout.EndHorizontal();
            }
            //It is In Custom 
            else
            {
                GUILayout.BeginHorizontal("box");

                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                //If the camera is not set but position has been set, place it
                if (CustomCameraCommand.CustomCameraObject == null && currentCommand.IsCustomSet)
                {
                    currentCommand.PlaceCustomCam(conversationData);
                }

                if (!CustomCameraCommand.IsCustomCameraActive)
                {
                    if (GUILayout.Button("Create Custom Camera", GUILayout.Width(170), GUILayout.Height(30)))
                    {
                        currentCommand.PlaceCustomCam(conversationData);
                    }
                }
                else
                {
                    if (GUILayout.Button("Clear Camera", GUILayout.Width(170), GUILayout.Height(30)))
                    {
                        currentCommand.ClearCamera();
                    }
                }

                //Update Camera Position
                using (var customCameraScope = new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Drag to set Cam:", inspectorText, GUILayout.Width(60));

                    CustomCameraCommand.CustomCameraObject = (GameObject)EditorGUILayout.ObjectField(CustomCameraCommand.CustomCameraObject, typeof(GameObject), true);
                    currentCommand.AssignCustomCameraPosition();
                }

                //If Set Display the coordinates
                if(currentCommand.IsCustomSet)
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
                    GUILayout.Space(10);
                    GUILayout.Label($"Position Set ✓ X:{posX:0.00} Y:{posY:0.00} Z:{posZ:0.00}");
                    GUILayout.Label($"Rotation Set ✓ X:{rotX:0.00} Y:{rotY:0.00} Z:{rotZ:0.00}");
                    GUILayout.Space(5);

                    currentNode.NodeConvodata.ShotConfig.TogglePreviewRenderSceneView = GUILayout.Toggle(currentNode.NodeConvodata.ShotConfig.TogglePreviewRenderSceneView, "Toggle Custom Scene View");
                }
            }
        }

    }
}
