using Assets.RydenCam.Scripts.BranchCamEditor.Camera;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.SequenceData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender
{
    public static class SetupPreviewSceneData
    {

        private static float defaultPreviewDistance = 5.0f;

        public static List<PreviewActorData> Initialize()
        {
            List<PreviewActorData> PreviewActorDatas = new List<PreviewActorData>();


            PreviewActorPositionData positionData = new PreviewActorPositionData();
            //Set the Position Data

            if (NodeManager.Instance.StartNode.StartPositionsEnabled)
            {
                //RS TODO: This needs to be set relative with StartNode's manual positions if that is enabled
            }
            else
            {
                int actorCount = NodeManager.Instance.ActorsInScene().Count;

                if (actorCount == 1)
                {
                    ActorInfo actor = NodeManager.Instance.ActorsInScene()[0];

                    PreviewActorDatas.Add(new PreviewActorData
                    {
                        ActorPositionData = new PreviewActorPositionData
                        {
                            ActorPosition = actor.ActorGO.transform.localPosition,
                            ActorName = actor.ActorName,
                            ActorRotation = Quaternion.identity,
                            ForwardN = Vector3.forward
                        },

                        MeshMat = CacheActorMeshes(actor.ActorGO)
                    });
                }
                else if (actorCount == 2)
                {
                    ActorInfo firstActor = NodeManager.Instance.ActorsInScene()[0];
                    ActorInfo secondActor = NodeManager.Instance.ActorsInScene()[1];    

                    // Move the second actor along the Z-axis by 5 units
                    Vector3 firstActorPosition = firstActor.ActorGO.transform.localPosition;
                    Vector3 secondActorPosition = secondActor.ActorGO.transform.localPosition;

                    //Move along the line between the two actors
                    secondActorPosition.z += defaultPreviewDistance;


                    // Add the first actor's information to the PreviewActorDatas list
                    PreviewActorDatas.Add(new PreviewActorData
                    {
                        ActorPositionData = new PreviewActorPositionData
                        {
                            ActorPosition = firstActor.ActorGO.transform.localPosition,
                            ActorRotation = Quaternion.identity,
                            ActorName = firstActor.ActorName,
                            ForwardN = Vector3.forward,
                        },

                        MeshMat = CacheActorMeshes(firstActor.ActorGO)

                    });

                    // Add the second actor's information to the PreviewActorDatas list
                    PreviewActorDatas.Add(new PreviewActorData
                    {
                        ActorPositionData = new PreviewActorPositionData
                        {
                            ActorPosition = secondActorPosition,
                            ActorRotation = Quaternion.identity,
                            ActorName = secondActor.ActorName,
                            ForwardN = Vector3.forward //?
                        },

                        MeshMat = CacheActorMeshes(secondActor.ActorGO)
                    });
                }
                else if (actorCount > 2)
                {
                    //RS TODO Position them in a circle
                }



            }
            return PreviewActorDatas;
        }

        private static  List<(Mesh Mesh, Material Mat)> CacheActorMeshes(GameObject focusTarget)
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

            return meshMatList;
        }

        private static GameObject[] GetChildrenWithMeshes(Transform actorParent)
        {
            var meshChildren = new List<GameObject>();
            FindMeshChildren(actorParent, meshChildren);
            return meshChildren.ToArray();
        }

        private static void FindMeshChildren(Transform parent, List<GameObject> meshChildren)
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