using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.NodeCommands;
using RydenCam.BranchCamEditor.Managers;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.RydenCam.Scripts.Editor.NodeDrawer
{
    /// <summary>
    /// Node related dialogue being spoken.
    /// </summary>
    public abstract class TalkableDrawerNode : NodeDrawerBase
    {
        public TalkableDrawerNode(Node _node) : base(_node) { }
        protected Dictionary<int, Rect> TextAreaRect { get; set; }

        protected TalkableCommand command { get; set; }

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
                command.AddSpeakingEntry(index + 1);
            });
            menu.AddItem(new GUIContent("Remove"), false, () =>
            {
                command.RemoveSpeakingEntry(index);

            });
            menu.ShowAsContext();
        }
    }
}
