using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender
{
    public class ActorMeshPreviewData
    {
        public GameObject FocusTarget { get; set; }
        public List<(Mesh Mesh, Material Mat)> MeshMat { get; set; }

        public ActorMeshPreviewData(GameObject focusTarget, List<(Mesh, Material)> meshMat)
        {
            FocusTarget = focusTarget;
            MeshMat = meshMat;
        }
    }
}