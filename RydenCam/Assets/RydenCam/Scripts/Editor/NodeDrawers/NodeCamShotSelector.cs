

using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
using Assets.RydenCam.Scripts.Editor.CameraShotEditor;
using RydenCam.BranchCamEditor.BranchCam;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.RydenCam.Scripts.Editor.NodeDrawers
{
    //Used In Dialogue and Decision Nodes
    //The View for the Camera Options on nodes
    public class NodeCamShotSelector
    {
        private GUIStyle labelStyleHead_Panel { get; set; }
        private ITalkable currentNode { get; set; }

        public event Action UpdateShotRender;

        public int selectedShotIndex { get; set; } = 0;

        public NodeCamShotSelector(ITalkable node, GUIStyle _inspectorText, GUIStyle _labelStyleHead_Panel)
        {
            currentNode = node;
            //currentCommand = new CustomCameraCommand(node);
            labelStyleHead_Panel = _labelStyleHead_Panel;
            SynchronizeSelectedShot();
        }

        private void SynchronizeSelectedShot()
        {
            var shots = CameraShotsManager.Instance.CameraShots;
            if (shots == null || shots.Count == 0 || currentNode?.NodeConvodata == null)
                return;

            string currentShotId = currentNode.NodeConvodata.ShotConfig?.ShotId;
            int matchingIndex = shots.FindIndex(shot => !string.IsNullOrEmpty(currentShotId) && shot.ShotId == currentShotId);
            selectedShotIndex = matchingIndex >= 0 ? matchingIndex : 0;

            if (matchingIndex < 0)
                currentNode.NodeConvodata.ShotConfig = shots[selectedShotIndex];
        }

        public void DrawUICamCompOptions()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shot Composition", labelStyleHead_Panel);
            EditorGUILayout.Space();

            var shots = CameraShotsManager.Instance.CameraShots;
            if (shots == null || shots.Count == 0)
            {
                EditorGUILayout.LabelField("No camera shots available.");
                return;
            }

            GUIStyle popupStyle = new GUIStyle(EditorStyles.popup);
            popupStyle.fontSize = 14;
            popupStyle.fixedHeight = 24;
            popupStyle.alignment = TextAnchor.MiddleLeft;

            string[] names = shots.Select(x => x.ShotName).ToArray();

            if (selectedShotIndex < 0 || selectedShotIndex >= names.Length)
                selectedShotIndex = 0;

            if (currentNode.NodeConvodata.ShotConfig == null ||
                !shots.Any(shot => shot.ShotId == currentNode.NodeConvodata.ShotConfig.ShotId))
            {
                currentNode.NodeConvodata.ShotConfig = shots[selectedShotIndex];
                UpdateShotRender?.Invoke();
            }

            int index = EditorGUILayout.Popup(selectedShotIndex, names, popupStyle, GUILayout.Width(250));

            if (index < 0 || index >= shots.Count) index = 0;

            if (index != selectedShotIndex)
            {
                selectedShotIndex = index;
                currentNode.NodeConvodata.ShotConfig = shots[selectedShotIndex];
                UpdateShotRender?.Invoke();
            }
        }
    }
}
