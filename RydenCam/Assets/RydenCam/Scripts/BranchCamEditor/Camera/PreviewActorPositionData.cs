using Assets.RydenCam.Scripts.BranchCamCC;
using System.Collections;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Camera
{  
    public class PreviewActorPositionData
    {
        public string ActorName { get; set; }

        public Vector3 MeshOriginPoint { get; set; }

        public Vector3 ActorPosition { get; set; }
        public Quaternion ActorRotation { get; set; }
        public Vector3 ForwardN { get; set; }


        public PreviewActorPositionData(ITalkable node)
        {
            ActorName = node.NodeConvodata.Actor.ActorGO.name;
            ActorPosition = node.NodeConvodata.Actor.ActorGO.transform.position;
            ActorRotation = node.NodeConvodata.Actor.ActorGO.transform.rotation;
            ForwardN = node.NodeConvodata.Actor.ActorGO.transform.forward;
        }

        public PreviewActorPositionData(string actorName)
        {
            GameObject actorGO = GameObject.Find(actorName);
            if (actorGO != null)
            {
                ActorName = actorGO.name;
                ActorPosition = actorGO.transform.position;
                ActorRotation = actorGO.transform.rotation;
                ForwardN = actorGO.transform.forward;
            }
        }

        //Preview Render
        public PreviewActorPositionData()
        {
        }
    }

}