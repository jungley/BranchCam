using Assets.RydenCam.Scripts.BranchCamEditor.Camera;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender
{
    public class PreviewActorData
    {
        public List<(Mesh Mesh, Material Mat, Vector3 Scale)> MeshMatScale { get; set; }
        public ActorPositionData ActorPositionData { get; set; }

        public string ActorID { get; set; }

        public Vector3 MeshOriginPoint { get; set; }

        public PreviewActorData()
        {
            MeshMatScale = new List<(Mesh Mesh, Material Mat, Vector3 Scale)>();
            ActorPositionData = new ActorPositionData();
        }
    }
}