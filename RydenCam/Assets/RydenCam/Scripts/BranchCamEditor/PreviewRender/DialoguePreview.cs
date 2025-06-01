using Assets.RydenCam.Scripts.BranchCamCC;
using UnityEngine;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender;
using System.Collections.Generic;
using RydenCam.BranchCamEditor.BranchCam;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup;

namespace RydenCam.BranchCamEditor.PreviewRender
{
    public class DialoguePreview<N> where N : Node, ITalkable
    {

        private PreviewCameraWrapper cameraWrapper;
        private PreviewRenderer previewRenderer;

        private N node;

        public List<PreviewActorData> ActorDatas => SetupPreviewSceneData.PreviewActorDatas;

        public DialoguePreview(N node)
        {
            this.node = node;
            Initailize();
        }


        public void Initailize()
        {
            previewRenderer = new PreviewRenderer();

            SetupPreviewSceneData.CalculateActorsInPreviewSpace();

            cameraWrapper = new PreviewCameraWrapper(ActorDatas);

            previewRenderer.CachedRenderTexture = null;
        }

        public void UpdateShotRender()
        {
            //Re Initialize the preview renderer
            Initailize();
        }


        public void DrawPreviewWindow()
        {
            var windowRect = new Rect(node.EditorPosition.x + node.NodeWidth, node.EditorPosition.y, node.NodeWidth, 120);
            /* TODO: Implement Render Caching, weirdness with Unity Editor GUI and RenderTextures
             * If performance hit is not too bad, might not do caching
             * The workaround might be literally saving the Texture to a file
            if (previewRenderer.CachedRenderTexture != null)
            {
                GUI.DrawTexture(windowRect, previewRenderer.CachedRenderTexture);
                return;
            } 
            
            */

            ComposePreviewImage(windowRect);
            
        }

        public void ComposePreviewImage(Rect windowRect)
        {
            CamShotConfig shot = node.NodeConvodata.ShotConfig;

            Pose camPose = cameraWrapper.CalculateCameraShot(shot);

            previewRenderer.RenderPreview(windowRect, camPose, ActorDatas ,shot);

        }
    }
}