using RydenCam.BranchCamEditor.BranchCam;
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


        public void RenderPreview(Rect windowRect, Pose camPose, List<PreviewActorData> actorsToRender, CamShotConfig shot)
        {
            if (shot.GoalType == CameraGoal.Custom)
            {
                if (!shot.IsCustomSet) return;

                CachedRenderTexture = RenderGlobalSceneFromPosition(camPose.position, camPose.rotation, (int)windowRect.width, (int)windowRect.height);
                
                GUI.DrawTexture(windowRect, CachedRenderTexture);
            }
            else
            {
                previewRenderUtility.BeginPreview(windowRect, GUIStyle.none);

                previewRenderUtility.camera.transform.SetPositionAndRotation(camPose.position, camPose.rotation);


                foreach (var actor in actorsToRender)
                {

                    foreach (var meshMatScale in actor.MeshMatScale)
                    {
                        if (meshMatScale.Mesh == null) continue;

                        // Use custom matrix for actor pose
                        Matrix4x4 customMatrix = Matrix4x4.TRS(actor.ActorPositionData.MeshOriginPoint, actor.ActorPositionData.ActorRotation, Vector3.one);//meshMatScale.Scale);
                        previewRenderUtility.DrawMesh(meshMatScale.Mesh, customMatrix, meshMatScale.Mat, 0);
                    }
                }

                previewRenderUtility.Render();

                CachedRenderTexture = previewRenderUtility.EndPreview();

                GUI.DrawTexture(windowRect, CachedRenderTexture);

                previewRenderUtility.Cleanup();
            }
        }
    }
}