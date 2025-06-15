
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Extensions
{
    public static class EditorGUILayoutExtensions
    {
        public static string SetTextAreaExpandable(Rect nodeWindow, Dictionary<int, Rect> textAreaRect, int index, ref int buffer, string dialogueText, GUIStyle style, float areaHeight = 0, float textWidth = 0)
        {
            float calculatedHeight = calculateTextAreaHeight(dialogueText, textWidth, areaHeight);
            dialogueText = drawTextArea(dialogueText, style, textWidth, calculatedHeight);
            updateTextAreaRect(nodeWindow, textAreaRect, index, ref buffer, textWidth, calculatedHeight);

            return dialogueText;
        }

        public static float GetTextAreaHeight(string dialogueText, float textWidth = 0)
        {
            return GUI.skin.GetStyle("TextArea").CalcHeight(new GUIContent(dialogueText), textWidth);
        }


        //-- Helper methods for calculating height and drawing text area

        private static float calculateTextAreaHeight(string dialogueText, float textWidth, float areaHeight)
        {
            float textHeight = GetTextAreaHeight(dialogueText, textWidth);
            return Math.Max(textHeight + 10, areaHeight);
        }

        private static string drawTextArea(string dialogueText, GUIStyle style, float textWidth, float calculatedHeight)
        {
            return EditorGUILayout.TextArea(dialogueText, style, GUILayout.Width(textWidth), GUILayout.Height(calculatedHeight));
        }

        private static void updateTextAreaRect(Rect nodeWindow, Dictionary<int, Rect> textAreaRect, int index, ref int buffer, float textWidth, float calculatedHeight)
        {
            textAreaRect[index] = new Rect(5 + nodeWindow.x, buffer + nodeWindow.y, textWidth, calculatedHeight);
            buffer += (int)calculatedHeight + 7;
        }
    }
}