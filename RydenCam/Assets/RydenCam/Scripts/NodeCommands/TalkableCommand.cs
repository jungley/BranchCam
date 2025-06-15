using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.RydenCam.Scripts.NodeCommands
{
    public abstract class TalkableCommand: NodeCommand
    {
        private ITalkable talkableNode { get; set; }

        public CustomCameraCommand CustomCameraCommand { get; set; }

        public Dictionary<int, Rect> TextAreaRect { get; set; }

        public abstract void AddSpeakingEntry(int index);
        public abstract void RemoveSpeakingEntry(int index);
        public abstract int SpeakingEntriesCount { get; }

        public TalkableCommand(Node node) : base(node) 
        {
            talkableNode = node as ITalkable;  
        }

        public void AssignNewActor(int actorIndex)
        {
            var actor = NodeManager.Instance.ActorsInScene[actorIndex].ActorID;
            talkableNode.NodeConvodata.Actor = NodeManager.Instance.ActorsInScene.Where(x => x.ActorID == actor).FirstOrDefault();
            talkableNode.NodeConvodata.ShotConfig.Actor = talkableNode.NodeConvodata.Actor.ActorName;
        }

        public void ShowAddRemoveMenu(Vector2 mousePos)
        {
            GUIUtility.keyboardControl = 0;
            int index = -1;
            foreach (var kvp in TextAreaRect)
            {
                if (kvp.Value.Contains(mousePos))
                {
                    index = kvp.Key;
                    break;
                }
            }
            if (index == -1) return;
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add"), false, () =>
            {
                AddSpeakingEntry(index + 1);
            });
            if (SpeakingEntriesCount > 1)
            {
                menu.AddItem(new GUIContent("Remove"), false, () =>
                {
                    RemoveSpeakingEntry(index);
                });
            }
            menu.ShowAsContext();
        }

    }

}
