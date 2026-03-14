using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions.DataStructures;
using RydenCam.BranchCamEditor.Managers;
using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.RydenCam.Scripts.NodeCommands
{
    public abstract class TalkableCommand: NodeCommand
    {
        private ITalkable talkableNode { get; set; }

        public TwoWayDictionary<int, Rect> TextAreaRectIndex { get; set; }

        public abstract void AddSpeakingEntry(int index);
        public abstract void RemoveSpeakingEntry(int index);
        public abstract int SpeakingEntriesCount { get; }

        public TalkableCommand(Node node) : base(node) 
        {
            talkableNode = node as ITalkable;  
        }

        public void AssignNewActor(int actorIndex)
        {
            var actors = NodeManager.Instance.ActorsInScene;
            if (actors == null || actorIndex < 0 || actorIndex >= actors.Count)
            {
                Debug.LogWarning("[RydenCam] Invalid actor index for assignment.");
                return;
            }

            var actorID = actors[actorIndex].ActorID;
            talkableNode.NodeConvodata.Actor = actors.FirstOrDefault(x => x.ActorID == actorID);
        }

#if UNITY_EDITOR
        public void ShowAddRemoveMenu(Vector2 mousePos)
        {
            GUIUtility.keyboardControl = 0;

            if (TextAreaRectIndex == null) return;

            Rect rect = TextAreaRectIndex.Values.Where(r => r.Contains(mousePos)).FirstOrDefault();
            if(!TextAreaRectIndex.GetByValue(rect, out int index)) return;

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
#endif

    }

}
