using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Managers;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
            if (EditorSettingsManager.Instance.SettingsData.IsNodePreviewEnabled)
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

            if (EditorSettingsManager.Instance.SettingsData.IsCornerPreviewEnabled)
            {
                float panX = NodeGraphEditorWindow.panX;
                float panY = NodeGraphEditorWindow.panY;

                var window = EditorWindow.GetWindow<NodeGraphEditorWindow>();
                float windowWidth = window.position.width;
                float windowHeight = window.position.height;

                var cornerRect = new Rect(windowWidth - (panX + 450) - 30, 80 - panY, 450, 300);
                
                if (node == NodeManager.Instance.ActiveNode)
                {
                    ComposePreviewImage(cornerRect);
                }
                else if(NodeManager.Instance.ActiveNode == null || !(NodeManager.Instance.ActiveNode is ITalkable))
                {
                    GUI.Box(cornerRect, "Select a node with a Shot Composition");
                }        
            }
        }

        public void ComposePreviewImage(Rect windowRect)
        {
            CamShotConfig shot = node.NodeConvodata.ShotConfig;

            Pose camPose = cameraWrapper.CalculateCameraShot(shot);

            previewRenderer.RenderPreview(windowRect, camPose, ActorDatas, shot);

        }
    }
}