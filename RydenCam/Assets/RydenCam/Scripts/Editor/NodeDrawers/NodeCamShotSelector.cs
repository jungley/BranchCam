

using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
using Assets.RydenCam.Scripts.Editor.CameraShotEdtior;
using RydenCam.BranchCamEditor.BranchCam;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Codice.Client.BaseCommands.Import.Commit;

namespace Assets.RydenCam.Scripts.Editor.NodeDrawers
{
    //Used In Dialogue and Decision Nodes
    //The View for the Camera Options on nodes
    public class NodeCamShotSelector
    {
        private GUIStyle labelStyleHead_Panel { get; set; }
        private ITalkable currentNode { get; set; }

        public event Action UpdateShotRender;
        public NodeCamShotSelector(ITalkable node, GUIStyle _inspectorText, GUIStyle _labelStyleHead_Panel)
        {
            currentNode = node;
            //currentCommand = new CustomCameraCommand(node);
            labelStyleHead_Panel = _labelStyleHead_Panel;
        }

        public void DrawUICamCompOptions()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shot Composition", labelStyleHead_Panel);
            EditorGUILayout.Space();

            // Create or reuse a larger font style
            GUIStyle popupStyle = new GUIStyle(EditorStyles.popup);
            popupStyle.fontSize = 14; // increase font size
            popupStyle.fixedHeight = 24; // increase control height if needed
            popupStyle.alignment = TextAnchor.MiddleLeft;

            string[] names = CameraShotsManager.Instance.CameraShots.Select(x => x.ShotName).ToArray();  

            int selectedIndex = Mathf.Max(0, CameraShotsManager.Instance.CameraShots.IndexOf(currentNode.NodeConvodata.ShotConfig));

            int newIndex = EditorGUILayout.Popup(selectedIndex, names, popupStyle, GUILayout.Width(250));

            // Return the selected object
            currentNode.NodeConvodata.ShotConfig = CameraShotsManager.Instance.CameraShots[newIndex];
        }
    }
}