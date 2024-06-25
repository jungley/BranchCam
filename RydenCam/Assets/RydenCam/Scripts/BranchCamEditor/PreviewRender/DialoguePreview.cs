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

        //Every node should have a separate PreviewCameraRenderUtil
        private static Dictionary<string, PreviewCameraRenderUtil> _previewWindowLookUp { get; set; }
        public static Dictionary<string, PreviewCameraRenderUtil> PreviewWindowLookUp
        {
            get
            {
                if(_previewWindowLookUp == null)
                {
                    _previewWindowLookUp = new Dictionary<string, PreviewCameraRenderUtil>();
                }
                return _previewWindowLookUp;
            }
            set
            {
                _previewWindowLookUp = value;
            }
        }
      
        Dictionary<Transform, GameObject[]> cachedChildrenWithMeshes = new Dictionary<Transform, GameObject[]>();

        public DialoguePreview()
        {
        }

        public void DrawPreviewWindow(EditorBaseNode node)
        {
            PreviewCameraRenderUtil previewindow;

            if (node is EditorDialogueNode)
            {
                var dialogudeNode = node as EditorDialogueNode;

                //TODO: Remove this
                //for the sake of the tool running I've kept it in here
                PreviewWindowLookUp.Clear();

                if (PreviewWindowLookUp.TryGetValue(dialogudeNode.node_id, out PreviewCameraRenderUtil util))
                {
                    previewindow = util;
                }
                else
                {
                    PreviewCameraRenderUtil newUtil = new PreviewCameraRenderUtil();
                    newUtil.Initialize();
                    previewindow = newUtil;
                    PreviewWindowLookUp.Add(node.node_id, newUtil);

                }

                var focusTarget = GameObject.Find(dialogudeNode.NodeConvodata.Actor.ActorName);
                var actorObjects = GetChildrenWithMeshes(focusTarget.transform.parent);
                var windowRect = new Rect(node.windowRect.position.x + node.windowRect.width, node.windowRect.position.y,
                    node.windowRect.width, node.windowRect.height);

                if (dialogudeNode.NodeConvodata.ShotConfig.GoalType == CameraGoal.Portrait)
                {

                    previewindow.DrawPreview(actorObjects, windowRect, dialogudeNode);
                }

                previewindow.PreviewRenderUtility.Cleanup();
            }
            else
            {
                //Temp: Do not render any unfinished shots types.
                previewindow = new PreviewCameraRenderUtil();
                previewindow.Initialize();
                previewindow.DrawBlankPreview(node.windowRect);
                previewindow.PreviewRenderUtility.Cleanup();
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
    }
}