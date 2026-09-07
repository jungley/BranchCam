using Assets.RydenCam.Scripts.BranchCamEditor.Camera;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
using RydenCam.SequenceData;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup
{
    public static class PreviewPlacementMeshGenerator
    {
        public static PreviewActorData Create(ActorInfo actor, Vector3 meshOrigin, Quaternion rotation, Vector3 forward)
        {
            PreviewActorData data = new PreviewActorData();
            data.MeshOriginPoint = meshOrigin;
            data.MeshMatScale = CacheActorMeshes(actor.ActorGO);
            data.ActorID = actor.ActorID;

            ActorPositionData actorPositionData = new ActorPositionData();
            actorPositionData.ActorPosition = actor.ActorGO.transform.localPosition + meshOrigin;
            actorPositionData.ActorRotation = rotation;
            actorPositionData.ForwardN = forward;

            data.ActorPositionData = actorPositionData;

            return data;

            /*
            return new PreviewActorData
            {
                MeshOriginPoint = meshOrigin,

                ActorPositionData = new ActorPositionData
                {
                    ActorPosition = actor.ActorGO.transform.localPosition + meshOrigin,
                    ActorRotation = rotation,
                    ForwardN = forward,
                },
                MeshMatScale = CacheActorMeshes(actor.ActorGO),
                ActorID = actor.ActorID
            };
            */
        }

        private static List<(Mesh Mesh, Material Mat, Vector3 Scale)> CacheActorMeshes(GameObject focusTarget)
        {
            if (focusTarget == null)
            {
                // Handle missing actor
                return null;
            }

            var objsToRender = GetChildrenWithMeshes(focusTarget.transform.FindMostParent());
            var meshMatList = new List<(Mesh Mesh, Material Mat, Vector3 Scale)>();

            foreach (var obj in objsToRender)
            {
                var mesh = GetMesh(obj);
                var mat = CreateUnlitMaterial(GetMaterial(obj));
                var scale = obj.transform.FindMostParent().localScale;

                meshMatList.Add((mesh, mat, scale));
            }

            return meshMatList;
        }

        /// <summary>
        /// Creates the unlit material for the Preview Window
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        private static Material CreateUnlitMaterial(Material material)
        {
            Material unlitMat = new Material(Shader.Find("Unlit/Texture"));
            unlitMat.mainTexture = material.mainTexture;
            unlitMat.SetInt("_ShadowCastingMode", (int)UnityEngine.Rendering.ShadowCastingMode.Off);

            return unlitMat;
        }

        private static Material GetMaterial(GameObject obj)
        {
            if (obj.GetComponent<SkinnedMeshRenderer>() != null) return obj.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
            if (obj.GetComponent<Renderer>() != null) return obj.GetComponent<Renderer>().sharedMaterial;

            return null;
        }

        private static Mesh GetMesh(GameObject obj)
        {
            if (obj.GetComponent<SkinnedMeshRenderer>() != null) return GetSkinnedMesh(obj);
            if (obj.GetComponent<MeshFilter>() != null) return obj.GetComponent<MeshFilter>().sharedMesh;

            return null;
        }

        private static Mesh GetSkinnedMesh(GameObject obj)
        {
            var skinnedRenderer = obj.GetComponent<SkinnedMeshRenderer>();
            Mesh newMesh = new Mesh();
            skinnedRenderer.BakeMesh(newMesh);
            return newMesh;
        }

        private static GameObject[] GetChildrenWithMeshes(Transform actorParent)
        {
            var meshChildren = new List<GameObject>();
            FindMeshesInHierarchy(actorParent, meshChildren);
            return meshChildren.ToArray();
        }

        private static void FindMeshesInHierarchy(Transform parent, List<GameObject> meshChildren)
        {
            if (parent == null) return;

            // Check if the parent object itself has a MeshRenderer or SkinnedMeshRenderer
            if (parent.GetComponent<MeshRenderer>() != null || parent.GetComponent<SkinnedMeshRenderer>() != null)
            {
                meshChildren.Add(parent.gameObject);
            }

            // Now check the children recursively
            foreach (Transform child in parent)
            {
                if (child.GetComponent<MeshRenderer>() != null || child.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    meshChildren.Add(child.gameObject);
                }

                // Recursively search through all children
                FindMeshesInHierarchy(child, meshChildren);
            }
        }
    }
}