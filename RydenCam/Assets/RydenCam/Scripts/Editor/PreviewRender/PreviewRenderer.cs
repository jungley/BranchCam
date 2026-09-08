using Assets.RydenCam.Scripts.BranchCamEditor.Camera;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using RydenCam.SequenceData;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender
{
    public class PreviewRenderer
    {
        public Texture CachedRenderTexture { get; set; }

        private PreviewRenderUtility _prevRenderUtility { get; set; }
        private PreviewRenderUtility previewRenderUtility
        {
            get
            {
                if (_prevRenderUtility == null || _prevRenderUtility.camera == null)
                {
                    _prevRenderUtility = new PreviewRenderUtility();
                    _prevRenderUtility.camera.fieldOfView = 40;
                    _prevRenderUtility.camera.nearClipPlane = 0.01f;
                    _prevRenderUtility.camera.farClipPlane = 200;
                }

                return _prevRenderUtility;
            }
        }

        private CameraCalculator cameraCalculator { get; }

        public PreviewRenderer()
        {
            cameraCalculator = new CameraCalculator();
        }

        public static Texture2D RenderGlobalSceneFromPosition(Vector3 camPosition, Quaternion camRotation, int width, int height)
        {
            // Create temporary camera
            GameObject tempCamGO = new GameObject("TempCamera");
            UnityEngine.Camera tempCam = tempCamGO.AddComponent<UnityEngine.Camera>(); // Explicitly specify UnityEngine.Camera

            tempCam.enabled = false; // Prevent it from interfering with scene rendering

            // Set position & rotation
            tempCam.transform.position = camPosition;
            tempCam.transform.rotation = camRotation;

            // Create RenderTexture
            RenderTexture rt = new RenderTexture(width, height, 24);
            tempCam.targetTexture = rt;

            // Render
            tempCam.Render();

            // Read pixels into Texture2D
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            // Clean up
            RenderTexture.active = null;
            tempCam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tempCamGO);

            return tex;
        }

        private List<ActorInfo> renderActors = new List<ActorInfo>();
        private readonly Dictionary<string, Vector3> renderOffsets = new Dictionary<string, Vector3>();

        public void ComposePreviewImage(Rect windowRect, CameraShotConfiguration shot, ActorInfo primary,
            ActorInfo opposite = null, float? spacing = null)
        {
            if (Event.current.type != EventType.Repaint || shot == null) return;
            var actors = NodeManager.Instance.ActorsInScene.ToList();
            primary = actors.FirstOrDefault(a => a.ActorID == primary?.ActorID) ?? actors.FirstOrDefault();
            opposite = actors.FirstOrDefault(a => a.ActorID == opposite?.ActorID && a != primary)
                ?? actors.FirstOrDefault(a => a != primary);
            bool pair = shot.GoalType == CameraGoal.OverShoulder || shot.GoalType == CameraGoal.FrameShare;
            if (primary?.PreviewData?.ActorPositionData == null || (pair && opposite == null))
            {
                GUI.Label(windowRect, pair ? "Assign two actor targets to preview this shot." : "Assign an actor target to preview.");
                return;
            }
            renderActors = shot.GoalType == CameraGoal.Custom ? actors
                : actors.Where(a => a == primary || (pair && a == opposite)).ToList();
            renderOffsets.Clear();
            var primaryPose = primary.PreviewData.ActorPositionData;
            var oppositePose = pair ? opposite.PreviewData.ActorPositionData : null;
            if (pair && spacing.HasValue)
            {
                Vector3 direction = oppositePose.ActorPosition - primaryPose.ActorPosition;
                direction.y = 0;
                if (direction.sqrMagnitude < 0.0001f) direction = Vector3.forward;
                Vector3 newPosition = primaryPose.ActorPosition + direction.normalized * spacing.Value;
                newPosition.y = oppositePose.ActorPosition.y;
                renderOffsets[opposite.ActorID] = newPosition - oppositePose.ActorPosition;
                oppositePose = new ActorPositionData {
                    ActorPosition = newPosition, ActorRotation = oppositePose.ActorRotation,
                    ForwardN = oppositePose.ForwardN
                };
            }
            cameraCalculator.PreviewActorPositions = renderActors.Select(a =>
                a.PreviewData.ActorPositionData.ActorPosition + GetRenderOffset(a)).ToList();
            Pose pose = cameraCalculator.CalculatePlacement(shot, primaryPose, oppositePose);
            RenderPreview(windowRect, pose, shot);
        }

        private Vector3 GetRenderOffset(ActorInfo actor) =>
            renderOffsets.TryGetValue(actor.ActorID, out var offset) ? offset : Vector3.zero;
        private void RenderPreview(Rect windowRect, Pose camPose, CameraShotConfiguration shot)
        {
            if (shot.GoalType == CameraGoal.Custom)
            {
                RenderCustomPreview(windowRect, camPose, shot);
            }
            else
            {
                RenderStandardPreview(windowRect, camPose.position, camPose.rotation);
            }
        }

        // Handles rendering for custom camera shots
        private void RenderCustomPreview(Rect windowRect, Pose camPose, CameraShotConfiguration shot)
        {
            // Skip if custom camera config isn't set
            if (!shot.IsCustomSet)
                return;

            // Render scene view if toggle is enabled
            if (shot.TogglePreviewRenderSceneView)
            {
                CachedRenderTexture = RenderGlobalSceneFromPosition(
                    camPose.position,
                    camPose.rotation,
                    (int)windowRect.width,
                    (int)windowRect.height
                );
                GUI.DrawTexture(windowRect, CachedRenderTexture);
            }
            else
            {
                // Use manually set custom camera position/rotation
                RenderWithPreviewUtility(windowRect, shot.GlobalCustomCamPos, shot.GlobalCustomCamRot);
            }
        }

        // Handles rendering for non-custom (standard) camera shots
        private void RenderStandardPreview(Rect windowRect, Vector3 camPos, Quaternion camRot)
        {
            RenderWithPreviewUtility(windowRect, camPos, camRot);
        }

        // Shared render logic
        private void RenderWithPreviewUtility(Rect windowRect, Vector3 cameraPosition, Quaternion cameraRotation)
        {
            previewRenderUtility.BeginPreview(windowRect, GUIStyle.none);
            previewRenderUtility.camera.transform.SetPositionAndRotation(cameraPosition, cameraRotation);

            foreach (var actor in renderActors)
            {
                if (actor?.PreviewData?.MeshMatScale == null) continue;

                foreach (var meshMatScale in actor.PreviewData.MeshMatScale)
                {
                    if (meshMatScale.Mesh == null || meshMatScale.Mat == null)
                        continue;

                    var matrix = Matrix4x4.TRS(
                        actor.PreviewData.MeshOriginPoint + GetRenderOffset(actor),
                        actor.PreviewData.ActorPositionData.ActorRotation,
                        Vector3.one // Replace with meshMatScale.Scale if needed
                    );

                    previewRenderUtility.DrawMesh(meshMatScale.Mesh, matrix, meshMatScale.Mat, 0);
                }
            }

            previewRenderUtility.Render();
            CachedRenderTexture = previewRenderUtility.EndPreview();
            GUI.DrawTexture(windowRect, CachedRenderTexture);
            previewRenderUtility.Cleanup();
            _prevRenderUtility = null;
        }
    }
}
