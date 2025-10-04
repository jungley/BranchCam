using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions.DatatStructures;
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
            try
            {
                var actorID = NodeManager.Instance.ActorsInScene[actorIndex].ActorID;
                talkableNode.NodeConvodata.Actor = NodeManager.Instance.ActorsInScene.Where(x => x.ActorID == actorID).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error assigning new actor: {ex.Message}");
            }

        }

        public void ShowAddRemoveMenu(Vector2 mousePos)
        {
            GUIUtility.keyboardControl = 0;

            Rect rect = TextAreaRectIndex.Values.Where(rect => rect.Contains(mousePos)).FirstOrDefault();
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

    }

}
