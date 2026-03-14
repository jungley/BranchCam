using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.RydenCam.Scripts.NodeCommands
{
    public class DecisionNodeCommand : TalkableCommand
    {
        private DecisionNode node { get; set; }

        public override int SpeakingEntriesCount => node.DecisionOptions.Count;

        public DecisionNodeCommand(Node _node) : base(_node)
        {
            node = _node as DecisionNode;
            //CustomCameraCommand = new CustomCameraCommand(node);
        }
        public override void AddSpeakingEntry(int index)
        {
            node.DecisionOptions.Insert(index, "");
            node.PointOut.Add(new ConnectionPoint(node, ConnectionPointType.Out));
        }

        public override void RemoveSpeakingEntry(int index)
        {
            //Remove Connection
            ConnectionManager.Instance.Remove(node.PointOut[index]);

            //Remove Point
            node.DecisionOptions.RemoveAt(index);
            node.PointOut.RemoveAt(index);

        }

        public override ConnectionPoint SelectedEndPointFromNode(ConnectionPointType incomingType, Vector2 mousePos)
        {
         
            if (incomingType == ConnectionPointType.Out)
                return Node.PointIn;

            if (incomingType == ConnectionPointType.In)
            {
                if (TryGetHoveredDecisionIndex(mousePos, out int index) &&
                    index >= 0 &&
                    index < Node.PointOut.Count)
                {
                    return Node.PointOut[index];
                }
            }

            return null;
        }

        public bool TryGetHoveredDecisionRect(Vector2 mousePos, out Rect hoveredRect)
        {
            hoveredRect = default;

            if (TextAreaRectIndex == null)
            {
                return false;
            }

            if (!TryGetHoveredDecisionIndex(mousePos, out int hoveredIndex))
            {
                return false;
            }

            if (!TextAreaRectIndex.GetByKey(hoveredIndex, out hoveredRect))
            {
                return false;
            }

            return hoveredRect != default;
        }

        private bool TryGetHoveredDecisionIndex(Vector2 mousePos, out int hoveredIndex)
        {
            hoveredIndex = -1;
            if (TextAreaRectIndex == null) return false;

            // Iterate by key to avoid fragile reverse Rect->index lookups.
            foreach (int key in TextAreaRectIndex.Keys.OrderBy(x => x))
            {
                if (TextAreaRectIndex.GetByKey(key, out Rect optionRect) && optionRect.Contains(mousePos))
                {
                    hoveredIndex = key;
                    return true;
                }
            }

            return false;
        }
    }
}
