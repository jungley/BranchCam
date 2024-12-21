using RydenCam.BranchCamEditor.BranchCam;
using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

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
        public Dictionary<Node, Texture2D> CachedTextures = new();
        public Dictionary<Node, (GameObject focusTarget, List<(Mesh mesh, Material mat)> meshMat)> CachedActor = new();
        CameraCalculator CameraCalculator = new();
        public static DialoguePreview CreateAndPopulateMeshes(Node[] nodes)
        {
            var dialoguePreview = new DialoguePreview();
            dialoguePreview.PopulateCachedMeshes(nodes);
            //BranchCamEditor.OnNodePropertyChanged += dialoguePreview.CreateActorCache;
            return dialoguePreview;
        }

        private DialoguePreview() { }

        void PopulateCachedMeshes(Node[] nodes)
        {
            foreach (var node in nodes)
            {
                CreateActorCache(node);
            }
        }

        void CreateActorCache(Node node)
        {
            if (node is not ITalkable dialogueNode) return;

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

        public void DrawPreviewWindows(List<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if (node is not ITalkable dialogueNode) continue;

                var windowRect = new Rect(node.EditorPosition.x + node.NodeWidth, node.EditorPosition.y,
                node.NodeWidth, node.NodeHeight);

                PreviewCameraRenderUtil newUtil = new();

                if (dialogueNode.NodeConvodata.ShotConfig.GoalType == CameraGoal.Portrait)
                {
                    if (!CachedActor.ContainsKey(node)) CreateActorCache(node);

                    newUtil.DrawSavePreview(windowRect, GetCamPose(node), GetActorPose(node), CachedActor[node].meshMat.ToArray());
                    CachedTextures[node] = newUtil.CachedRenderTexture;
                }

                newUtil.Dispose();
            }
        }

        Pose GetActorPose(Node node) => new Pose(Vector3.zero, GetRotation(CachedActor[node].focusTarget.transform.position));

        Quaternion GetRotation(Vector3 pos)
        {
            Vector3 midPoint = CameraCalculator.CalculateMidPoint();
            Vector3 direction = pos - midPoint;

            direction.y = 0;

            return Quaternion.LookRotation(direction);
        }

        Pose GetCamPose(Node node)
        {
            if (node is not ITalkable posNode) throw new ArgumentException($"Error: {node} is not {typeof(ITalkable)}");

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

        public void DrawCachedWindows(List<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if (node is not ITalkable) continue;

                CachedTextures.TryGetValue(node, out Texture2D texture);
                if (texture == null) continue;

                //For some reason DrawPreviewWindow sometimes draws a blank texture and that gets cached. This prevents a blank texture from being drawn.

                if (texture.IsTextureEmpty())
                {
                    DrawPreviewWindows(nodes);
                    return;
                }

                var windowRect = new Rect(node.EditorPosition.x + node.NodeWidth, node.EditorPosition.y,
                node.NodeWidth, node.NodeHeight);

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