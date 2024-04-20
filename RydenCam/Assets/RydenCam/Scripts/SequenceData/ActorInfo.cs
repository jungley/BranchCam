using System;
using UnityEngine;
using RydenCam.Common;

namespace RydenCam.SequenceData
{
    [ExecuteAlways]
    [System.Serializable]
    public class ActorInfo
    {
        public string ActorID;
        public string ActorName;
        public Pose PreDefinedStartPosition;
        public Pose OriginalPositionAtStartOfDialogue;

        private GameObject _actorGO { get; set; }
        public GameObject ActorGO
        {
            get
            {
                if (_actorGO == null && !string.IsNullOrEmpty(ActorName))
                {
                    _actorGO = GameObject.Find(ActorName);
                }
                return _actorGO;
            }
            set
            {
                _actorGO = value;
            }
        }

    public ActorInfo()
        {
            ActorID = Guid.NewGuid().ToString();
            ActorName = BranchConstants.UnAssignedActor;
        }
    }
}
