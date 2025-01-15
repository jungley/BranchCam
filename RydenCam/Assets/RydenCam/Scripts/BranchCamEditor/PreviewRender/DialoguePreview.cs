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
    /// 


    public class DialoguePreview
    {
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

        public class ActorMeshData
        {
            public GameObject FocusTarget { get; set; }
            public List<(Mesh Mesh, Material Mat)> MeshMat { get; set; }

            public ActorMeshData(GameObject focusTarget, List<(Mesh, Material)> meshMat)
            {
                FocusTarget = focusTarget;
                MeshMat = meshMat;
            }
        }

        private PreviewCameraRenderUtil PreviewRender { get; set; }

        public ActorMeshData CachedActorMesh { get; set; }

        private CameraCalculator CameraCalculator { get; set; }

        private Node node { get; set; }

        public DialoguePreview(Node node) 
        {
            this.node = node;
            CameraCalculator = new CameraCalculator();  
            PreviewRender = new PreviewCameraRenderUtil();
            CreateActorCache();
        }

        public void CreateActorCache()
        {
            if (node is not ITalkable dialogueNode) return;

            var focusTarget = GameObject.Find(dialogueNode.NodeConvodata.ShotConfig.actor);
            if(focusTarget == null)
            {
                //Need to make image that says actor "actor" is not in the current 
                return;
            }

            var objsToRender = GetChildrenWithMeshes(focusTarget.transform.parent);

            var meshMatList = new List<(Mesh, Material)>();

            foreach (var obj in objsToRender)
            {
                var mesh = GetMesh(obj);
                var mat = GetMaterial(obj);

                meshMatList.Add((mesh, mat));
            }

            CachedActorMesh = new ActorMeshData(focusTarget, meshMatList);
        }

        //TODO: Create a single entry point that knows when to draw a new preview window or load a texture.
        public void DrawWindow()
        {
            DrawPreviewWindow();

            /*
CachedTextures.TryGetValue(node, out Texture2D texture);

if (texture == null)
{
    DrawPreviewWindow();
}
else
{
    DrawCachedWindow();
}
*/

        }

        void DrawPreviewWindow()
        {
            if (node is not ITalkable dialogueNode) return;

            var windowRect = new Rect(node.EditorPosition.x + node.NodeWidth, node.EditorPosition.y,
            node.NodeWidth, node.NodeHeight);

            if (dialogueNode.NodeConvodata.ShotConfig.GoalType == CameraGoal.Portrait)
            {
                if (CachedActorMesh == null)
                {
                    CreateActorCache();
                }

                PreviewRender.DrawSavePreview(windowRect, GetCamPose(), GetActorPose(), CachedActorMesh.MeshMat.ToArray());
            }

            PreviewRender.Dispose();
        }

        Pose GetActorPose() => new Pose(Vector3.zero, GetRotation(CachedActorMesh.FocusTarget.transform.position));

        Quaternion GetRotation(Vector3 pos)
        {
            Vector3 midPoint = CameraCalculator.CalculateMidPoint();
            Vector3 direction = pos - midPoint;

            direction.y = 0;

            // Check if the direction vector is valid (non-zero), prevents warning message being spammed to console.
            if (direction.sqrMagnitude > Mathf.Epsilon) 
            {
                return Quaternion.LookRotation(direction);
            }
            else
            {
                return Quaternion.identity;
            }
        }

        Pose GetCamPose()
        {
            if (node is not ITalkable posNode) throw new ArgumentException($"Error: {node} is not {typeof(ITalkable)}");

            var actorPosition = CachedActorMesh.FocusTarget.transform.position;

            var initialCamPose = CameraCalculator.CalculatePlacement(posNode.NodeConvodata.ShotConfig);

            // Calculate the relative position to the actor, ignoring y-axis differences.
            Vector3 relativePosition = new Vector3(actorPosition.x - initialCamPose.position.x, 0, actorPosition.z - initialCamPose.position.z);

            // Adjust the camera's position to maintain the initial y-position.
            Vector3 finalPosition = relativePosition + new Vector3(0, initialCamPose.position.y, 0);

            // Rotate the camera to face the actor.
            Quaternion finalRotation = Quaternion.Euler(initialCamPose.rotation.eulerAngles + new Vector3(0, 180f, 0));

            return new Pose(finalPosition, finalRotation);
        }

        void DrawCachedWindow()
        {
            if (node is not ITalkable) return;

            if (PreviewRender.CachedRenderTexture == null) return;

            //For some reason DrawPreviewWindow sometimes draws a blank texture and that gets cached. This prevents a blank texture from being drawn.

            if (PreviewRender.CachedRenderTexture.IsTextureEmpty())
            {
                DrawPreviewWindow();
                return;
            }

            var windowRect = new Rect(node.EditorPosition.x + node.NodeWidth, node.EditorPosition.y,
            node.NodeWidth, node.NodeHeight);

            GUI.DrawTexture(windowRect, PreviewRender.CachedRenderTexture);
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