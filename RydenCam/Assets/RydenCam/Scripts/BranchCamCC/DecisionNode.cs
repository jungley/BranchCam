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
    public class DecisionNode : NodeCC, ITalkable
    {

        public override NodeType TypeOfNode => NodeType.DecisionNode;
        public ConversationData NodeConvodata { get; set; }

        public override float NodeHeight => DecisionOptions.Count > 2 ? DecisionOptions.Count * 25 + 65 : 120;

        public List<string> DecisionOptions { get; set; }

        public DecisionNode(Vector2 position) : base(position)
        {
            DecisionOptions = new List<string>() { "" };
            PointIn = new ConnectionPoint(this, ConnectionPointType.In);
            PointOut = new List<ConnectionPoint>() { new ConnectionPoint(this, ConnectionPointType.Out) };
            NodeConvodata = new ConversationData();
        }
    }
}
