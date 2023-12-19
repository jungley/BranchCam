using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

public class GlobalSettingsHelper
{
    private GlobalSettingsData globalSettingsData;

    public GlobalSettingsHelper(GlobalSettingsData globalSettingsData)
    {
        this.globalSettingsData = globalSettingsData;
    }

    public void SetVisibility(VisualElement visualElement, bool isVisible)
    {
        visualElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void Delete(VisualElement visualElement)
    {
        visualElement.RemoveFromHierarchy();
    }

    public void KeyboardToggleEvent(VisualElement visualElement, bool isToggled)
    {
        SetVisibility(visualElement, isToggled);
    }
    public void OnKeyValueChange(KeyCode newKeyCode, ToolbarMenu keyMenu, KeyCode oldKeyCode)
    {
        var indexOfOldKey = globalSettingsData.progressionKeyInputs.IndexOf(oldKeyCode);

        globalSettingsData.progressionKeyInputs[indexOfOldKey] = newKeyCode;

        keyMenu.text = newKeyCode.ToString();
    }

    public void RemoveKeyFromList(ToolbarMenu toolbar)
    {
        var key = (KeyCode)Enum.Parse(typeof(KeyCode), toolbar.text);

        globalSettingsData.progressionKeyInputs.Remove(key);
    }
}
