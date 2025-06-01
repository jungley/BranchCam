using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RibbonBuilder
{
    public Rect ButtonPanelArea => new Rect(0, 0, 1000, 30);
    private int standardButtonHeight => 30;
    private int standardButtonDropdownHeight => 15;

    private static GUIStyle panelstyle_button = new GUIStyle();
    private Texture2D targetTextureButtonHeader = new Texture2D(1, 1);
    private NodeGraphViewModel viewModel;

    private Dictionary<string, Action> fileOptions;
    private Dictionary<string, Action> previewOptions;

    private bool showDropdown_file = false;
    private bool showPreview_preview = false;

    public RibbonBuilder(NodeGraphViewModel viewM)
    {
        viewModel = viewM;
        panelstyle_button.normal.background = targetTextureButtonHeader;
        InitializeDropdownActions();
    }

    public void DrawRibbon()
    {
        using (var horizontalScope = new GUILayout.HorizontalScope(panelstyle_button, GUILayout.Width(EditorGUIUtility.currentViewWidth), GUILayout.Height(30)))
        {
            DrawDropdown(RibbonButtonNames.File, new string[] { RibbonButtonNames.New, RibbonButtonNames.Open, RibbonButtonNames.Save, RibbonButtonNames.SaveAs }, fileOptions, visibilityToggle: ref showDropdown_file);

            DrawButton(name: RibbonButtonNames.Save, buttonWidth: 65, standardButtonHeight, action: viewModel.Save);

            DrawDropdown(RibbonButtonNames.PreviewRender, new string[] { RibbonButtonNames.PreviewRender_CornerPreview, RibbonButtonNames.PreviewRender_NodePreview }, previewOptions, visibilityToggle: ref showPreview_preview, width: 130);

            DrawButton(name: RibbonButtonNames.GlobalSettings, buttonWidth: 65, standardButtonHeight, action: viewModel.LocateGlobalSettings);

            DrawButton(name: RibbonButtonNames.InkleScriptView, buttonWidth: 120, standardButtonHeight, action: null); /* future TODO action */
        }
    }

    public void DrawButton(string name, int buttonWidth, int buttonHeight, Action action)
    {
        if (GUILayout.Button(name, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
        {
            action?.Invoke();
        }
    }

    private void DrawDropdown(string label, string[] options, Dictionary<string, Action> actionLookup, ref bool visibilityToggle, int width = 100)
    {
        using (var scope = new GUILayout.VerticalScope(GUILayout.Width(width)))
        {
            // Add some space from top, formatting issue
            GUILayout.Space(2);

            // Draw the main button
            if (GUILayout.Button(label, GUILayout.Width(width), GUILayout.Height(standardButtonHeight)))
            {
                visibilityToggle = !visibilityToggle;
            }

            if (visibilityToggle)
            {
                foreach (var option in options)
                {
                    DrawButton(name: option, buttonWidth: width, standardButtonDropdownHeight, action: actionLookup[option]);
                }
            }
        }
    }

    private void InitializeDropdownActions()
    {
        //For File Options
        fileOptions = new Dictionary<string, Action>
        {
            {
                RibbonButtonNames.New, () =>
                {
                    viewModel.NewFile();
                    showDropdown_file = false;
                }
            },
            {
                RibbonButtonNames.Open, () =>
                {
                    viewModel.Open();
                    showDropdown_file = false;
                }
            },
            {
                RibbonButtonNames.Save, () =>
                {
                    viewModel.Save();
                    showDropdown_file = false;
                }
            },
            {
                RibbonButtonNames.SaveAs, () =>
                {
                    viewModel.SaveAs();
                    showDropdown_file = false;
                }
            }
        };



        //For Preview Options
        previewOptions = new Dictionary<string, Action>
        {
            {
                RibbonButtonNames.PreviewRender_CornerPreview, () =>
                {
                    //viewModel.ToggleCornerPreviewRender();
                    showPreview_preview = false;
                }
            },
            {
                RibbonButtonNames.PreviewRender_NodePreview, () =>
                {
                    //viewModel.ToggleNodePreviewRender();
                    showPreview_preview = false;
                }
            }
        };
    }

    public static class RibbonButtonNames
    {
        public static string File => "File";
        public static string Utility => "Utility";
        public static string GlobalSettings => "Global Settings";
        public static string PreviewRender => "Preview Render";
        public static string PreviewRender_CornerPreview => "Corner Preview";
        public static string PreviewRender_NodePreview => "Node Preview";

        public static string InkleScriptView => "Inkle Script View";
        public static string SaveAs => "Save As";
        public static string Save => "Save";
        public static string Open => "Open";
        public static string New => "New";
    }
}