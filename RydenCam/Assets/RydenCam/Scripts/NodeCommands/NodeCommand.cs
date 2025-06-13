using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Managers;

namespace Assets.RydenCam.Scripts.NodeCommands
{
    public abstract class NodeCommand
    {
        protected Node Node { get; set; }

        public NodeCommand(Node node)
        {
            Node = node;
        }   

        /// <summary>
        /// Removes the node from the NodeManager and any associated connections
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode()
        {
            NodeManager.Instance.ActiveNode = null;
            NodeManager.Instance.RemoveNode(Node);
            ConnectionManager.Instance.RemoveAssociatedConnections(Node);
        }
    }
}
