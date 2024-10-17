using System.Collections.Generic;
using UnityEngine;

namespace RydenCam.BranchCamEditor.PreviewRender
{
    /// <summary>
    /// Sets up the dialogue variables in order to send to the PreviewCameraRenderUtil to render.
    /// </summary>
    /// 

    public class DialoguePreview
    {

        static Dictionary<string, PreviewCameraRenderUtil> PreviewRenderMap { get; set; } = new Dictionary<string, PreviewCameraRenderUtil>();

        Texture2D blankTexture;
        Texture2D BlankTexture
        {
            get
            {
                if (blankTexture == null)
                {
                    var previewTexture = new Texture2D(1, 1);
                    previewTexture.SetPixel(0, 0, Color.black);
                    previewTexture.Apply();
                    blankTexture = previewTexture;
                }
                return blankTexture;
            }
        }
        /*
        public void DrawPreviewWindow(Node node)
        {

            var windowRect = new Rect(node.windowRect.position.x + node.windowRect.width, node.windowRect.position.y,
                node.windowRect.width, node.windowRect.height);

            if (node is ITalkable)
            {
                var dialogueNode = node as ITalkable;

                //PreviewRenderMap.TryGetValue(node.node_id, out PreviewCameraRenderUtil previewRender);

                PreviewCameraRenderUtil newUtil = new PreviewCameraRenderUtil(dialogueNode.NodeConvodata.ShotConfig);

                    if (dialogueNode.NodeConvodata.ShotConfig.GoalType == CameraGoal.Portrait)
                    {
                        newUtil.DrawSavePreview(windowRect, dialogueNode);
                        //newUtil.PreviewRenderUtility.Cleanup();
                        //PreviewRenderMap.Add(node.node_id, newUtil);
                    }
                    newUtil.PreviewRenderUtility.Cleanup();

            }
        }
        */

    }
}
