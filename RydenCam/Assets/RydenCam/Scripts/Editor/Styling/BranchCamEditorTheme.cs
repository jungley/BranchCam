using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RydenCam.Editor.Styling
{
    internal static class BranchCamEditorTheme
    {
        public static readonly Color CanvasBackground = new Color32(26, 28, 32, 255);
        public static readonly Color PanelBackground = new Color32(38, 42, 48, 255);
        public static readonly Color PanelBackgroundElevated = new Color32(46, 51, 58, 255);
        public static readonly Color ToolbarBackground = new Color32(30, 34, 39, 255);
        public static readonly Color BorderMuted = new Color32(77, 85, 96, 255);
        public static readonly Color TextPrimary = new Color32(235, 238, 242, 255);
        public static readonly Color TextSecondary = new Color32(170, 177, 186, 255);
        public static readonly Color Accent = new Color32(72, 141, 255, 255);

        public const int FontTitle = 16;
        public const int FontBody = 12;
        public const int FontCaption = 11;

        private static readonly Dictionary<Color32, Texture2D> SolidTextureCache = new Dictionary<Color32, Texture2D>();

        public static Texture2D GetSolidTexture(Color color)
        {
            Color32 key = color;
            if (SolidTextureCache.TryGetValue(key, out Texture2D texture))
            {
                return texture;
            }

            texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            SolidTextureCache[key] = texture;
            return texture;
        }

        public static GUIStyle CreateToolbarButtonStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
            style.normal.textColor = TextPrimary;
            style.hover.textColor = Color.white;
            style.active.textColor = Color.white;
            style.focused.textColor = Color.white;
            style.fontSize = FontBody;
            style.fontStyle = FontStyle.Normal;
            style.alignment = TextAnchor.MiddleCenter;
            style.padding = new RectOffset(12, 12, 6, 6);
            style.margin = new RectOffset(4, 4, 4, 4);
            style.fixedHeight = 32f;
            style.normal.background = GetSolidTexture(PanelBackgroundElevated);
            style.hover.background = GetSolidTexture(new Color(PanelBackgroundElevated.r + 0.04f, PanelBackgroundElevated.g + 0.04f, PanelBackgroundElevated.b + 0.04f, 1f));
            style.active.background = GetSolidTexture(Accent);
            return style;
        }

        public static GUIStyle CreateToolbarPanelStyle()
        {
            GUIStyle style = new GUIStyle();
            style.normal.background = GetSolidTexture(ToolbarBackground);
            style.padding = new RectOffset(8, 8, 6, 6);
            return style;
        }
    }
}
