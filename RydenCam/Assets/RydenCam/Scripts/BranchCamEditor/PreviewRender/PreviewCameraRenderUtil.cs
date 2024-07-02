using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using RydenCam.Common;
using RydenCam.BranchCamEditor.BranchCam;

namespace RydenCam.BranchCamEditor.PreviewRender
{
    /// <summary>
    /// Draws a rect and renders objects into the preview window.
    /// </summary>

    public class PreviewCameraRenderUtil  
    {
        public PreviewRenderUtility PreviewRenderUtility { get; set; }
        public Texture CachedRenderTexture { get; set; }
        public CamShotConfig CachedShot { get; set; }

        //May not need the 2 cached Meshs stuff after saving Texture in Dictionary
        public List<GameObject> cachedChildrenWithMeshes = new List<GameObject>();
        public Dictionary<GameObject, Mesh> cachedMeshes = new Dictionary<GameObject, Mesh>();


        public CameraCalculator CameraCalculator;
   

        public PreviewCameraRenderUtil(CamShotConfig shot)
        {
            CachedShot = shot;
            CameraCalculator = new CameraCalculator();
            InitializeUnityRenderUtility();
        }

        public void InitializeUnityRenderUtility()
        {
            PreviewRenderUtility = new PreviewRenderUtility();

            //Initialize Camera Settings
            var sourceCamera = Camera.main;
            PreviewRenderUtility.camera.fieldOfView = 40;
            PreviewRenderUtility.camera.nearClipPlane = 0.01f;
            PreviewRenderUtility.camera.farClipPlane = 20;
        }
    
        public void DrawSavePreview(Rect windowRect, EditorDialogueNode node)
        {
            var focusTarget = GameObject.Find(node.NodeConvodata.Actor.ActorName);
            var objsToRender = GetChildrenWithMeshes(focusTarget.transform.parent);

            //TODO: Adjust the rotation of each object individually.
            Quaternion adjustedRotation = objsToRender[0].transform.rotation * Quaternion.Euler(0, 180, 0);
            Pose actorPose = new Pose(Vector3.zero, adjustedRotation);

            CameraCalculator.SetSide(NodeManager.Instance.StartNode.CameraSide);
            Pose camPose = CameraCalculator.CalculatePlacement(node.NodeConvodata.ShotConfig);

            var inSceneActorPos = GameObject.Find(node.NodeConvodata.ShotConfig.actor).transform.position;
            var relativeVector = inSceneActorPos - camPose.position; 
            var finalPose = new Pose(actorPose.position + relativeVector + new Vector3(0, camPose.position.y, 0), camPose.rotation);

            SetCamera(finalPose);

            PreviewRenderUtility.BeginStaticPreview(windowRect);

            foreach (var obj in objsToRender)
            {
                DrawCustomObjectPreview(obj, actorPose);
            }

            PreviewRenderUtility.Render();
            Texture previewRenderTexture = PreviewRenderUtility.EndStaticPreview();
            PreviewRenderUtility.Cleanup();
            GUI.DrawTexture(windowRect, previewRenderTexture);

            CachedRenderTexture = previewRenderTexture;
        }

      
        private void DrawCustomObjectPreview(GameObject objToRender, Pose actorPose)
        {
            if (objToRender == null)
            {
                BranchLog.Log("Gameobject to render is null.");
                return;
            }

            Matrix4x4 customMatrix = Matrix4x4.TRS(actorPose.position, actorPose.rotation.normalized, Vector3.one);

            PreviewRenderUtility.DrawMesh(GetMesh(objToRender), customMatrix, GetMaterial(objToRender), 0);
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
            if (obj.GetComponent<SkinnedMeshRenderer>() != null) return GetCachedMesh(obj);

            if (obj.GetComponent<MeshFilter>() != null) return obj.GetComponent<MeshFilter>().sharedMesh;


            BranchLog.Log("No Mesh or Skinned Mesh found on " + obj);
            return null;
        }


        private Mesh GetCachedMesh(GameObject keyObject)
        {
            if (keyObject == null)
            {
                BranchLog.Log ("GameObject is null. Cannot retrieve cached mesh.");
                return null;
            }

            // Check if the mesh is already cached
            if (cachedMeshes.TryGetValue(keyObject, out Mesh cachedMesh)) return cachedMesh;

            var skinnedRenderer = keyObject.GetComponent<SkinnedMeshRenderer>();

            // Create a new Mesh and bake it
            Mesh newMesh = new Mesh();
            skinnedRenderer.BakeMesh(newMesh);

            // Cache the new mesh
            cachedMeshes[keyObject] = newMesh;

            return newMesh;
        }

        private GameObject[] GetChildrenWithMeshes(Transform actorParent)
        {
            //if (cachedChildrenWithMeshes.TryGetValue(actorParent, out GameObject[] cachedObjects))
            if(cachedChildrenWithMeshes.Any())
            {
                return cachedChildrenWithMeshes.ToArray();
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

                //cachedChildrenWithMeshes[actorParent] = meshChildren.ToArray();
                cachedChildrenWithMeshes = meshChildren;

                return meshChildren.ToArray();
            }
        }

        void SetCamera(Pose camPose)
        {
            Quaternion finalCameraRotation()
            {
                Vector3 euler = camPose.rotation.eulerAngles;
                //flip the camera around to face the actor.
                euler.y += 180f;
                return Quaternion.Euler(euler);
            }

            PreviewRenderUtility.camera.transform.SetPositionAndRotation(camPose.position, finalCameraRotation());
        }
    }
}
