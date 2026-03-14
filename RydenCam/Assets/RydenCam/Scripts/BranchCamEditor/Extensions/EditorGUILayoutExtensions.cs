#if UNITY_EDITOR
using Assets.RydenCam.Scripts.BranchCamEditor.Extensions.DataStructures;
using System;
using UnityEditor;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Extensions
{
    public static class EditorGUILayoutExtensions
    {
        public static string SetTextAreaExpandable(Rect nodeWindow, TwoWayDictionary<int, Rect> textAreaRect, int index, ref int buffer, string dialogueText, GUIStyle style, float areaHeight = 0, float textWidth = 0)
        {
            float calculatedHeight = CalculateTextAreaHeight(dialogueText, textWidth, areaHeight);
            dialogueText = DrawTextArea(dialogueText, style, textWidth, calculatedHeight);
            UpdateTextAreaRect(nodeWindow, textAreaRect, index, ref buffer, textWidth, calculatedHeight);

            return dialogueText;
        }

        public static float GetTextAreaHeight(string dialogueText, float textWidth = 0)
        {
            if (GUI.skin == null) return 50;
            return GUI.skin.GetStyle("TextArea").CalcHeight(new GUIContent(dialogueText ?? string.Empty), textWidth);
        }

        private static float CalculateTextAreaHeight(string dialogueText, float textWidth, float areaHeight)
        {
            float textHeight = GetTextAreaHeight(dialogueText, textWidth);
            return Math.Max(textHeight + 10, areaHeight);
        }

        private static string DrawTextArea(string dialogueText, GUIStyle style, float textWidth, float calculatedHeight)
        {
            return EditorGUILayout.TextArea(dialogueText, style, GUILayout.Width(textWidth), GUILayout.Height(calculatedHeight));
        }

        private static void UpdateTextAreaRect(Rect nodeWindow, TwoWayDictionary<int, Rect> textAreaRect, int index, ref int buffer, float textWidth, float calculatedHeight)
        {
            textAreaRect.UpdateByKey(index, new Rect(5 + nodeWindow.x, buffer + nodeWindow.y, textWidth, calculatedHeight));
            buffer += (int)calculatedHeight + 7;
        }
    }
}
#endif
