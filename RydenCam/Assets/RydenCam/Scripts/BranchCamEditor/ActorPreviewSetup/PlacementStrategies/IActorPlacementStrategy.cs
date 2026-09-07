using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup
{
    public interface IActorPlacementStrategy
    {
        void GeneratePreviewData(float distanceBetween = 2.0f);
    }
}
