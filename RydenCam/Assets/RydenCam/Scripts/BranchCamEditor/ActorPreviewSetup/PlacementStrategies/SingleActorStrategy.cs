using RydenCam.BranchCamEditor.Managers;
using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup.PlacementStrategies
{
    //Strategy - one actor centered 
    public class SingleActorStrategy : IActorPlacementStrategy
    {
        public void GeneratePreviewData(float distanceBetween)
        {
            var actor = NodeManager.Instance.ActorsInScene[0];
            
            actor.PreviewData = PreviewPlacementMeshGenerator.Create(actor, Vector3.zero, Quaternion.identity, Vector3.forward);
        }
    }
}
