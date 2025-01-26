using System.Collections;
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

        public void RenderPreview(Rect windowRect, Pose camPose, Pose actorPose, ActorMeshPreviewData actorMeshData)
        {
            previewRenderUtility.BeginPreview(windowRect, GUIStyle.none);

            previewRenderUtility.camera.transform.SetPositionAndRotation(camPose.position, camPose.rotation);

            foreach (var meshMat in actorMeshData.MeshMat)
            {
                if (meshMat.Mesh == null) continue;

                // Use custom matrix for actor pose
                Matrix4x4 customMatrix = Matrix4x4.TRS(actorPose.position, actorPose.rotation, Vector3.one);
                previewRenderUtility.DrawMesh(meshMat.Mesh, customMatrix, meshMat.Mat, 0);
            }
            
            previewRenderUtility.Render();

            CachedRenderTexture = previewRenderUtility.EndPreview();

            GUI.DrawTexture(windowRect, CachedRenderTexture);

            previewRenderUtility.Cleanup();
        }
    }
}