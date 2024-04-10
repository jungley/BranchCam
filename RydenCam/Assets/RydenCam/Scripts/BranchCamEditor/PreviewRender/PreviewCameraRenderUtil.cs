using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RydenCam.BranchCamEditor.PreviewRender
{
    [ExecuteInEditMode]
    public class PreviewCameraRenderUtil : EditorWindow
    {

        private PreviewRenderUtility previewRender;
        

        public void Initialize()
        {
            previewRender = new PreviewRenderUtility();

        }

       public void OnGUI()
        {
            if(previewRender == null)
            {
                Initialize();
            }


            DrawSelectedMesh();
        }

        public void DrawSelectedMesh()
        {

            Mesh meshFilterMesh = null;
            Material meshRenderMaterial = null;

            var boundaries = new Rect(0, 0, this.position.width, this.position.height);
            previewRender.BeginStaticPreview(boundaries);
            var render = previewRender.EndStaticPreview();


            EditorGUI.DrawTextureAlpha(new Rect(0, 0, boundaries.width/2, boundaries.height/2), render);
        }

    }
}
