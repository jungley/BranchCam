using System;
using UnityEngine;
using RydenCam.Common;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender;

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
                }
                return _actorGO;
            }
            set
            {
                _actorGO = value;
                ActorName = value?.name ?? BranchConstants.UnAssignedActor;
                //A new Actor has been asigned, recalculate the 3D Scene
                SetupPreviewSceneData.CalculateActorsinPreviewSpace();
            }
        }

    public ActorInfo()
        {
            ActorID = Guid.NewGuid().ToString();
            ActorName = BranchConstants.UnAssignedActor;
        }
    }
}
