using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using RydenCam.SequenceData;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamCC
{
    [System.Serializable]
    public class StartNode : Node
    {
        //Actors In Scene
        public List<ActorInfo> ActorsInScene;

        //start Position variables
        public bool StartPositionsEnabled;
        public bool OverrideRotation;
        public bool ReturnToOriginalPositions;
        public List<Pose> OriginalPositions;
        public List<Pose> SetStartPositions;

        //Scene Information
        public string UnitySceneName;
        public string SequenceName;
        public Side CameraSide;

        public override float NodeHeight => 70;

        public StartNode(Vector2 position) : base(position)
        {
            TypeOfNode = NodeType.StartNode;
            ActorsInScene = new List<ActorInfo>();
            PointOut = new List<ConnectionPoint>() { new ConnectionPoint(this, ConnectionPointType.Out) }; 
            
        }
    }
}
