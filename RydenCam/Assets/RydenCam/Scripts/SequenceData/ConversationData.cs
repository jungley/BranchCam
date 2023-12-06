using RydenCam.BranchCamEditor.BranchCam;
using System.Collections.Generic;
using UnityEngine;

namespace RydenCam.SequenceData
{
    /*
     * Contains the following:
     * ActorID - which character
     * DialogText - text of the dialog
     * Audio Clip - actor speaking
     * Animation clip 
     * CameraShots - Shots or Shot of the dialog
    */
    [System.Serializable]
    [ExecuteAlways]
    public class ConversationData
    {
        [SerializeField]
        public List<string> DialogTextList;
        [SerializeField]
        public ActorInfo Actor;
        [SerializeField]
        public CamShotConfig ShotConfig;
        public ConversationData(ActorInfo actor, List<string> dialog = null)
        {
            DialogTextList = dialog == null ? new List<string>() : dialog;
            Actor = actor;
        }
    }
}
