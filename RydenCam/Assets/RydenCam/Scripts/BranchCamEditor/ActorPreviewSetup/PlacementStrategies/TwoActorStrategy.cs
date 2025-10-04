using RydenCam.BranchCamEditor.Managers;
using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup
{
    public class TwoActorStrategy : IActorPlacementStrategy
    {
        public void GeneratePreviewData(float distanceBetween)
        {
            Vector3 offset = new Vector3(0, 0, distanceBetween);

            var actors = NodeManager.Instance.ActorsInScene;
            actors[0].PreviewData = PreviewPlacementMeshGenerator.Create(actors[0], Vector3.zero, Quaternion.identity, Vector3.forward);
            actors[1].PreviewData = PreviewPlacementMeshGenerator.Create(actors[1], offset, Quaternion.Euler(0, 180, 0), Vector3.back);
        }
    }
}
