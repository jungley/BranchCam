using System.Collections.Generic;
using UnityEngine;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;
using RydenCam.SequenceData;
using System.Linq;
using Assets.RydenCam.Scripts.BranchCamCC;
using System.Collections.ObjectModel;

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

        public List<ActorInfo> ActorsInScene()
        {
            var startNode = Nodes.OfType<StartNode>().FirstOrDefault();
            return startNode?.ActorsInScene ?? new List<ActorInfo>();
        }

        public ObservableCollection<NodeCC> Nodes { get; set; }

        public void Clear() => instance = new NodeManager();

        public void RemoveNode(NodeCC node)
        {
            if (node.TypeOfNode == NodeType.StartNode)
            {
                BranchCamEditor.startNodeAdded = false;
                
            }
            Nodes.Remove(node);
            
        }

        public void AddNode(NodeCC node) => Nodes.Add(node);
        public NodeCC GetNodeCC(int index) => Nodes[index];

        private NodeManager()
        {
            nodes = new List<EditorBaseNode>();
            Nodes = new ObservableCollection<NodeCC>();
        }


        ////////////////////////////////////////////////////////////////////////////


        private List<EditorBaseNode> nodes;
        public int Length => nodes.Count;
        public EditorStartNode StartNode => nodes.Find(n => n.TypeOfNode == NodeType.StartNode) as EditorStartNode;
        public List<EditorBaseNode> GetList() => nodes;
        public void RemoveNode(EditorBaseNode node)
        {
            if (node.TypeOfNode == NodeType.StartNode)
            {
                BranchCamEditor.startNodeAdded = false;
            }
            nodes.Remove(node);
        }

        public void AddNode(EditorBaseNode node) => nodes.Add(node);
        public EditorBaseNode GetNode(int index) => nodes[index];
        public EditorBaseNode FindNode(string id) => nodes.Find(n => n.node_id == id);

        public string GetSequenceName()
        {
            EditorStartNode startNodeRef = (EditorStartNode)NodeManager.Instance.StartNode;
            string name = string.IsNullOrWhiteSpace(startNodeRef.SequenceName) ? "NewDialogueFile" : startNodeRef.SequenceName;

            return name;
        }
        public bool IsValidSequence()
        {
            if (Length == 0)
            {
                BranchLog.Log("Cannot save an empty file!");
                return false;
            }
            else
            {
                return true;
            }
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


        //Check on this?
        //When NodeManager is updated,
        //The ActorManger should be updated via decorator pattern?

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
    }
}
