using Assets.RydenCam.Scripts.BranchCamCC;
using Ink.Parsed;
using RydenCam.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.RydenCam.Scripts.Editor.NodeDrawer
{
    public static class NodeDrawerFactory
    {

        public static NodeDrawerBase CreateNodeDrawer(Node node)
        {
            if (node == null) return null;

            switch(node.TypeOfNode)
            {
                case NodeType.StartNode:
                    return new StartNodeDrawer(node);
                case NodeType.DialogueNode:
                    return new DialogueNodeDrawer(node);
                case NodeType.DecisionNode:
                    return new DecisionNodeDrawer(node);
                case NodeType.ActionNode:
                    return new ActionNodeDrawer(node);
                default:
                    throw new ArgumentException("Unsupported node type " + node.TypeOfNode.ToString());

            }
        }

    }
}
