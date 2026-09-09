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


        private int configurationWidth = 240;

        private void OnGUI()
        {
            if (ViewModel == null || ribbonRenderer == null)
                InitializeWindowState();

            ribbonRenderer.Draw(position.width);
            if (CameraShotsManager.Instance.CameraShots.Count == 0) return;

            windowScrollPos = EditorGUILayout.BeginScrollView(windowScrollPos);
            GUILayout.Space(8);
            bool wideLayout = position.width >= 940;
            configurationWidth = wideLayout ? Mathf.Max(240, (int)position.width - 600) : Mathf.Max(240, (int)position.width - 270);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(8);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(220)))
                    DrawCameraShotListSection();
                GUILayout.Space(8);
                if (wideLayout)
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(320)))
                    {
                        DrawShotPreviewSection();
                        DrawBottomConfigurationPanel();
                    }
                    GUILayout.Space(8);
                }
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(240), GUILayout.ExpandWidth(true)))
                    DrawShotConfigurationSection();
                GUILayout.Space(8);
            }
            if (!wideLayout)
            {
                GUILayout.Space(8);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    DrawShotPreviewSection();
                    DrawBottomConfigurationPanel();
                }
            }
            GUILayout.Space(8);
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

            GUILayout.Label("Shot Preview", largeBoldLabel);

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
            GUILayout.Label("Shot Settings", EditorStyles.boldLabel);
            GUILayout.Space(8);
            var shot = ViewModel?.CurrentShot;
            if (shot == null) return;

            EditorGUILayout.LabelField("Shot Name");
            GUI.SetNextControlName("ShotNameField");
            using (new EditorGUI.DisabledScope(shot.IsDefault))
                shot.ShotName = EditorGUILayout.TextField(shot.ShotName, GUILayout.Height(24));
            if (shot.IsDefault)
                EditorGUILayout.LabelField("Built-in shot", EditorStyles.miniLabel);
            GUILayout.Space(8);

            EditorGUILayout.LabelField("Shot Type");
            bool singleActor = NodeManager.Instance.ActorsInScene.Count == 1;
            CameraGoal[] allowedGoals = { CameraGoal.Portrait, CameraGoal.Custom };
            shot.GoalType = EnumPopupExtensions.EnumPopup(shot.GoalType, singleActor, configurationWidth, allowedGoals);
            GUILayout.Space(8);

            if (shot.GoalType == CameraGoal.Custom)
            {
                DrawCustomShotConfiguration(shot);
                return;
            }
            EditorGUILayout.LabelField("Camera Distance");
            shot.GoalDistance = (CameraDistance)EditorGUILayout.EnumPopup(shot.GoalDistance, GUILayout.Height(24));
            GUILayout.Space(8);
            EditorGUILayout.LabelField("Camera Height");
            shot.GoalAngle = (CameraAngle)EditorGUILayout.EnumPopup(shot.GoalAngle, GUILayout.Height(24));
        }
        private void DrawCustomShotConfiguration(CameraShotConfiguration shot)
        {
            EditorGUILayout.LabelField("Camera Position");
            shot.GlobalCustomCamPos = EditorGUILayout.Vector3Field(GUIContent.none, shot.GlobalCustomCamPos, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Camera Rotation");
            Vector3 euler = EditorGUILayout.Vector3Field(GUIContent.none, shot.GlobalCustomCamRot.eulerAngles, GUILayout.ExpandWidth(true));
            shot.GlobalCustomCamRot = Quaternion.Euler(euler);

            if (GUILayout.Button("Capture Scene View", GUILayout.ExpandWidth(true), GUILayout.Height(28)))
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

            if (GUILayout.Button("Use Entered Pose", GUILayout.ExpandWidth(true)))
                shot.IsCustomSet = true;

            using (new EditorGUI.DisabledScope(!shot.IsCustomSet))
            {
                if (GUILayout.Button("Clear Custom Pose", GUILayout.ExpandWidth(true)))
                {
                    shot.GlobalCustomCamPos = Vector3.zero;
                    shot.GlobalCustomCamRot = Quaternion.identity;
                    shot.IsCustomSet = false;
                }
            }

            shot.TogglePreviewRenderSceneView = EditorGUILayout.ToggleLeft("Preview in Scene View", shot.TogglePreviewRenderSceneView, GUILayout.ExpandWidth(true));
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
            EditorGUILayout.Space(4f);
            GUIStyle sectionLabel = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = BranchCamEditorTheme.FontTitle,
                normal = { textColor = BranchCamEditorTheme.TextPrimary },
                alignment = TextAnchor.MiddleLeft
            };
            GUILayout.Label("Camera Shots", sectionLabel);

            float scrollViewHeight = Mathf.Clamp(position.height - 190f, 150f, 320f);

            // 🔹 Vertical-only scroll view (horizontal scrolling disabled)
            scrollPos = GUILayout.BeginScrollView(
                scrollPos,
                alwaysShowHorizontal: false,
                alwaysShowVertical: false,
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
                    if (GUILayout.Toggle(ViewModel.CurrentShot == shot, shot.ShotName, "Button", GUILayout.ExpandWidth(true), GUILayout.Height(28)))
                    {
                        ViewModel.CurrentShot = shot;
                    }

                    using (new EditorGUI.DisabledScope(!CameraShotsManager.Instance.CanRemoveShot(shot)))
                    {
                        if (GUILayout.Button(new GUIContent("X", "Delete this custom shot"), GUILayout.Width(28), GUILayout.Height(28)))
                            shotsToRemove.Add(shot);
                    }

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();

            // Add button
            if (GUILayout.Button("+ Add New Shot", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
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
