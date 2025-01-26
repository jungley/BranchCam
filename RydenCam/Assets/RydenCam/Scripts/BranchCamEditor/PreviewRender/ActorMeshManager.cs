using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender
{
    public class ActorMeshManager
    {
        public ActorMeshPreviewData CachedActorMesh { get; set; }

        public ActorMeshManager(GameObject focusTarget)
        {
            CachedActorMesh = CacheActorMeshes(focusTarget);
        }

        public ActorMeshPreviewData CacheActorMeshes(GameObject focusTarget)
        {
            if (focusTarget == null)
            {
                // Handle missing actor
                return null;
            }

            var objsToRender = GetChildrenWithMeshes(focusTarget.transform.parent);
            var meshMatList = new List<(Mesh Mesh, Material Mat)>();

            foreach (var obj in objsToRender)
            {
                var mesh = GetMesh(obj);
                var mat = GetMaterial(obj);

                meshMatList.Add((mesh, mat));
            }

            return new ActorMeshPreviewData(focusTarget, meshMatList);
        }

        private GameObject[] GetChildrenWithMeshes(Transform actorParent)
        {
            var meshChildren = new List<GameObject>();
            FindMeshChildren(actorParent, meshChildren);
            return meshChildren.ToArray();
        }

        private void FindMeshChildren(Transform parent, List<GameObject> meshChildren)
        {
            if (parent == null) return;

            foreach (Transform child in parent)
            {
                if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    meshChildren.Add(child.gameObject);
                }

                // Recursively search through all children
                FindMeshChildren(child, meshChildren);
            }
        }

        private Material GetMaterial(GameObject obj)
        {
            if (obj.GetComponent<SkinnedMeshRenderer>() != null) return obj.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
            if (obj.GetComponent<Renderer>() != null) return obj.GetComponent<Renderer>().sharedMaterial;

            return null;
        }

        private Mesh GetMesh(GameObject obj)
        {
            if (obj.GetComponent<SkinnedMeshRenderer>() != null) return GetSkinnedMesh(obj);
            if (obj.GetComponent<MeshFilter>() != null) return obj.GetComponent<MeshFilter>().sharedMesh;

            return null;
        }

        private Mesh GetSkinnedMesh(GameObject obj)
        {
            var skinnedRenderer = obj.GetComponent<SkinnedMeshRenderer>();
            Mesh newMesh = new Mesh();
            skinnedRenderer.BakeMesh(newMesh);
            return newMesh;
        }
    }
}