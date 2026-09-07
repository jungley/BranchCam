using RydenCam.Editor.Ribbon.RibbonItem;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using RydenCam.Editor.Styling;

namespace RydenCam.Editor.Ribbon
{
    public class RibbonRenderer
    {
        private readonly RibbonDefinition definition;
        private readonly Dictionary<string, bool> dropdownState = new Dictionary<string, bool>();

        private GUIStyle toolbarPanelStyle;
        private GUIStyle toolbarButtonStyle;

        public RibbonRenderer(RibbonDefinition def)
        {
            definition = def;
        }

        public void Draw(float availableWidth)
        {
            // EditorStyles is not available during some OnEnable/hot-reload phases.
            // Build GUI styles lazily on the first actual GUI draw.
            toolbarPanelStyle ??= BranchCamEditorTheme.CreateToolbarPanelStyle();
            toolbarButtonStyle ??= BranchCamEditorTheme.CreateToolbarButtonStyle();

            GUILayout.BeginHorizontal(toolbarPanelStyle, GUILayout.Width(availableWidth), GUILayout.Height(50));
            //Weird bug requires a width of 1f to push everything to the left
            GUILayout.BeginHorizontal(GUILayout.Width(1f));

            foreach (var item in definition.Items)
            {
                if (item is RibbonButton btn)
                {
                    float buttonHeight = Mathf.Max(btn.Height, 34f);
                    if (GUILayout.Button(btn.Label, toolbarButtonStyle,
                        GUILayout.Width(btn.Width),
                        GUILayout.Height(buttonHeight)))
                        {
                            btn.Action?.Invoke();
                        }
                }
                else if (item is RibbonDropdown dropdown)
                {
                    GUILayout.BeginVertical();

                    bool visible = dropdownState.ContainsKey(dropdown.Label) && dropdownState[dropdown.Label];
                    float dropdownHeight = Mathf.Max(dropdown.Height, 34f);
                    if (GUILayout.Button(dropdown.Label, toolbarButtonStyle,
                        GUILayout.Width(dropdown.Width),
                        GUILayout.Height(dropdownHeight)))
                        {
                            dropdownState[dropdown.Label] = !visible;
                        }

                        if (dropdownState.ContainsKey(dropdown.Label) && dropdownState[dropdown.Label])
                        {
                            foreach (var opt in dropdown.Options)
                            {
                                float optionHeight = Mathf.Max(opt.Height, 28f);
                                if (GUILayout.Button(opt.Label, toolbarButtonStyle,
                                    GUILayout.Width(opt.Width),
                                    GUILayout.Height(optionHeight)))
                                    {
                                        opt.Action?.Invoke();
                                        dropdownState[dropdown.Label] = false;
                            }
                            }
                        }

                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
        }

        public void Draw()
        {
            Draw(EditorGUIUtility.currentViewWidth);
        }
    }
}
