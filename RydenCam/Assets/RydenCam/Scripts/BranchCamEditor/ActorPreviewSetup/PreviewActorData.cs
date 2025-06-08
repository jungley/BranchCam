using Assets.RydenCam.Scripts.BranchCamEditor.Camera;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender
{
    public class PreviewActorData
    {
        public List<(Mesh Mesh, Material Mat, Vector3 Scale)> MeshMatScale { get; set; }
        public ActorPositionWrapper ActorPositionData { get; set; }
    }
}