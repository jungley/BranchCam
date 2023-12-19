using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public static class VisualElementExtensions
{
    public static VisualElement SetLabelStyle(this VisualElement visualElement, LabelStyle labelStyle)
    {
        #region Set Variable Values
        var largeFont = 18;
        var mediumFont = 14;
        var smallFont = 12;

        var largeSpacing = 10;
        var mediumSpacing = 7.5f;
        var smallSpacing = 2;

        var brightGrey = new Color(0.85f, 0.85f, 0.85f);
        var mediumGrey = new Color(0.8f, 0.8f, 0.8f);
        var darkGrey = new Color(0.75f, 0.75f, 0.75f);
        #endregion

        switch (labelStyle)
        {
            case LabelStyle.Header:
                ApplyLabelStyle(visualElement, largeFont, brightGrey, FontStyle.Bold, largeSpacing);
                break;
            case LabelStyle.SubHeader:
                ApplyLabelStyle(visualElement, mediumFont, mediumGrey, FontStyle.Bold, mediumSpacing);
                break;
            case LabelStyle.Label:
                ApplyLabelStyle(visualElement, smallFont, darkGrey, FontStyle.Normal, smallSpacing);
                break;
        }

        return visualElement;
    }

    private static void ApplyLabelStyle(VisualElement visualElement, int fontSize, Color color, FontStyle fontStyle, float verticalSpacing)
    {
        visualElement.style.fontSize = fontSize;
        visualElement.style.color = color;
        visualElement.style.unityFontStyleAndWeight = fontStyle;

        visualElement.SetMargins(verticalSpacing, verticalSpacing);
    }

    public static VisualElement SetFontStyle(this VisualElement visualElement, FontStyle fontStyle)
    {
        visualElement.style.unityFontStyleAndWeight = fontStyle;

        return visualElement;
    }


    public static VisualElement AddUnderline(this VisualElement visualElement)
    {
        visualElement.style.borderBottomColor = new Color(0.8f, 0.8f, 0.8f);

        visualElement.style.borderBottomWidth = 1;

        visualElement.style.width = StyleKeyword.Auto;
        return visualElement;
    }

    public static VisualElement SetMargins(this VisualElement visualElement, float marginBottom = 0, float marginTop = 0, float marginLeft = 0, float marginRight = 0)
    {
        visualElement.style.marginBottom = marginBottom;
        visualElement.style.marginTop = marginTop;
        visualElement.style.marginLeft = marginLeft;
        visualElement.style.marginRight = marginRight;

        return visualElement;
    }

    public static VisualElement Indent(this VisualElement visualElement)
    {
        var indentValue = 35;

        visualElement.style.marginLeft = indentValue;
        visualElement.style.marginRight = indentValue;

        return visualElement;
    }

    public static VisualElement SetSize(this VisualElement visualElement, float? width = null, float? height = null)
    {
        if (width.HasValue) visualElement.style.width = width.Value;
        if (height.HasValue) visualElement.style.height = height.Value;

        return visualElement;
    }

    public static VisualElement SetText(this VisualElement visualElement, string text)
    {
        visualElement.Add(new Label(text));

        return visualElement;
    }

    public static VisualElement FlexGrow(this VisualElement visualElement)
    {
        visualElement.style.flexGrow = 1;

        return visualElement;
    }
}
public enum LabelStyle
{
    Header,
    SubHeader,
    Label
}