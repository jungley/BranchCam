using RydenCam.BranchCamEditor.Managers;
using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup.PlacementStrategies
{
    //Strategy - use predefined start positions
    public class PredefinedPositionStrategy : IActorPlacementStrategy
    {
        public void GeneratePreviewData(float d)
        {

            foreach(var actor in NodeManager.Instance.ActorsInScene)
            {
                actor.PreviewData = PreviewPlacementMeshGenerator.Create(
                    actor,
                    actor.PreDefinedStartPosition.position,
                    actor.PreDefinedStartPosition.rotation,
                    actor.PreDefinedStartPosition.forward
                );
            }
        }
    }
}
