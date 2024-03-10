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

        //Make into ScriptableObject
        public GameObject ActorGO;
      
        public ActorInfo()
        {
            ActorID = Guid.NewGuid().ToString();
            ActorName = BranchConstants.UnAssignedActor;
        }
    }
}
