using Assets.RydenCam.Scripts.BranchCamCC;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Extensions
{
    public static class EditorGUILayoutExtensions
    {
        public static string SetTextAreaExpandable(string dialogueText, GUIStyle style, float areaHeight = 0, float textWidth = 0)
        {
            // Calculate the required height for the text content
            float textHeight = GetTextAreaHeight(dialogueText, textWidth);
            float calculatedHeight = Math.Max(textHeight + 10, areaHeight);

            dialogueText = EditorGUILayout.TextArea(dialogueText, style, GUILayout.Width(textWidth), GUILayout.Height(calculatedHeight));
            return dialogueText;
        }

        public static float GetTextAreaHeight(string dialogueText, float textWidth = 0)
        {
            // Calculate the required height for the text content
            float textHeight = GUI.skin.GetStyle("TextArea").CalcHeight(new GUIContent(dialogueText), textWidth);
            return textHeight;
        }
    }
}