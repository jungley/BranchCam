using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamCC
{
    //Serves as the Model in the MVVM pattern with 
    //NodeGraphViewModel (ViewModel)
    //NodeGraphEditorView (View)
    [System.Serializable]
    public abstract class NodeCC : INodeCC
    {
        //Most properties need to be fields NOT properties in order to serialize to JSON

        //Ideally move this out of Node?
        public Vector2 EditorPosition;
        public float NodeWidth;
        public virtual float NodeHeight { get; set; }
        public int WindowId;

        public NodeType TypeOfNode;

        public ConnectionPoint PointIn;
        public List<ConnectionPoint> PointOut;
        public string NodeId;

        //RSTODO move this?
        public static void OnClickRemoveConnection(Connection connection)
        {
            ConnectionManager.Instance.Remove(connection);
        }

        public NodeCC(Vector2 position)
        {
            Guid guidVal = Guid.NewGuid();
            NodeId = guidVal.ToString();
            NodeWidth = 200;
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
