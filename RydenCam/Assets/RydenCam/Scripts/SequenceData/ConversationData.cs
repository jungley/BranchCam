using Assets.RydenCam.Scripts.Editor.CameraShotEdtior;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System.Collections.Generic;
using System.Linq;
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
        public ActorInfo Actor;
        public ActorInfo OppositeActor;

        public List<string> DialogTextList;

        public CamShotConfig ShotConfig;
        public ConversationData(ActorInfo actor, List<string> dialog = null)
        {
            DialogTextList = dialog == null ? new List<string>() { string.Empty } : dialog;
            Actor = actor;
        }

        public ConversationData()
        {
            Actor = NodeManager.Instance.ActorsInScene.FirstOrDefault();
            ShotConfig = CameraShotsManager.Instance.DefaultShot;
            DialogTextList = new List<string> { string.Empty };
        }
    }
}
