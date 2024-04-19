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

namespace RydenCam.BranchCamEditor.PreviewRender
{
    /// <summary>
    /// Draws a rect and renders objects into the preview window.
    /// </summary>

    public class PreviewCameraRenderUtil 
    {
        public PreviewRenderUtility PreviewRenderUtility { get => previewUtility; }
        private PreviewRenderUtility previewUtility;

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
            if(previewUtility == null) previewUtility = new PreviewRenderUtility();
        }

        public void DrawPreview(GameObject[] objsToRender, Rect windowRect)
        {
            if (ShouldCreateNewPreview(objsToRender))
            {
                SetCamera();

                previewUtility.BeginStaticPreview(windowRect);

                foreach (var obj in objsToRender)
                {
                    DrawCustomObjectPreview(obj);
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

        private bool ShouldCreateNewPreview(GameObject[] objs) => objs != previousRenderObject;

        private void DrawCustomObjectPreview(GameObject objToRender)
        {
            if (objToRender == null)
            {
                BranchLog.Log("Gameobject to render is null.");
                return;
            }

            //TODO: Replace Matrix4x4.identity with custom Matrix according to custom convo settings in order to set the actors in the right place.
            Matrix4x4 customMatrix = Matrix4x4.TRS(new Vector3(), Quaternion.Euler(0f, 180f, 0f), Vector3.one);

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

        void SetCamera()
        {
            //TODO: Set the camera's pos/rot according to settings.
            previewUtility.camera.transform.position = new Vector3(0f, 1.5f, -2.5f);

            //Set near/far plane for performance. If something is not rendering, it could be outside the farclip plane.
            previewUtility.camera.nearClipPlane = 1f;
            previewUtility.camera.farClipPlane = 20f;
        }
    }
}
