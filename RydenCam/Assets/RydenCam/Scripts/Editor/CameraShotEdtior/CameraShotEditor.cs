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
        private string previewActorId;
        private string previewOppositeActorId;
        private bool overridePreviewSpacing;
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
                .AddDropdown("File")
                .AddDropdownOption("File", "New", ViewModel.NewFile)
                .AddDropdownOption("File", "Open", ViewModel.Open)
                .AddDropdownOption("File", "Save", ViewModel.Save)
                .AddDropdownOption("File", "Save As", ViewModel.SaveAs)
                .AddButton("Open", ViewModel.Open)
                .AddButton("Save", ViewModel.Save)
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
            GUILayout.BeginVertical(GUILayout.Width(230));
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

            // Match the node preview's 200:120 aspect ratio and respect scrolling.
            Rect boxRect = GUILayoutUtility.GetRect(300f, 180f, GUILayout.Width(300f), GUILayout.Height(180f));
            EditorGUI.DrawRect(boxRect, BranchCamEditorTheme.PanelBackground);
            var actors = NodeManager.Instance.ActorsInScene;
            var primary = actors.FirstOrDefault(a => a.ActorID == previewActorId) ?? actors.FirstOrDefault();
            var opposite = actors.FirstOrDefault(a => a.ActorID == previewOppositeActorId && a != primary)
                ?? actors.FirstOrDefault(a => a != primary);
            ViewModel.PreviewRenderer.ComposePreviewImage(boxRect, ViewModel.CurrentShot, primary, opposite,
                overridePreviewSpacing ? distanceValue : (float?)null);
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
                var actors = NodeManager.Instance.ActorsInScene.ToList();
                if (actors.Count == 0)
                {
                    EditorGUILayout.HelpBox("Assign actor targets in the Start node to test previews.", MessageType.Info);
                    return;
                }
                int primaryIndex = Mathf.Max(0, actors.FindIndex(a => a.ActorID == previewActorId));
                int selectedPrimary = EditorGUILayout.Popup("Preview Actor", primaryIndex, actors.Select(a => a.ActorName).ToArray());
                if (previewActorId != actors[selectedPrimary].ActorID) overridePreviewSpacing = false;
                previewActorId = actors[selectedPrimary].ActorID;
                var primary = actors[selectedPrimary];
                var others = actors.Where(a => a != primary).ToList();
                if (shot.GoalType == CameraGoal.OverShoulder || shot.GoalType == CameraGoal.FrameShare)
                {
                    if (others.Count == 0)
                    {
                        EditorGUILayout.HelpBox("Assign a second actor target in the Start node.", MessageType.Info);
                        return;
                    }
                    int oppositeIndex = Mathf.Max(0, others.FindIndex(a => a.ActorID == previewOppositeActorId));
                    int selectedOpposite = EditorGUILayout.Popup("Opposite Actor", oppositeIndex, others.Select(a => a.ActorName).ToArray());
                    if (previewOppositeActorId != others[selectedOpposite].ActorID) overridePreviewSpacing = false;
                    previewOppositeActorId = others[selectedOpposite].ActorID;
                    if (!overridePreviewSpacing)
                        distanceValue = Vector3.ProjectOnPlane(
                            others[selectedOpposite].PreviewData.ActorPositionData.ActorPosition
                            - primary.PreviewData.ActorPositionData.ActorPosition, Vector3.up).magnitude;
                    EditorGUI.BeginChangeCheck();
                    float spacing = EditorGUILayout.Slider("Actor Spacing", distanceValue, 0.25f, 20f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        distanceValue = spacing;
                        overridePreviewSpacing = true;
                        Repaint();
                    }
                    if (GUILayout.Button("Use Scene Spacing", GUILayout.Width(160)))
                        overridePreviewSpacing = false;
                    EditorGUILayout.HelpBox("Test spacing moves preview actors only. Scene actors and node previews keep their configured spacing.", MessageType.None);
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
                    if (GUILayout.Button(shot.ShotName, GUILayout.ExpandWidth(true)))
                    {
                        ViewModel.CurrentShot = shot;
                    }

                    using (new EditorGUI.DisabledScope(!CameraShotsManager.Instance.CanRemoveShot(shot)))
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
