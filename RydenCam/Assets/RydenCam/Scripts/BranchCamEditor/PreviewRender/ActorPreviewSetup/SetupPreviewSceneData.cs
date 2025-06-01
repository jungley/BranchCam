using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup.PlacementStrategies;
using RydenCam.BranchCamEditor.Managers;
using System.Collections.Generic;
namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup
{
    public static class SetupPreviewSceneData
    {
        public static List<PreviewActorData> PreviewActorDatas { get; set; }

        public static void CalculateActorsInPreviewSpace()
        {
            var actors = NodeManager.Instance.ActorsInScene;
            IActorPlacementStrategy strategy;

            if (NodeManager.Instance.StartNode.StartPositionsEnabled)
                strategy = new PredefinedPositionStrategy();
            else if (actors.Count == 1)
                strategy = new SingleActorStrategy();
            else if (actors.Count == 2)
                strategy = new TwoActorStrategy();
            else
                strategy = new CircularPlacementStrategy();

            PreviewActorDatas = strategy.GeneratePreviewData(actors);
        }
    }
}
