#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class TextEditor : EditorWindow
{
    string text = "";
    string path = "";

    [MenuItem("Window/Text Editor")]
    static void Init()
    {
        GetWindow<TextEditor>(false, "Text Editor");
    }

    void OnGUI()
    {
        var toolbarRect = DrawToolbar();
        float yOffset = toolbarRect.height + toolbarRect.y;
        var textRect = new Rect(toolbarRect.x, yOffset, position.width, position.height - yOffset - 4);

        var style = EditorStyles.textArea;
        style.richText = true;

        GUI.SetNextControlName("TextEditorArea");
        text = EditorGUI.TextArea(textRect, text, style);
    }

    Rect DrawToolbar()
    {
        var rect = EditorGUILayout.BeginHorizontal();
        EditorGUI.DrawRect(rect, Color.white * 0.5f);

        Button(new GUIContent("New"), NewFile, GUILayout.Width(48));
        Button(new GUIContent("Open"), OpenFile, GUILayout.Width(48));
        Button(new GUIContent("Save"), SaveFile, GUILayout.Width(48));

        EditorGUILayout.LabelField(Path.GetFileName(path), EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();

        return rect;
    }

    void NewFile()
    {
        text = "";
        path = "";
        FocusTextEditor();
    }

    void OpenFile()
    {
        path = EditorUtility.OpenFilePanel("Open text file", "", "*");
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            text = File.ReadAllText(path);
            FocusTextEditor();
        }
    }

    void SaveFile()
    {
        if (string.IsNullOrEmpty(path))
        {
            path = EditorUtility.SaveFilePanel("Save text file", "", "Untitled.txt", "txt");
        }

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, text);
            GUI.FocusControl(null);
        }
    }

    void FocusTextEditor()
    {
        EditorGUI.FocusTextInControl("TextEditorArea");
        EditorGUIUtility.editingTextField = true;
    }

    void Button(GUIContent content, Action action, params GUILayoutOption[] options)
    {
        if (GUILayout.Button(content, EditorStyles.miniButtonLeft, options))
        {
            action();
        }
    }
}
#endif
