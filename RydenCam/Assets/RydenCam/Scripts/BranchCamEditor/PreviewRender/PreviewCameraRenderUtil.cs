using UnityEditor;
using UnityEngine;
using RydenCam.Common;

namespace RydenCam.BranchCamEditor.PreviewRender
{
    /// <summary>
    /// Draws a rect and renders objects into the preview window.
    /// </summary>

    public class PreviewCameraRenderUtil
    {
        private PreviewRenderUtility _previewRenderUtility { get; set; }
        public PreviewRenderUtility PreviewRenderUtility
        {
            get
            {
                if(_previewRenderUtility == null || _previewRenderUtility.camera == null)
                {
                    _previewRenderUtility = new PreviewRenderUtility();
                    _previewRenderUtility.camera.fieldOfView = 40;
                    _previewRenderUtility.camera.nearClipPlane = 0.01f;
                    _previewRenderUtility.camera.farClipPlane = 20;
                }

                return _previewRenderUtility;   
            }
        }
        public Texture2D CachedRenderTexture { get; set; }
        public PreviewCameraRenderUtil() { }

        internal void DrawSavePreview(Rect windowRect, Pose camPose, Pose objPose, (Mesh mesh, Material mat)[] meshMats)
        {
            // Pose actorPose = new Pose(Vector3.zero, GetRotation(actorPosition));

            SetCameraPose(camPose);

            PreviewRenderUtility.BeginStaticPreview(windowRect);

            foreach (var meshMat in meshMats)
            {
                DrawCustomObjectPreview(meshMat.mesh, meshMat.mat, objPose);
            }

            PreviewRenderUtility.Render();
            Texture2D previewRenderTexture = PreviewRenderUtility.EndStaticPreview();
            GUI.DrawTexture(windowRect, previewRenderTexture);
            CachedRenderTexture = previewRenderTexture;
        }

        private void DrawCustomObjectPreview(Mesh meshToRender, Material material, Pose actorPose)
        {
            if (meshToRender == null)
            {
                BranchLog.Log("Mesh to render is null.");
                return;
            }

            //Issue: DrawMesh does NOT set the rotation based on the parameter rather it forces the object to LookAt the direction.
            //This means rotation in scene view affects renderview when it should not.
            Matrix4x4 customMatrix = Matrix4x4.TRS(actorPose.position, actorPose.rotation, Vector3.one);
            // Use the copied mesh in the preview
            PreviewRenderUtility.DrawMesh(meshToRender, customMatrix, material, 0);
        }

        void SetCameraPose(Pose camPose)
        {
            // Set the camera's position and rotation.
            PreviewRenderUtility.camera.transform.SetPositionAndRotation(camPose.position, camPose.rotation);
        }


        public void Dispose()
        {
            if (PreviewRenderUtility != null)
            {
                PreviewRenderUtility.Cleanup();
            }
        }

    }
}
