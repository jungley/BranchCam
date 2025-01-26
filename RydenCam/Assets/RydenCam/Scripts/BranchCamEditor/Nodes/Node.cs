using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamCC
{
    //Serves as the Model in the MVVM pattern with 
    //NodeGraphViewModel (ViewModel)
    //NodeGraphEditorView (View)
    [System.Serializable]
    public abstract class Node : INode
    {
        //Ideally move this out of Node?
        public Vector2 EditorPosition;
        public float NodeWidth;
        public virtual float NodeHeight { get; set; }
        public int WindowId;

        public NodeType TypeOfNode;

        public ConnectionPoint PointIn;
        public List<ConnectionPoint> PointOut;
        public string NodeId;

        public Node(Vector2 position)
        {
            Guid guidVal = Guid.NewGuid();
            NodeId = guidVal.ToString();
            NodeWidth = 200;
            WindowId = new System.Random().Next(int.MinValue, int.MaxValue);

            EditorPosition = position;
        }

        public virtual Node GetNextNode() => PointOut.FirstOrDefault()?.ConnectedTo?.Node;


    }
}
