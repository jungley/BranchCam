using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Controllers;
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
    [System.Serializable]
    public class DialogueNode : NodeCC, ITalkable 
    {
        [SerializeField]
        public ConversationData NodeConvodataField;

        public ConversationData NodeConvodata
        {
            get { return NodeConvodataField; }
            set { NodeConvodataField = value; }
        }

        public override float NodeHeight => NodeConvodata?.DialogTextList.Count > 2 ? NodeConvodata.DialogTextList.Count * 20 + 75 : 120;

        public DialogueNode(Vector2 position) : base(position)
        {
            TypeOfNode = NodeType.DialogueNode;
            PointIn = new ConnectionPoint(this, ConnectionPointType.In);
            PointOut = new List<ConnectionPoint>() { new ConnectionPoint(this, ConnectionPointType.Out)};
            NodeConvodata = new ConversationData();
        }

    }
}
