using Assets.RydenCam.Scripts.BranchCamEditor.Camera;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.SequenceData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender
{
    public static class SetupPreviewSceneData
    {

        private static float defaultPreviewDistance = 2.0f;


        public static List<PreviewActorData> PreviewActorDatas { get; set; }

        public static void CalculateActorsinPreviewSpace()
        {
            PreviewActorDatas = new List<PreviewActorData>();


            ActorPositionWrapper positionData = new ActorPositionWrapper();
            //Set the Position Data

            if (NodeManager.Instance.StartNode.StartPositionsEnabled)
            {
                //RS TODO: This needs to be set relative with StartNode's manual positions if that is enabled
            }
            else
            {
                int actorCount = NodeManager.Instance.ActorsInScene.Count;

                if (actorCount == 1)
                {
                    ActorInfo actor = NodeManager.Instance.ActorsInScene[0];

                    PreviewActorDatas.Add(new PreviewActorData
                    {
                        ActorPositionData = new ActorPositionWrapper
                        {
                            MeshOriginPoint = Vector3.zero,
                            ActorPosition = actor.ActorGO.transform.localPosition,
                            ActorName = actor.ActorName,
                            ActorRotation = Quaternion.identity,
                            ForwardN = Vector3.forward,
                        },

                        MeshMatScale = CacheActorMeshes(actor.ActorGO)
                    });
                }

                else if (actorCount == 2)
                {
                    ActorInfo firstActor = NodeManager.Instance.ActorsInScene[0];
                    ActorInfo secondActor = NodeManager.Instance.ActorsInScene[1];    

                    Vector3 firstActorPosition = firstActor.ActorGO.transform.localPosition;
                    Vector3 secondActorPosition = secondActor.ActorGO.transform.localPosition;


                    // Add the first actor's information to the PreviewActorDatas list
                    PreviewActorDatas.Add(new PreviewActorData
                    {
                        ActorPositionData = new ActorPositionWrapper
                        {
                            MeshOriginPoint = Vector3.zero,
                            ActorPosition = firstActorPosition,
                            ActorRotation = Quaternion.identity,
                            ActorName = firstActor.ActorName,
                            ForwardN = Vector3.forward,
                        },

                        MeshMatScale = CacheActorMeshes(firstActor.ActorGO)

                    });

                    // Add the second actor's information to the PreviewActorDatas list
                    PreviewActorDatas.Add(new PreviewActorData
                    {
                        ActorPositionData = new ActorPositionWrapper
                        {
                            //Move along the line between the two actors
                            MeshOriginPoint = new Vector3(0, 0, defaultPreviewDistance),
                            ActorPosition = new Vector3(secondActorPosition.x, secondActorPosition.y, secondActorPosition.z + defaultPreviewDistance),
                            ActorRotation = Quaternion.Euler(0, 180, 0),
                            ActorName = secondActor.ActorName,
                            ForwardN = new Vector3(0, 0, -1)
                        },

                        MeshMatScale = CacheActorMeshes(secondActor.ActorGO)
                    });
                }
                else if (actorCount > 2)
                {
                    float radius = defaultPreviewDistance; // Radius of the circle
                    float angleStep = 360f / actorCount;

                    for (int i = 0; i < actorCount; i++)
                    {
                        ActorInfo actor = NodeManager.Instance.ActorsInScene[i];
                        float angle = i * angleStep * Mathf.Deg2Rad;

                        // Compute position/origin point on the circle
                        float x = radius * Mathf.Cos(angle);
                        float z = radius * Mathf.Sin(angle);
                        Vector3 originPoint = new Vector3(x, 0, z);

                        // Face the center (opposite of the position vector)
                        Vector3 forward = -originPoint.normalized;

                        PreviewActorDatas.Add(new PreviewActorData
                        {
                            ActorPositionData = new ActorPositionWrapper
                            {
                                MeshOriginPoint = originPoint,
                                ActorPosition = actor.ActorGO.transform.localPosition + originPoint,
                                ActorRotation = Quaternion.LookRotation(forward),
                                ActorName = actor.ActorName,
                                ForwardN = forward
                            },

                            MeshMatScale = CacheActorMeshes(actor.ActorGO)
                        });
                    }
                }
            }
            return;
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

        private static  List<(Mesh Mesh, Material Mat, Vector3 Scale)> CacheActorMeshes(GameObject focusTarget)
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

    }
}