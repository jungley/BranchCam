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
        public PreviewRenderUtility PreviewRenderUtility { get => previewUtility; }
        private PreviewRenderUtility previewUtility;

        public CameraCalculator CameraCalculator;
        GameObject[] previousRenderObject;
        Dictionary<GameObject, Mesh> cachedMeshes = new Dictionary<GameObject, Mesh>();
        Texture cachedTexture;

        Texture2D blankTexture;
        Texture2D BlankTexture
        {
            get
            {
                if(blankTexture == null)
                {
                    var previewTexture = new Texture2D(1, 1);
                    previewTexture.SetPixel(0, 0, Color.black);
                    previewTexture.Apply(); 
                    blankTexture = previewTexture;
                }
                return blankTexture;
            }
        }

        public void Initialize()
        {
            if(previewUtility == null)
            {
                previewUtility = new PreviewRenderUtility();
                CameraCalculator = new CameraCalculator();

                //Initialize Camera Settings
                var sourceCamera = Camera.main;
                previewUtility.camera.fieldOfView = sourceCamera.fieldOfView;
                previewUtility.camera.depth = sourceCamera.depth;
                previewUtility.camera.nearClipPlane = 1f;
                previewUtility.camera.farClipPlane = 20;
            }
        }



        public void DrawPreview(GameObject[] objsToRender, Rect windowRect, EditorDialogueNode node)
        {
            if (ShouldCreateNewPreview(objsToRender))
            {

                Pose actorPose = new Pose(node.NodeConvodata.Actor.PreDefinedStartPosition.position, node.NodeConvodata.Actor.PreDefinedStartPosition.rotation);
                CameraCalculator.SetSide(NodeManager.Instance.StartNode.CameraSide);
                CameraCalculator.CalculatePlacement(node.NodeConvodata.ShotConfig);

                SetCamera(actorPose);

                previewUtility.BeginStaticPreview(windowRect);

                foreach (var obj in objsToRender)
                {
                    DrawCustomObjectPreview(obj, actorPose);
                }

                previewUtility.Render();
                Texture previewTexture = previewUtility.EndStaticPreview();
                GUI.DrawTexture(windowRect, previewTexture);

                cachedTexture = previewTexture;
                previousRenderObject = objsToRender;
            }
            else
            {
                GUI.DrawTexture(windowRect, cachedTexture);
            }
        }

        public void DrawBlankPreview(Rect windowRect) => GUI.DrawTexture(windowRect, BlankTexture);

        //BUG: For some reason this outputs a blank grey box sometimes. 
       //private bool ShouldCreateNewPreview(GameObject[] objs) => objs != previousRenderObject || cachedTexture == null;
       
        //TEMP: Redraw preview every GUI call in order to debug camera situation.
        private bool ShouldCreateNewPreview(GameObject[] objs) => true;

        private void DrawCustomObjectPreview(GameObject objToRender, Pose actorPose)
        {
            if (objToRender == null)
            {
                BranchLog.Log("Gameobject to render is null.");
                return;
            }

            Matrix4x4 customMatrix = Matrix4x4.TRS(actorPose.position, actorPose.rotation.normalized, Vector3.one);

            previewUtility.DrawMesh(GetMesh(objToRender), customMatrix, GetMaterial(objToRender), 0);
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

        void SetCamera(Pose camPose)
        {
            Vector3 finalCameraPosition()
            {
                //forced to add this strange offset in order to get the camera on the actor. Need to figure out a way to not need this offset.
                var offset = new Vector3(-0.5f, 0, -4f);
                return camPose.position + offset;
            }

            Quaternion finalCameraRotation()
            {
                Vector3 euler = camPose.rotation.eulerAngles;
                //flip the camera around to face the actor.
                euler.y += 180f;
                return Quaternion.Euler(euler);
            }

            previewUtility?.camera.transform.SetPositionAndRotation(finalCameraPosition(), finalCameraRotation());
        }
    }
}
