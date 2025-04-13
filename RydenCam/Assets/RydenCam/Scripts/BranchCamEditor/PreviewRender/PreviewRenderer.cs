using Ink.Parsed;
using System.Collections;
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
                    _prevRenderUtility.camera.farClipPlane = 20;
                }

                return _prevRenderUtility;
            }
        }

        public void RenderPreview(Rect windowRect, Pose camPose, List<PreviewActorData> actorsToRender)
        {
            previewRenderUtility.BeginPreview(windowRect, GUIStyle.none);

            previewRenderUtility.camera.transform.SetPositionAndRotation(camPose.position, camPose.rotation);


            foreach (var actor in actorsToRender)
            {

                //Vector3 parentScale = actor.ActorPositionData

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