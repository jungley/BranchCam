using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Managers;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.RydenCam.Scripts.NodeCommands
{
    public class DialogueNodeCommand : TalkableCommand
    {
        private DialogueNode node { get; set; }

        public override int SpeakingEntriesCount => node.NodeConvodata.DialogTextList.Count;

        public DialogueNodeCommand(Node _node) : base(_node)    
        {
            node = _node as DialogueNode;
            CustomCameraCommand = new CustomCameraCommand(node);
        }

        public override void AddSpeakingEntry(int index)
        {
            node.NodeConvodata.DialogTextList.Insert(index, string.Empty);
        }

        public override void RemoveSpeakingEntry(int index)
        {
           node.NodeConvodata.DialogTextList.RemoveAt(index);

        }

    }
}
