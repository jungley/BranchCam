using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamCC
{


    public class StartNode : NodeCC
    {
        public override NodeType TypeOfNode => NodeType.StartNode;

        //Actors In Scene
        public List<ActorInfo> ActorsInScene { get; set; }

        //start Position variables
        public bool StartPositionsEnabled;
        public bool OverrideRotation;
        public bool ReturnToOriginalPositions;
        public List<Pose> OriginalPositions;
        public List<Pose> SetStartPositions;

        //Scene Information
        public string UnitySceneName { get; set; }
        public string SequenceName { get; set; }
        public Side CameraSide { get; set; }

        public StartNode(Vector2 position) : base(position)
        {
            ActorsInScene = new List<ActorInfo>();
            PointOut = new List<ConnectionPoint>() { new ConnectionPoint(this, ConnectionPointType.Out) }; 
            
        }


    }
}
