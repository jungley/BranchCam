using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using System.Linq;

namespace Assets.RydenCam.Scripts.NodeCommands
{
    public class DecisionNodeCommand : INodeCommand, IHasCustomCameraCommand
    {

        private DecisionNode node { get; set; }

        public CustomCameraCommand CustomCameraCommand { get; set; }

        public DecisionNodeCommand(Node _node)
        {
            node = _node as DecisionNode;
            CustomCameraCommand = new CustomCameraCommand(node);
        }

        public void AddDecisionOption()
        {
            node.DecisionOptions.Add("");
            node.PointOut.Add(new ConnectionPoint(node, ConnectionPointType.Out));
        }

        public void AssignNewActor(int actorIndex)
        {
            var actor = NodeManager.Instance.ActorsInScene[actorIndex].ActorID;
            node.NodeConvodata.Actor = NodeManager.Instance.ActorsInScene.Where(x => x.ActorID == actor).FirstOrDefault();
            node.NodeConvodata.ShotConfig.actor = node.NodeConvodata.Actor.ActorName;
        }

        public void RemoveDecisionOption(int index)
        {
            node.DecisionOptions.RemoveAt(index);
            node.PointOut.RemoveAt(index);
        }

        public void RemoveNode(Node node)
        {
            NodeManager.Instance.ActiveNode = null;
            NodeManager.Instance.RemoveNode(node);
            ConnectionManager.Instance.RemoveAssociatedConnections(node);
        }
    }
}
