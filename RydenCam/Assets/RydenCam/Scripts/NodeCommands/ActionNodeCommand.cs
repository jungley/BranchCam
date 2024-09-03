using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.RydenCam.Scripts.NodeCommands
{
    public class ActionNodeCommand : INodeCommand
    {
        private ActionNode node { get; set; }

        public ActionNodeCommand(NodeCC _node)
        {
            node = _node;
        }


        public void RemoveNode(NodeCC node)
        {
            NodeManager.Instance.RemoveNode(node);
        }
    }
}
