using Assets.RydenCam.Scripts.BranchCamCC;
using System.Collections;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Camera
{
    public class PreviewActorPositionData
    {
        public string ActorName { get; set; }

        public Vector3 ActorPosition { get; set; }
        public Quaternion ActorRotation { get; set; }
        public Vector3 ForwardN { get; set; }

        //Optional
        public Vector3 OppPosition { get; set; }
        public Quaternion OppRotation { get; set; }
        public Vector3 OppForwardN { get; set; }

        //For in Game
        //Need to rename this because it's also used in game
        public PreviewActorPositionData(ITalkable node)
        {
            ActorName = node.NodeConvodata.Actor.ActorGO.name;
            ActorPosition = node.NodeConvodata.Actor.ActorGO.transform.position;
            ActorRotation = node.NodeConvodata.Actor.ActorGO.transform.rotation;
            ForwardN = node.NodeConvodata.Actor.ActorGO.transform.forward;
            OppPosition = (node.NodeConvodata.ShotConfig.oppositeActor != string.Empty) ? GameObject.Find(node.NodeConvodata.ShotConfig.oppositeActor).transform.position : Vector3.zero;
            OppRotation = (node.NodeConvodata.ShotConfig.oppositeActor != string.Empty) ? GameObject.Find(node.NodeConvodata.ShotConfig.oppositeActor).transform.rotation : Quaternion.identity;
            OppForwardN = (node.NodeConvodata.ShotConfig.oppositeActor != string.Empty) ? GameObject.Find(node.NodeConvodata.ShotConfig.oppositeActor).transform.forward : Vector3.zero;
        }

        //Preview Render
        public PreviewActorPositionData()
        {

        }
    }

}