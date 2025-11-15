using RydenCam.Editor.Ribbon.RibbonItem;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RydenCam.Editor.Ribbon
{
    public class RibbonRenderer
    {
        private readonly RibbonDefinition definition;
        private readonly Dictionary<string, bool> dropdownState = new Dictionary<string, bool>();

        private Texture2D targetRibbonTexture { get; set; }

        public RibbonRenderer(RibbonDefinition def)
        {
            //Button Header Texture
            targetRibbonTexture = new Texture2D(1, 1);
            targetRibbonTexture.SetPixel(0, 0, Color.gray);
            targetRibbonTexture.Apply();

            definition = def;
        }

        public void Draw()
        {
            float availableWidth = EditorGUIUtility.currentViewWidth;

            var panelStyle = new GUIStyle();
            panelStyle.normal.background = targetRibbonTexture;

            GUILayout.BeginHorizontal(panelStyle, GUILayout.Width(availableWidth));
            //Weird bug requires a width of 1f to push everything to the left
            GUILayout.BeginHorizontal(GUILayout.Width(1f));

            foreach (var item in definition.Items)
            {
                if (item is RibbonButton btn)
                {
                    if (GUILayout.Button(btn.Label,
                        GUILayout.Width(btn.Width),
                        GUILayout.Height(btn.Height)))
                        {
                            btn.Action?.Invoke();
                        }
                }
                else if (item is RibbonDropdown dropdown)
                {
                    GUILayout.BeginVertical();

                        bool visible = dropdownState.ContainsKey(dropdown.Label) && dropdownState[dropdown.Label];
                    if (GUILayout.Button(dropdown.Label,
                        GUILayout.Width(dropdown.Width),
                        GUILayout.Height(dropdown.Height)))
                        {
                            dropdownState[dropdown.Label] = !visible;
                        }

                        if (dropdownState.ContainsKey(dropdown.Label) && dropdownState[dropdown.Label])
                        {
                            foreach (var opt in dropdown.Options)
                            {
                                if (GUILayout.Button(opt.Label,
                                    GUILayout.Width(opt.Width),
                                    GUILayout.Height(opt.Height)))
                                    {
                                        opt.Action?.Invoke();
                                    }
                            }
                        }

                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
        }
    }
}