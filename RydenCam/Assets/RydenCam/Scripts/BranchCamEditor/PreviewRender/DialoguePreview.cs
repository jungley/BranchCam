using Assets.RydenCam.Scripts.BranchCamCC;
using UnityEngine;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender;
using System.Collections.Generic;
using RydenCam.BranchCamEditor.BranchCam;

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

            SetupPreviewSceneData.CalculateActorsinPreviewSpace();

            cameraWrapper = new PreviewCameraWrapper(ActorDatas);
        }

        public void UpdateShotRender()
        {
            //Re Initialize the preview renderer
            Initailize();
        }


        public void DrawPreviewWindow()
        {
            var windowRect = new Rect(node.EditorPosition.x + node.NodeWidth, node.EditorPosition.y, node.NodeWidth, 120);
            /* TODO use Cached Image render result */
            
            /*
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

            //RS TODO Preview should handle local custom
            if (shot.IsCustomSet) return;

            Pose camPose = cameraWrapper.CalculateCameraShot(shot);

            previewRenderer.RenderPreview(windowRect, camPose, ActorDatas);

        }
    }
}