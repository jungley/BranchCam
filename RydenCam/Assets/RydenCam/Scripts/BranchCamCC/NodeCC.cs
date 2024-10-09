using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamCC
{
    //Serves as the Model in the MVVM pattern with 
    //NodeGraphViewModel (ViewModel)
    //NodeGraphEditorView (View)
    public abstract class NodeCC : INodeCC
    {
        //Ideally move this out of Node?
        public Vector2 EditorPosition { get; set; }
        public virtual float NodeWidth { get; set; } = 200;
        public virtual float NodeHeight { get; set; }
        public int WindowId { get; set; }

        public virtual NodeType TypeOfNode { get; set; }

        public ConnectionPoint PointIn;
        public List<ConnectionPoint> PointOut;

        public string NodeId { get; set; }

        public NodeCC(Vector2 position)
        {
            Guid guidVal = Guid.NewGuid();
            NodeId = guidVal.ToString();
            WindowId = new System.Random().Next(int.MinValue, int.MaxValue);

            EditorPosition = position;
        }

        //RS TODO - Check this
        public bool ContainsPoint(ConnectionPoint point)
        {
            return (point == PointIn || PointOut.Contains(point));
        }

    }
}
