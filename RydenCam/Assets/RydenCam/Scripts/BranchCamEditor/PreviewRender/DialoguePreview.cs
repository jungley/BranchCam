using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.Common;
using UnityEngine;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender;

namespace RydenCam.BranchCamEditor.PreviewRender
{
    public class DialoguePreview<N> where N : Node, ITalkable
    {
        private CameraPoseCalculator cameraPoseCalculator;
        private ActorMeshManager actorMeshManager;
        private PreviewRenderer previewRenderer;

        private N node;

        public DialoguePreview(N node)
        {
            this.node = node;
            Initailize();
        }

        public void Initailize()
        {
            cameraPoseCalculator = new CameraPoseCalculator();
            actorMeshManager = new ActorMeshManager(node.NodeConvodata.Actor.ActorGO);
            previewRenderer = new PreviewRenderer();
        }

        public void UpdateShotRender()
        {
            //Re Initialize the preview renderer
            Initailize();
        }


        public void DrawPreviewWindow()
        {
            var windowRect = new Rect(node.EditorPosition.x + node.NodeWidth, node.EditorPosition.y, node.NodeWidth, 120);
            /* TODO use Cached Image render result
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
            if (node.NodeConvodata.ShotConfig.GoalType == CameraGoal.Portrait)
            {
                Pose camPose = cameraPoseCalculator.CalculateCameraPose(node.NodeConvodata.ShotConfig, actorMeshManager.CachedActorMesh.FocusTarget.transform);
                Pose actorPose = GetActorPreviewPositionData();

                previewRenderer.RenderPreview(windowRect, camPose, actorPose, actorMeshManager.CachedActorMesh);
            }
        }

        private Pose GetActorPreviewPositionData()
        {
            Vector3 pos = actorMeshManager.CachedActorMesh.FocusTarget.transform.position;

            Vector3 midPoint = cameraPoseCalculator.CalculateMidPreviewPoint();
            Vector3 direction = pos - midPoint;

            direction.y = 0;

            // Check if the direction vector is valid (non-zero), prevents warning message being spammed to console.
            Quaternion rotation = (direction.sqrMagnitude > Mathf.Epsilon) ? Quaternion.LookRotation(direction) : Quaternion.identity;

            return new Pose(Vector3.zero, rotation);

        }
    }
}