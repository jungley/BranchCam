using Assets.RydenCam.Scripts.BranchCamEditor.Serialization.Saveables;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.BranchCamEditor.Serialization;
using RydenCam.BranchCamEditor.Serialization.Saveables;
using RydenCam.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RydenCam.BranchCamEditor.Serialization
{
    /*
    //Combine move code from NodeSerializer to here?
    public class NodeConversionFactory
    {
        private readonly Dictionary<NodeType, Func<Saveable, EditorBaseNode>> createNodeMap = new Dictionary<NodeType, Func<Saveable, EditorBaseNode>>();
        private readonly Dictionary<NodeType, Func<EditorBaseNode, Saveable>> createSaveNodeMap = new Dictionary<NodeType, Func<EditorBaseNode, Saveable>>();

        public NodeConversionFactory()
        {
            createNodeMap[NodeType.StartNode] = savenode => new EditorStartNode(savenode);
            createNodeMap[NodeType.DialogueNode] = savenode => new EditorDialogueNode(savenode);
            createNodeMap[NodeType.DecisionNode] = savenode => new EditorDecisionNode(savenode);
            createNodeMap[NodeType.ActionNode] = savenode => new EditorActionNode(savenode);

            createSaveNodeMap[NodeType.StartNode] = node => new SaveableStartNode(node as EditorStartNode);
            createSaveNodeMap[NodeType.DialogueNode] = node => new SaveableDialogueNode(node as EditorDialogueNode);
            createSaveNodeMap[NodeType.DecisionNode] = node => new SaveableDecisionNode(node as EditorDecisionNode);
            createSaveNodeMap[NodeType.ActionNode] = node => new SaveableActionNode(node as EditorActionNode);
        }

        public EditorBaseNode CreateEditorNode(Saveable savenode)
        {
            if (createNodeMap.TryGetValue(savenode.TypeOfNode, out var createFunction))
            {
                return createFunction(savenode);
            }

            BranchLog.Error("Conversion issue occurred");
            return null;
        }

        public Saveable CreateSaveNode(EditorBaseNode node)
        {
            if (createSaveNodeMap.TryGetValue(node.TypeOfNode, out var createFunction))
            {
                return createFunction(node);
            }
            BranchLog.Error("Conversion issue occurred");
            return null;
        }
    }
    */
}
