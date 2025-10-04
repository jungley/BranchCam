using Assets.RydenCam.Scripts.BranchCamCC;
using System.Collections;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Camera
{  
    /// <summary>
    /// Used in Preview Window AND In Game
    /// </summary>
    public class ActorPositionData
    {
        public Vector3 ActorPosition { get; set; }
        public Quaternion ActorRotation { get; set; }
        public Vector3 ForwardN { get; set; }


        public ActorPositionData(ITalkable node)
        {
            ActorPosition = node.NodeConvodata.Actor.ActorGO.transform.position;
            ActorRotation = node.NodeConvodata.Actor.ActorGO.transform.rotation;
            ForwardN = node.NodeConvodata.Actor.ActorGO.transform.forward;
        }

        //Preview Render
        public ActorPositionData()
        {
        }
    }

}