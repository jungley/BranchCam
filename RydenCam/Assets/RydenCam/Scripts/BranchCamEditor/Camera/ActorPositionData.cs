using Assets.RydenCam.Scripts.BranchCamCC;
using System.Collections;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Camera
{
    /// <summary>
    /// Used in Preview Window AND In Game
    /// </summary>
    [System.Serializable]
    public class ActorPositionData
    {
        public Vector3 ActorPosition;
        public Quaternion ActorRotation;
        public Vector3 ForwardN;


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