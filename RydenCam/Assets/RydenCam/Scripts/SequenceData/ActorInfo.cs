using Assets.RydenCam.Scripts.BranchCamEditor.Camera;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup;
using RydenCam.Common;
using System;
using UnityEngine;

namespace RydenCam.SequenceData
{
    [ExecuteAlways]
    [System.Serializable]
    public class ActorInfo
    {
        public string ActorID;
        public string ActorName;

        public bool PreDefinedStartPositionEnabled;
        public Pose PreDefinedStartPosition;
        public Pose OriginalPositionAtStartOfDialogue;

        public ActorPositionData PosData { get; set; }

        public PreviewActorData PreviewData;

        private GameObject _actorGO { get; set; }
        public GameObject ActorGO
        {
            get
            {
                if (_actorGO == null 
                    && !string.IsNullOrEmpty(ActorName) 
                    && ActorName != BranchConstants.UnAssignedActor)
                {
                    _actorGO = GameObject.Find(ActorName);
                    SetupPreviewSceneData.CalculateActorsInPreviewSpace();
                }
                return _actorGO;
            }
            set
            {
                _actorGO = value;
                ActorName = value?.name ?? BranchConstants.UnAssignedActor;

                // Recalculate only after the new actor is observable through ActorGO.
                if (value != null)
                    SetupPreviewSceneData.CalculateActorsInPreviewSpace();
            }

        }

        public ActorInfo()
        {
            ActorID = Guid.NewGuid().ToString();
            ActorName = BranchConstants.UnAssignedActor;
            PreviewData = new PreviewActorData();
            PosData = new ActorPositionData();
        }
    }
}
