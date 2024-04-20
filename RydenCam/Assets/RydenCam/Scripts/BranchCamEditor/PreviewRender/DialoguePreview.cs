using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.BranchCamEditor.PreviewRender;
using RydenCam.Common;
using RydenCam.SequenceData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace RydenCam.BranchCamEditor.PreviewRender
{
    /// <summary>
    /// Sets up the dialogue variables in order to send to the PreviewCameraRenderUtil to render.
    /// </summary>

    public class DialoguePreview
    {
        PreviewCameraRenderUtil previewUtil;
        CameraCalculator cameraCalculator;
        Dictionary<Transform, GameObject[]> cachedChildrenWithMeshes = new Dictionary<Transform, GameObject[]>();


        public DialoguePreview()
        {
            previewUtil = new PreviewCameraRenderUtil();
            cameraCalculator = new CameraCalculator();
        }

        public void DrawPreviewWindows()
        {
            previewUtil.Initialize();

            var previewRender = previewUtil.PreviewRenderUtility;

            foreach (EditorDialogueNode dialogueNode in NodeManager.Instance.GetList().Where(x => x.TypeOfNode == NodeType.DialogueNode))
            {
                var focusTarget = GameObject.Find(dialogueNode.NodeConvodata.Actor.ActorName);
                var actorObjects = GetChildrenWithMeshes(focusTarget.transform.parent);
                var windowRect = new Rect(dialogueNode.windowRect.position.x + dialogueNode.windowRect.width, dialogueNode.windowRect.position.y,
                    dialogueNode.windowRect.width, dialogueNode.windowRect.height);

                if (dialogueNode.NodeConvodata.ShotConfig.GoalType == CameraGoal.Portrait)
                {
                    previewUtil.DrawPreview(actorObjects, windowRect, GetCamPose(dialogueNode), GetActorPose());
                }
                else
                {
                    //Temp: Do not render any unfinished shots types.
                    previewUtil.DrawBlankPreview(windowRect);
                }
            }
        }

        private GameObject[] GetChildrenWithMeshes(Transform actorParent)
        {
            if (cachedChildrenWithMeshes.TryGetValue(actorParent, out GameObject[] cachedObjects))
            {
                return cachedObjects;
            }
            else
            {
                var meshChildren = new List<GameObject>();

                // Define a local function for recursive traversal
                void FindMeshChildren(Transform parent)
                {
                    foreach (Transform child in parent)
                    {
                        // Check if the child has a MeshRenderer or SkinnedMeshRenderer
                        if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<SkinnedMeshRenderer>() != null)
                        {
                            meshChildren.Add(child.gameObject);
                        }

                        // Recursively search through all children
                        FindMeshChildren(child);
                    }
                }

                // Start recursive traversal from the actor's transform
                FindMeshChildren(actorParent.transform);

                cachedChildrenWithMeshes[actorParent] = meshChildren.ToArray();

                return meshChildren.ToArray();
            }
        }

        private Pose GetActorPose()
        {
            return new Pose();
            var actor = EditorController.Instance.ActorsInScene[0];

            return new Pose(actor.PreDefinedStartPosition.position, actor.PreDefinedStartPosition.rotation);
        }

        private Pose GetCamPose(EditorDialogueNode posNode)
        {
            cameraCalculator.SetSide(NodeManager.Instance.StartNode.CameraSide);
            return cameraCalculator.CalculatePlacement(posNode.NodeConvodata.ShotConfig);
        }

        public void CleanUp() => previewUtil.PreviewRenderUtility.Cleanup();
    }
}