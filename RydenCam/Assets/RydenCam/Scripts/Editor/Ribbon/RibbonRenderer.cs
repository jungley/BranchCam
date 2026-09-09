using RydenCam.Editor.Ribbon.RibbonItem;
using UnityEditor;
using UnityEngine;
using RydenCam.Editor.Styling;

namespace RydenCam.Editor.Ribbon
{
    public class RibbonRenderer
    {
        // Graph drawing and hit testing share the ribbon's actual height.
        public const float Height = 30f;
        private readonly RibbonDefinition definition;
        private GUIStyle toolbarPanelStyle;
        private GUIStyle toolbarButtonStyle;

        public RibbonRenderer(RibbonDefinition def)
        {
            definition = def;
        }

        public void Draw(float availableWidth)
        {
            // Create styles during GUI drawing, after Unity's skin is available.
            toolbarPanelStyle ??= BranchCamEditorTheme.CreateToolbarPanelStyle();
            toolbarButtonStyle ??= BranchCamEditorTheme.CreateToolbarButtonStyle();

            using (new GUILayout.HorizontalScope(toolbarPanelStyle,
                GUILayout.Width(availableWidth), GUILayout.Height(Height)))
            {
                foreach (var item in definition.Items)
                {
                    if (item is RibbonButton button) DrawButton(button);
                    else if (item is RibbonDropdown dropdown) DrawDropdown(dropdown);
                }
                GUILayout.FlexibleSpace();
            }
        }

        private void DrawButton(RibbonButton button)
        {
            if (GUILayout.Button(button.Label, toolbarButtonStyle,
                GUILayout.Width(button.Width), GUILayout.Height(button.Height)))
                button.Action?.Invoke();
        }

        private void DrawDropdown(RibbonDropdown dropdown)
        {
            Rect anchor = GUILayoutUtility.GetRect(dropdown.Width, dropdown.Height);
            if (!GUI.Button(anchor, dropdown.Label, toolbarButtonStyle)) return;

            // A popup keeps File options from expanding the ribbon over the canvas.
            var menu = new GenericMenu();
            foreach (var option in dropdown.Options)
                menu.AddItem(new GUIContent(option.Label), false, () => option.Action?.Invoke());
            menu.DropDown(anchor);
        }

        public void Draw()
        {
            Draw(EditorGUIUtility.currentViewWidth);
        }
    }
}