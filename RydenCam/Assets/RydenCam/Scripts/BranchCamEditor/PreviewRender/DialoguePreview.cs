using Ink.Parsed;
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
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;

namespace RydenCam.BranchCamEditor.PreviewRender
{
    /// <summary>
    /// Sets up the render preview variables in order to send to the PreviewCameraRenderUtil to render.
    /// </summary>

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
        //TODO: remove nodes that are deleted.
        public Dictionary<EditorBaseNode, Texture2D> CachedTextures = new();
        public Dictionary<EditorBaseNode, (GameObject focusTarget, List<(Mesh mesh, Material mat)> meshMat)> CachedActor = new();
        CameraCalculator CameraCalculator = new();
        public static DialoguePreview CreateAndPopulateMeshes(EditorBaseNode[] nodes)
        {
            var dialoguePreview = new DialoguePreview();
            dialoguePreview.PopulateCachedMeshes(nodes);

            return dialoguePreview;
        }

        private DialoguePreview() { }

        void PopulateCachedMeshes(EditorBaseNode[] nodes)
        {
            foreach (var node in nodes)
            {
                if (node is not IPositionalNode dialogueNode) continue;
                if (CachedActor.ContainsKey(node)) continue;

                var focusTarget = GameObject.Find(dialogueNode.NodeConvodata.ShotConfig.actor);
                var objsToRender = GetChildrenWithMeshes(focusTarget.transform.parent);
                
                var meshMatList = new List<(Mesh, Material)>();

                foreach (var obj in objsToRender)
                {
                    var mesh = GetMesh(obj);
                    var mat = GetMaterial(obj);

                    meshMatList.Add((mesh, mat));
                }

                CachedActor[node] = (focusTarget, meshMatList);
            }
        }

        public void DrawPreviewWindows(EditorBaseNode[] nodes)
        {
            foreach (var node in nodes)
            {
                if (node is not IPositionalNode dialogueNode) continue;

                var windowRect = new Rect(node.windowRect.position.x + node.windowRect.width, node.windowRect.position.y,
                node.windowRect.width, node.windowRect.height);

                PreviewCameraRenderUtil newUtil = new();

                if (dialogueNode.NodeConvodata.ShotConfig.GoalType == CameraGoal.Portrait)
                {
                    newUtil.DrawSavePreview(windowRect, GetCamPose(node), GetActorPose(node), CachedActor[node].meshMat.ToArray());
                    CachedTextures[node] = newUtil.CachedRenderTexture;
                }

                newUtil.Dispose();
            }
        }

        Pose GetActorPose(EditorBaseNode node) => new Pose(Vector3.zero, GetRotation(CachedActor[node].focusTarget.transform.position));

        Quaternion GetRotation(Vector3 pos)
        {
            Vector3 midPoint = CameraCalculator.CalculateMidPoint();
            Vector3 direction = pos - midPoint;

            direction.y = 0;

            return Quaternion.LookRotation(direction);
        }

        Pose GetCamPose(EditorBaseNode node) 
        {
            if (node is not IPositionalNode posNode) throw new ArgumentException($"Error: {node} is not {typeof(IPositionalNode)}");

            var actorPosition = CachedActor[node].focusTarget.transform.position;

            var initialCamPose = CameraCalculator.CalculatePlacement(posNode.NodeConvodata.ShotConfig);

            // Calculate the relative position to the actor, ignoring y-axis differences.
            Vector3 relativePosition = new Vector3(actorPosition.x - initialCamPose.position.x, 0, actorPosition.z - initialCamPose.position.z);

            // Adjust the camera's position to maintain the initial y-position.
            Vector3 finalPosition = relativePosition + new Vector3(0, initialCamPose.position.y, 0);

            // Rotate the camera to face the actor.
            Quaternion finalRotation = Quaternion.Euler(initialCamPose.rotation.eulerAngles + new Vector3(0, 180f, 0));

            return new Pose(finalPosition, finalRotation);
        }

        public void DrawCachedWindows(EditorBaseNode[] nodes) 
        {
            foreach (var node in nodes)
            {
                if (node is not IPositionalNode) continue;
                if (CachedTextures[node] == null) continue;

                //For some reason DrawPreviewWindow sometimes draws a blank texture and that gets cached. This prevents a blank texture from being drawn.
                if (CachedTextures[node].IsTextureEmpty())
                {
                    Debug.Log("Empty Texture found redrawing.");
                    DrawPreviewWindows(nodes);
                    return;
                }

                var windowRect = new Rect(node.windowRect.position.x + node.windowRect.width, node.windowRect.position.y,
                node.windowRect.width, node.windowRect.height);
                GUI.DrawTexture(windowRect, CachedTextures[node]);
            }
        }

        private GameObject[] GetChildrenWithMeshes(Transform actorParent)
        {
            var meshChildren = new List<GameObject>();

            // Define a local function for recursive traversal
            void FindMeshChildren(Transform parent)
            {
                foreach (Transform child in parent)
                {
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

            return meshChildren.ToArray();
        }

        private Material GetMaterial(GameObject obj)
        {
            if (obj.GetComponent<SkinnedMeshRenderer>() != null) return obj.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
            if (obj.GetComponent<Renderer>() != null) return obj.GetComponent<Renderer>().sharedMaterial;

            BranchLog.Log("No Renderer or Skinned Renderer found on " + obj);
            return null;
        }
        private Mesh GetMesh(GameObject obj)
        {
            if (obj.GetComponent<SkinnedMeshRenderer>() != null) return GetSkinnedMesh(obj);
            if (obj.GetComponent<MeshFilter>() != null) return obj.GetComponent<MeshFilter>().sharedMesh;

            throw new NullReferenceException($"No Mesh or Skinned Mesh found on {obj}");
        }

        private Mesh GetSkinnedMesh(GameObject obj)
        {
            var skinnedRenderer = obj.GetComponent<SkinnedMeshRenderer>();

            // Create a new Mesh and bake it
            Mesh newMesh = new Mesh();
            skinnedRenderer.BakeMesh(newMesh);

            return newMesh;
        }
    }
}