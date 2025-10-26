using RydenCam.BranchCamEditor.Managers;
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
        /// <summary>
        /// Actors in Scene, Start Position Data set in <see cref="ActorInfo"/>
        /// </summary>
        public List<ActorInfo> ActorsInScene;

        //start Position variables
        public bool StartPositionsEnabled;
        public bool OverrideRotation;
        public bool ReturnToOriginalPositions;

        //Scene Information
        public string UnitySceneName;
        public string SequenceName;
        public Side CameraSide;

        public override float NodeHeight => 70;

        public StartNode(Vector2 position) : base(position)
        {
            TypeOfNode = NodeType.StartNode;
            ActorsInScene = new List<ActorInfo>();
            PointIn = null;
            PointOut = new List<ConnectionPoint>() { new ConnectionPoint(this, ConnectionPointType.Out) };
        }
    }
}
