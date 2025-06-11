using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using System.Linq;

namespace Assets.RydenCam.Scripts.NodeCommands
{
    public class DecisionNodeCommand : TalkableCommand
    {
        private DecisionNode node { get; set; }

        public override int SpeakingEntriesCount => node.DecisionOptions.Count;

        public DecisionNodeCommand(Node _node) : base(_node)
        {
            node = _node as DecisionNode;
            CustomCameraCommand = new CustomCameraCommand(node);
        }
        public override void AddSpeakingEntry(int index)
        {
            node.DecisionOptions.Insert(index, "");
            node.PointOut.Add(new ConnectionPoint(node, ConnectionPointType.Out));
           
        }

        public override void RemoveSpeakingEntry(int index)
        {
            node.DecisionOptions.RemoveAt(index);
            node.PointOut.RemoveAt(index);
        }
    }
}
