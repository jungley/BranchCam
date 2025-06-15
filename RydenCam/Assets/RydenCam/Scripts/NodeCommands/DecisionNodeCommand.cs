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
            CustomCameraCommand = new CustomCameraCommand(node);
        }
        public override void AddSpeakingEntry(int index)
        {
            node.DecisionOptions.Insert(index, "");
            node.PointOut.Add(new ConnectionPoint(node, ConnectionPointType.Out));
        }

        public override void RemoveSpeakingEntry(int index)
        {
            node.DecisionOptions.RemoveAt(index);
            node.PointOut.RemoveAt(index);
        }

        /*
        public void HighlightSpeakingEntry(Vector2 mousePos, Dictionary<int, Rect> TextAreaRect)
        {
            foreach (var entry in TextAreaRect)
            {
                if (entry.Value.Contains(mousePos))
                {
                    // Highlight the text area
                    GUI.DrawTextureWithTexCoords(entry.Value, HighlightTex, new Rect(0, 0, 1, 1.0f));
                    break;
                }
            }
        }
        */
    }
}
