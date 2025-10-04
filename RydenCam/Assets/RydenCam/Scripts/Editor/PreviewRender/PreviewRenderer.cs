using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System.Collections.Generic;
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
                    _prevRenderUtility.camera.farClipPlane = 20;
                }

                return _prevRenderUtility;
            }
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

        public void RenderPreview(Rect windowRect, Pose camPose, CamShotConfig shot)
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
        private void RenderCustomPreview(Rect windowRect, Pose camPose, CamShotConfig shot)
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

            foreach (var actor in NodeManager.Instance.ActorsInScene)
            {
                foreach (var meshMatScale in actor.PreviewData.MeshMatScale)
                {
                    if (meshMatScale.Mesh == null)
                        continue;

                    var matrix = Matrix4x4.TRS(
                        actor.PreviewData.MeshOriginPoint,
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
        }
    }
}