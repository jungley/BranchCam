using System.Collections.Generic;
using UnityEngine;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;
using RydenCam.SequenceData;
using System.Linq;

namespace RydenCam.BranchCamEditor.Managers
{    
    [System.Serializable]
    [ExecuteAlways]
    public class NodeManager
    {
        private static NodeManager instance;
        public static NodeManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NodeManager();
                }
                return instance;
            }
        }

        private List<EditorBaseNode> nodes;
        public int Length => nodes.Count;
        public EditorStartNode StartNode => nodes.Find(n => n.TypeOfNode == NodeType.StartNode) as EditorStartNode;
        public List<EditorBaseNode> GetList() => nodes;
        public void Clear() => instance = new NodeManager();
        public EditorBaseNode GetNode(int index) => nodes[index];
        public EditorBaseNode FindNode(string id) => nodes.Find(n => n.node_id == id);
        private NodeManager()
        {
            nodes = new List<EditorBaseNode>();
        }

        //This is for when the user clicks off or selects another node.
        //If the other node is also a node that uses custom camera, it will not use the position of the previously
        //created custom camera.
        public void EnsureUniqueCustomCameraSelection(EditorBaseNode curr)
        {
            foreach (EditorBaseNode node in nodes)
            {
                if (node != curr)
                {
                    node.SetCustomCameraPosition = null;
                }
            }
            
        }


        public void ReplaceActorInfo(string previousActorName, ActorInfo newActorInfo)
        {
            if (string.IsNullOrEmpty(previousActorName))
                return;

            foreach (var node in nodes.OfType<IPositionalNode>()
                                        .Where(posNode => posNode.NodeConvodata.Actor.ActorName == previousActorName))
            {
                node.NodeConvodata.Actor.ActorName = newActorInfo.ActorName;
                node.NodeConvodata.Actor.ActorGO = newActorInfo.ActorGO;
            }
        }



#if UNITY_EDITOR
        public void RemoveNode(EditorBaseNode node)
        {
            if (node.TypeOfNode == NodeType.StartNode)
            {
                BranchCamEditor.startNodeAdded = false;
            }
            nodes.Remove(node);
        }
#endif

        public void AddNode(EditorBaseNode node) => nodes.Add(node);
    }
}
