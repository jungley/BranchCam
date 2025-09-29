using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
using Assets.RydenCam.Scripts.Editor.CameraShotEdtior;
using Assets.RydenCam.Scripts.NodeCommands;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RydenCam.Editor.CamersaShotEditor
{
    public class CameraShotEditor : EditorWindow
    {
        private float distanceValue = 1f;
        private Vector2 scrollPos;

        public CameraShotViewModel ViewModel { get; set; }

        private CustomCameraCommand currentCommand { get; set; }
        public event Action UpdateShotRender;

        private void OnEnable()
        {
            // Set a minimum window size so it's always visible
            minSize = new Vector2(400, 300);

            ViewModel = new CameraShotViewModel();

        }

        private void OnDisable()
        {
         
        }


        private void OnGUI()
        {
            GUILayout.BeginHorizontal();

            // Preview Section with fixed width
            GUILayout.BeginVertical(GUILayout.Width(400));
                DrawShotPreviewSection();
            GUILayout.EndVertical();

            // Configuration Section with fixed width
            GUILayout.BeginVertical(GUILayout.Width(250));
                DrawShotConfigurationSection();
            GUILayout.EndVertical();

            // List Section
            GUILayout.BeginVertical(GUILayout.Width(200));
                DrawCameraShotListSection();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }   

        private void DrawShotPreviewSection()
        {
            GUIStyle largeBoldLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            };

            GUILayout.Label("Image Preview", largeBoldLabel);

            float boxHeight = Mathf.Max(200, position.height * 0.5f);
            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(boxHeight));

            distanceValue = EditorGUILayout.Slider("Scale", distanceValue, 1f, 20f);
            GUILayout.Label($"Distance value: {distanceValue}");
        }

        private void DrawShotConfigurationSection()
        {
            GUIStyle largeBoldLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            };

            GUILayout.Label("Shot Composition", largeBoldLabel);
            EditorGUILayout.Space();

            var shot = ViewModel?.CurrentShot;
            if (shot == null)
                return;

            EditorGUILayout.LabelField("Shot Name");

            // Assign a unique control name to the text field
            GUI.SetNextControlName("ShotNameField");
            shot.ShotName = EditorGUILayout.TextField(shot.ShotName, GUILayout.Width(250));

            // Handle focus loss on Enter or mouse click outside
            Event e = Event.current;
            if (GUI.GetNameOfFocusedControl() == "ShotNameField")
            {
                // Press Enter
                if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
                {
                    GUI.FocusControl(null);
                    e.Use();
                }
                // Click outside
                else if (e.type == EventType.MouseDown && e.button == 0)
                {
                    // Only unfocus if the click is outside the text field rect
                    GUI.FocusControl(null);
                }
            }

            EditorGUILayout.LabelField("Type");
            bool filteredEnabled = NodeManager.Instance.ActorsInScene.Count == 1;
            CameraGoal[] allowedGoals = new CameraGoal[] { CameraGoal.Portrait, CameraGoal.Custom };
            CameraGoal selected_goal = EnumPopupExtensions.EnumPopup(shot.GoalType, filteredEnabled, width: 140, allowedGoals);
            if (shot.GoalType != selected_goal)
                shot.GoalType = selected_goal;

            if (selected_goal == CameraGoal.OverShoulder || selected_goal == CameraGoal.FrameShare)
            {
                EditorGUILayout.LabelField("Opp Actor");
                var actors = NodeManager.Instance.ActorsInScene
                    .Where(x => x.ActorName != shot.Actor)
                    .Select(x => x.ActorName)
                    .ToList();

                int OppActorIndex = actors.IndexOf(shot.OppositeActor);
                if (OppActorIndex == -1) OppActorIndex = 0;

                if (actors.Count > 0)
                {
                    OppActorIndex = EditorGUILayout.Popup(OppActorIndex, actors.ToArray(), GUILayout.Width(140));
                    shot.OppositeActor = actors[OppActorIndex];
                }
            }

            //Not Custom
            if (selected_goal != CameraGoal.Custom)
            {
                EditorGUILayout.LabelField("Distance");
                var options_Distance = Enum.GetNames(typeof(CameraDistance)).ToList();
                int index_dist = Array.IndexOf(Enum.GetValues(typeof(CameraDistance)), shot.GoalDistance);
                index_dist = EditorGUILayout.Popup(index_dist, options_Distance.ToArray(), GUILayout.Width(140));
                if (index_dist == -1) index_dist = 0;
                var newDist = (CameraDistance)Enum.GetValues(typeof(CameraDistance)).GetValue(index_dist);
                if (shot.GoalDistance != newDist)
                    shot.GoalDistance = newDist;

                EditorGUILayout.LabelField("Height");
                var options_Angle = Enum.GetNames(typeof(CameraAngle)).ToList();
                int index_angle = Array.IndexOf(Enum.GetValues(typeof(CameraAngle)), shot.GoalAngle);
                index_angle = EditorGUILayout.Popup(index_angle, options_Angle.ToArray(), GUILayout.Width(140));
                if (index_angle == -1) index_angle = 0;
                var newAngle = (CameraAngle)Enum.GetValues(typeof(CameraAngle)).GetValue(index_angle);
                if (shot.GoalAngle != newAngle)
                    shot.GoalAngle = newAngle;
            }
            //It is In Custom 
            else
            {
                GUILayout.BeginHorizontal("box");

                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                //If the camera is not set but position has been set, place it
                if (CustomCameraCommand.CustomCameraObject == null &&  ViewModel.CurrentShot.IsCustomSet)
                {
                    //currentCommand.PlaceCustomCam(conversationData);
                }

                if (!CustomCameraCommand.IsCustomCameraActive)
                {
                    if (GUILayout.Button("Create Custom Camera", GUILayout.Width(170), GUILayout.Height(30)))
                    {
                        //currentCommand.PlaceCustomCam(conversationData);
                    }
                }
                else
                {
                    if (GUILayout.Button("Clear Camera", GUILayout.Width(170), GUILayout.Height(30)))
                    {
                        //currentCommand.ClearCamera();
                    }
                }

                //Update Camera Position
                using (var customCameraScope = new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Drag to set Cam:", GUILayout.Width(60));

                    CustomCameraCommand.CustomCameraObject = (GameObject)EditorGUILayout.ObjectField(CustomCameraCommand.CustomCameraObject, typeof(GameObject), true);
                    //currentCommand.AssignCustomCameraPosition();
                }

                //If Set Display the coordinates
                if (ViewModel.CurrentShot.IsCustomSet)
                {

                    var positionData = ViewModel.CurrentShot?.GlobalCustomCamPos ?? Vector3.zero;
                    var rotationData = ViewModel.CurrentShot?.GlobalCustomCamRot ?? Quaternion.identity;

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

                    ViewModel.CurrentShot.TogglePreviewRenderSceneView = GUILayout.Toggle(ViewModel.CurrentShot.TogglePreviewRenderSceneView, "Toggle Custom Scene View");
                    
                }
            }
        }

        private void DrawCameraShotListSection()
        {
            GUIStyle largeBoldLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            };

            GUILayout.Label("Shot List", largeBoldLabel);
            GUILayout.Label("Camera Shots", EditorStyles.boldLabel);

            float scrollViewHeight = 200f;
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(scrollViewHeight));

            var shots = CameraShotsManager.Instance.CameraShots;
            List<CamShotConfig> shotsToRemove = new List<CamShotConfig>();

            if (shots != null)
            {
                foreach (var shot in shots.ToList())
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(shot.ShotName, GUILayout.Width(150)))
                    {
                        ViewModel.CurrentShot = shot;
                    }

                    if (GUILayout.Button("X", GUILayout.Width(25)))
                    {
                        shotsToRemove.Add(shot);
                    }

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();

            if (GUILayout.Button("Add New Shot", GUILayout.Width(180)))
            {
                string newShotName = $"New Shot {shots.Count + 1}";
                var newShot = new CamShotConfig(shotName: newShotName);
                shots.Add(newShot);
                ViewModel.CurrentShot = newShot;
            }

            // Remove after the loop to avoid modifying the collection during iteration
            foreach (var shot in shotsToRemove)
            {
                ViewModel.RemoveShot(shot);
            }
        }
    }
}