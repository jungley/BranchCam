using Assets.RydenCam.Scripts.BranchCamEditor.Camera;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender
{
    [System.Serializable]
    public class PreviewActorData
    {
        public List<(Mesh Mesh, Material Mat, Vector3 Scale)> MeshMatScale { get; set; }
        public ActorPositionData ActorPositionData;

        public string ActorID;

        public Vector3 MeshOriginPoint;

        public bool IsSet = false;

        public PreviewActorData()
        {
            MeshMatScale = new List<(Mesh Mesh, Material Mat, Vector3 Scale)>();
            ActorPositionData = new ActorPositionData();
        }
    }
}