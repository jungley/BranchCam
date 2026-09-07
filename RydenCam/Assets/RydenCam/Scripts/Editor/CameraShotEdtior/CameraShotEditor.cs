using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Camera;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
using Assets.RydenCam.Scripts.Editor.CameraShotEditor;
using Assets.RydenCam.Scripts.NodeCommands;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using RydenCam.Editor.Ribbon;
using RydenCam.Editor.Styling;
using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RydenCam.Editor
{
    public class CameraShotEditor : EditorWindow
    {
        private float distanceValue = 2f;
        private Vector2 scrollPos;
        private Vector2 windowScrollPos;

        public NodeGraphViewModel NodeGraphViewModel { get; set; }

        public CameraShotViewModel ViewModel { get; set; }

        private RibbonRenderer ribbonRenderer;

        //private CustomCameraCommand currentCommand { get; set; }
        public event Action UpdateShotRender;

        private void OnEnable()
        {
            minSize = new Vector2(500, 300);
            EditorApplication.delayCall += () =>
            {
                if (this != null && (ViewModel == null || ribbonRenderer == null))
                    InitializeWindowState();
            };
        }

        private void InitializeWindowState()
        {
            // Set a minimum window size so it's always visible
            minSize = new Vector2(500, 300);

            ViewModel = new CameraShotViewModel();

            var ribbonDefinition = new RibbonDefinitionBuilder()
                .AddButton("New", ViewModel.NewFile)
                .AddButton("Open", ViewModel.Open)
                //.AddButton("Save", ViewModel.Save)
                .AddButton("Save As", ViewModel.SaveAs)
                .Build();

            ribbonRenderer = new RibbonRenderer(ribbonDefinition);
        }



        private void OnDisable()
        {
         
        }


        private void OnGUI()
        {
            if (ViewModel == null || ribbonRenderer == null)
                InitializeWindowState();

            //Draw the ribbon
            ribbonRenderer.Draw(position.width);

            if (CameraShotsManager.Instance.CameraShots.Count == 0) return;

            windowScrollPos = EditorGUILayout.BeginScrollView(windowScrollPos);
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            // Preview Section with fixed width
            GUILayout.BeginVertical(GUILayout.Width(320));
                DrawShotPreviewSection();
            GUILayout.EndVertical();

            // Configuration Section with fixed width
            GUILayout.BeginVertical(GUILayout.Width(200));
                DrawShotConfigurationSection();
            GUILayout.EndVertical();

            // List Section
            GUILayout.BeginVertical(GUILayout.Width(200));
                DrawCameraShotListSection();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            EditorGUILayout.Space(12f);
            DrawBottomConfigurationPanel();

            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }   

        private void DrawShotPreviewSection()
        {
            GUIStyle largeBoldLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = BranchCamEditorTheme.FontTitle + 2,
                normal = { textColor = BranchCamEditorTheme.TextPrimary },
                alignment = TextAnchor.MiddleCenter
            };

            GUILayout.Label("Shot Configuration Manager", largeBoldLabel);

            float margin = 10f;
            float boxWidth = 300f; // Fixed width on the left
            float boxHeight = Mathf.Max(200, position.height * 0.5f);

            // Define the preview box rect on the left side of the window
            Rect boxRect = new Rect(margin, margin + 50f, boxWidth, boxHeight);

            // Optional: draw a background to visualize the box
            EditorGUI.DrawRect(boxRect, BranchCamEditorTheme.PanelBackground);

            var actors = NodeManager.Instance.ActorsInScene;
            if (actors.Count == 0 || actors[0]?.PreviewData?.ActorPositionData == null) return;

            // Get position data
            var posData = actors[0].PreviewData.ActorPositionData;
            var oppPosData = actors.Count > 1 ? actors[1]?.PreviewData?.ActorPositionData : null;

            // Render the preview
            ActorPositionData dataCopy = new ActorPositionData
            {
                ActorPosition = posData.ActorPosition,
                ActorRotation = posData.ActorRotation,
                ForwardN = posData.ForwardN
            };           

            ActorPositionData oppositeCopy = null;
            if (oppPosData != null)
            {
                Vector3 direction = oppPosData.ActorPosition - posData.ActorPosition;
                if (direction.sqrMagnitude < 0.0001f)
                    direction = posData.ForwardN.sqrMagnitude > 0.0001f ? posData.ForwardN : Vector3.forward;

                oppositeCopy = new ActorPositionData
                {
                    ActorPosition = posData.ActorPosition + direction.normalized * distanceValue,
                    ActorRotation = oppPosData.ActorRotation,
                    ForwardN = oppPosData.ForwardN
                };
            }

            ViewModel.PreviewRenderer.ComposePreviewImage(boxRect, ViewModel.CurrentShot, dataCopy, oppositeCopy);
            GUILayout.Space(boxHeight + 35f);

        }

        private void DrawShotConfigurationSection()
        {
            EditorGUILayout.Space(20f);

            var shot = ViewModel?.CurrentShot;
            if (shot == null)
                return;

            EditorGUILayout.LabelField("Shot Name");

            // Assign a unique control name to the text field
            GUI.SetNextControlName("ShotNameField");
            using (new EditorGUI.DisabledScope(shot.IsDefault))
                shot.ShotName = EditorGUILayout.TextField(shot.ShotName, GUILayout.Width(150));

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
            CameraGoal selected_goal = EnumPopupExtensions.EnumPopup(shot.GoalType, filteredEnabled, width: 150, allowedGoals);
            if (shot.GoalType != selected_goal)
                shot.GoalType = selected_goal;

            if (selected_goal == CameraGoal.OverShoulder || selected_goal == CameraGoal.FrameShare)
            {
                EditorGUILayout.LabelField("Opp Actor");
                /*
                var actors = NodeManager.Instance.ActorsInScene
                    .Where(x => x.ActorID != shot.Actor)
                    .Select(x => x.ActorName)
                    .ToList();
                */
                /*
                var actors = NodeManager.Instance.ActorsInScene;

                int OppActorIndex = actors.IndexOf(shot.OppositeActor);

                if (OppActorIndex == -1) OppActorIndex = 0;

                if (actors.Count > 0)
                {
                    OppActorIndex = EditorGUILayout.Popup(OppActorIndex, actors.ToArray(), GUILayout.Width(140));
                    shot.OppositeActor = actors[OppActorIndex];
                }
                */
            }

            //Not Custom
            if (selected_goal != CameraGoal.Custom)
            {
                EditorGUILayout.LabelField("Distance");
                var options_Distance = Enum.GetNames(typeof(CameraDistance)).ToList();
                int index_dist = Array.IndexOf(Enum.GetValues(typeof(CameraDistance)), shot.GoalDistance);
                index_dist = EditorGUILayout.Popup(index_dist, options_Distance.ToArray(), GUILayout.Width(150));
                if (index_dist == -1) index_dist = 0;
                var newDist = (CameraDistance)Enum.GetValues(typeof(CameraDistance)).GetValue(index_dist);
                if (shot.GoalDistance != newDist)
                    shot.GoalDistance = newDist;

                EditorGUILayout.LabelField("Height");
                var options_Angle = Enum.GetNames(typeof(CameraAngle)).ToList();
                int index_angle = Array.IndexOf(Enum.GetValues(typeof(CameraAngle)), shot.GoalAngle);
                index_angle = EditorGUILayout.Popup(index_angle, options_Angle.ToArray(), GUILayout.Width(150));
                if (index_angle == -1) index_angle = 0;
                var newAngle = (CameraAngle)Enum.GetValues(typeof(CameraAngle)).GetValue(index_angle);
                if (shot.GoalAngle != newAngle)
                    shot.GoalAngle = newAngle;

            }
            //It is In Custom 
            else
            {
                DrawCustomShotConfiguration(shot);

                /*
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
                */
                //If Set Display the coordinates
                if (false && ViewModel.CurrentShot.IsCustomSet)
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

        private void DrawCustomShotConfiguration(CameraShotConfiguration shot)
        {
            EditorGUILayout.LabelField("Camera Position");
            shot.GlobalCustomCamPos = EditorGUILayout.Vector3Field(GUIContent.none, shot.GlobalCustomCamPos, GUILayout.Width(170));
            EditorGUILayout.LabelField("Camera Rotation");
            Vector3 euler = EditorGUILayout.Vector3Field(GUIContent.none, shot.GlobalCustomCamRot.eulerAngles, GUILayout.Width(170));
            shot.GlobalCustomCamRot = Quaternion.Euler(euler);

            if (GUILayout.Button("Capture Scene View", GUILayout.Width(170), GUILayout.Height(28)))
            {
                Camera sceneCamera = SceneView.lastActiveSceneView?.camera;
                if (sceneCamera != null)
                {
                    shot.GlobalCustomCamPos = sceneCamera.transform.position;
                    shot.GlobalCustomCamRot = sceneCamera.transform.rotation;
                    shot.IsCustomSet = true;
                    UpdateShotRender?.Invoke();
                }
            }

            if (GUILayout.Button("Use Entered Pose", GUILayout.Width(170)))
                shot.IsCustomSet = true;

            using (new EditorGUI.DisabledScope(!shot.IsCustomSet))
            {
                if (GUILayout.Button("Clear Custom Pose", GUILayout.Width(170)))
                {
                    shot.GlobalCustomCamPos = Vector3.zero;
                    shot.GlobalCustomCamRot = Quaternion.identity;
                    shot.IsCustomSet = false;
                }
            }

            shot.TogglePreviewRenderSceneView = EditorGUILayout.ToggleLeft("Preview in Scene View", shot.TogglePreviewRenderSceneView, GUILayout.Width(170));
            EditorGUILayout.HelpBox(shot.IsCustomSet ? "Custom camera pose is set." : "Enter a pose or capture the active Scene view.", MessageType.Info);
        }

        private void DrawBottomConfigurationPanel()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Shot Preview Controls", EditorStyles.boldLabel);
                CameraShotConfiguration shot = ViewModel?.CurrentShot;
                if (shot == null)
                {
                    EditorGUILayout.HelpBox("Select a camera shot to configure it.", MessageType.Info);
                    return;
                }

                EditorGUILayout.LabelField($"Selected: {shot.ShotName}");
                if (shot.GoalType == CameraGoal.OverShoulder || shot.GoalType == CameraGoal.FrameShare)
                {
                    distanceValue = EditorGUILayout.Slider("Actor Spacing", distanceValue, 1f, 20f);
                    EditorGUILayout.HelpBox("Actor Spacing changes the two-actor preview layout.", MessageType.None);
                }
                else if (shot.GoalType == CameraGoal.Custom)
                {
                    EditorGUILayout.HelpBox("Use the custom pose fields above or capture the active Scene view camera.", MessageType.None);
                }
                else
                {
                    EditorGUILayout.HelpBox("Portrait previews use the primary actor's configured position.", MessageType.None);
                }
            }
        }

        private void DrawCameraShotListSection()
        {
            EditorGUILayout.Space(20f);
            GUIStyle sectionLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = BranchCamEditorTheme.FontTitle,
                normal = { textColor = BranchCamEditorTheme.TextPrimary },
                alignment = TextAnchor.MiddleLeft
            };
            GUILayout.Label("Camera Shots", sectionLabel);

            float scrollViewHeight = 120f;

            // 🔹 Vertical-only scroll view (horizontal scrolling disabled)
            scrollPos = GUILayout.BeginScrollView(
                scrollPos,
                alwaysShowHorizontal: false,
                alwaysShowVertical: true,
                GUILayout.Height(scrollViewHeight)
            );

            // 🔹 Force horizontal scroll position to 0
            scrollPos.x = 0;

            var shots = CameraShotsManager.Instance.CameraShots;
            List<CameraShotConfiguration> shotsToRemove = new List<CameraShotConfiguration>();

            if (shots != null)
            {
                foreach (var shot in shots.ToList())
                {
                    GUILayout.BeginHorizontal();

                    // Slightly reduced width to avoid layout overflow (prevents unwanted horizontal bar)
                    if (GUILayout.Button(shot.ShotName, GUILayout.Width(145)))
                    {
                        ViewModel.CurrentShot = shot;
                    }

                    using (new EditorGUI.DisabledScope(shot.IsDefault))
                    {
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                            shotsToRemove.Add(shot);
                    }

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();

            // Add button
            if (GUILayout.Button("Add New Shot", GUILayout.Width(175)))
            {
                string newShotName = $"New Shot {shots.Count + 1}";
                var newShot = new CameraShotConfiguration(shotName: newShotName);
                shots.Add(newShot);
                ViewModel.CurrentShot = newShot;
            }


            // Remove after loop to avoid modifying collection during iteration
            foreach (var shot in shotsToRemove)
            {
                ViewModel.RemoveShot(shot);
            }
        }

    }
}
