using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Managers;
using System.Linq;

namespace Assets.RydenCam.Scripts.NodeCommands
{
    public class DialogueNodeCommand : INodeCommand
    {
        private DialogueNode node { get; set; }

        public DialogueNodeCommand(Node _node)
        {
            node = _node as DialogueNode;
        }

        public void AddDialogue()
        {
            node.NodeConvodata.DialogTextList.Add(string.Empty);
        }

        public void RemoveDialogue(int dialogueIndex)
        {
            node.NodeConvodata.DialogTextList.RemoveAt(dialogueIndex);
        }

        public void AssignNewActor(int actorIndex)
        {
            var actor = NodeManager.Instance.ActorsInScene()[actorIndex].ActorID;
            node.NodeConvodata.Actor = NodeManager.Instance.ActorsInScene().Where(x => x.ActorID == actor).FirstOrDefault();
            node.NodeConvodata.ShotConfig.actor = node.NodeConvodata.Actor.ActorName;
        }

        public void RemoveNode(Node node)
        {
            NodeManager.Instance.RemoveNode(node);
            ConnectionManager.Instance.RemoveAssociatedConnections(node);
            NodeManager.Instance.ActiveNode = null;
        }
    }
}
