using System.Collections.Generic;
using UnityEngine;
using RydenCam.Common;
using RydenCam.SequenceData;
using System.Linq;
using Assets.RydenCam.Scripts.BranchCamCC;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Node = Assets.RydenCam.Scripts.BranchCamCC.Node;
using Assets.RydenCam.Scripts.NodeCommands;

namespace RydenCam.BranchCamEditor.Managers
{
    [ExecuteAlways]    
    public class NodeManager : INotifyPropertyChanged
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
        public ObservableCollection<Node> Nodes { get; set; }


        // Add this dictionary to hold NodeCommands
        private readonly Dictionary<Node, NodeCommand> nodeCommands = new Dictionary<Node, NodeCommand>();
        public Dictionary<Node, NodeCommand> NodeCommands => nodeCommands;

        private Node activeNode { get; set; }
        public Node ActiveNode
        {
            get => activeNode;
            set
            {
                activeNode = value;
                OnPropertyChanged(nameof(ActiveNode));
            }
        }

        public StartNode StartNode => Nodes.ToList().Find(n => n.TypeOfNode == NodeType.StartNode) as StartNode;

        public static bool StartNodeAdded { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private NodeManager()
        {
            Nodes = new ObservableCollection<Node>();
        }

        public void LoadNodes(List<Node> nodes) => nodes.ForEach(n => { Nodes.Add(n); });

        public void AddNode(Node node) => Nodes.Add(node);

        public Node GetNode(int index) => Nodes[index];

        public Node FindNode(string id) => Nodes.ToList().Find(n => n.NodeId == id);

        public int Length => Nodes.Count;

        public void RemoveNode(Node node)
        {
            if (node.TypeOfNode == NodeType.StartNode)
            {
                StartNodeAdded = false;
            }
            Nodes.Remove(node);
        }

        public void Clear()
        {
            Nodes.Clear();
            ActiveNode = null;
            StartNodeAdded = false;
        }

        public string GetSequenceName()
        {
            StartNode startNodeRef = Nodes.OfType<StartNode>().FirstOrDefault();
            return string.IsNullOrWhiteSpace(startNodeRef.SequenceName) ? "NewDialogueFile" : startNodeRef.SequenceName;
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

        public List<ActorInfo> ActorsInScene => StartNode?.ActorsInScene.Where(actor => actor.ActorGO != null).ToList() ?? new List<ActorInfo>();
            
        public void ClearActorsInScene()
        {
            var startNode = Nodes.OfType<StartNode>().FirstOrDefault();
            if (startNode != null) startNode.ActorsInScene = new List<ActorInfo>();
        }

       
        //NodeCommand Management

        public void RegisterNodeCommand(Node node, NodeCommand command)
        {
            if (node == null || command == null) return;
            nodeCommands[node] = command;
        }

        public void UnregisterNodeCommand(Node node)
        {
            if (node == null) return;
            nodeCommands.Remove(node);
        }

        public NodeCommand GetNodeCommand(Node node)
        {
            if (node == null) return null;
            nodeCommands.TryGetValue(node, out var command);
            return command;
        }

    }
}
