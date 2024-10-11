using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.RydenCam.Scripts.NodeCommands
{
    public class DecisionNodeCommand : INodeCommand
    {

        private DecisionNode node { get; set; }


        public DecisionNodeCommand(NodeCC _node)
        {
            node = _node as DecisionNode;
        }

        public void AddDecisionOption()
        {
            node.DecisionOptions.Add("");
            node.PointOut.Add(new ConnectionPoint(node, ConnectionPointType.Out));
        }

        public void AssignNewActor(int actorIndex)
        {
            var actor = NodeManager.Instance.ActorsInScene()[actorIndex].ActorID;
            node.NodeConvodata.Actor = NodeManager.Instance.ActorsInScene().Where(x => x.ActorID == actor).FirstOrDefault();
            node.NodeConvodata.ShotConfig.actor = node.NodeConvodata.Actor.ActorName;
        }

        public void RemoveDecisionOption(int index)
        {
            node.DecisionOptions.RemoveAt(index);
            node.PointOut.RemoveAt(index);
        }

        public void RemoveNode(NodeCC node)
        {
            NodeManager.Instance.ActiveNode = null;
            NodeManager.Instance.RemoveNode(node);
            ConnectionManager.Instance.RemoveAssocConnec(node);
        }
    }
}
