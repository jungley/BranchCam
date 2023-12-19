using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

[CustomEditor(typeof(GlobalSettingsData))]
public class GlobalSettingsEditor : Editor
{
    private SerializedProperty isMouseAllowed;
    private SerializedProperty isKeyboardAllowed;

    private GlobalSettingsData globalSettingsData;

    private GlobalSettingsHelper globalSettingsHelper;

    private void OnEnable()
    {

        isMouseAllowed = serializedObject.FindProperty("isMouseAllowed");
        isKeyboardAllowed = serializedObject.FindProperty("isKeyboardAllowed");

        globalSettingsData = (GlobalSettingsData)target;

        globalSettingsHelper = new GlobalSettingsHelper(globalSettingsData);
    }

    public override VisualElement CreateInspectorGUI()
    {
        serializedObject.Update();

        var root = new VisualElement();

        root.Add(new VisualElement()
            .SetText("BranchCam UI Configuration")
            .SetLabelStyle(LabelStyle.Header));

        CreateFontSettingsField(root);

        CreateUIProgressionSettingsField(root);

        EditorUtility.SetDirty(target);

        return root;
    }

    private void CreateFontSettingsField(VisualElement root)
    {
        root.Add(new VisualElement()
            .SetText("Font Settings")
            .SetLabelStyle(LabelStyle.SubHeader));

        root.Add(new PropertyField(serializedObject.FindProperty("defaultFont"), "Default Font").Indent());
        root.Add(new PropertyField(serializedObject.FindProperty("defaultFontSize"), "Font Size").Indent());

    }

    private void CreateUIProgressionSettingsField(VisualElement root)
    {
        root.Add(new VisualElement()
            .SetLabelStyle(LabelStyle.SubHeader)
            .SetText("UI Progression Settings"));

        var selectableInputContainer = new VisualElement() { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.NoWrap } }.Indent();

        selectableInputContainer.Add(new VisualElement()
            .SetLabelStyle(LabelStyle.Label)
            .SetMargins(marginRight: 35)
            .SetText("Allowed Inputs"));

        #region Mouse Toggle
        Toggle mouseToggle = new Toggle() { };

        mouseToggle.BindProperty(isMouseAllowed);

        selectableInputContainer.Add(mouseToggle);

        selectableInputContainer.Add(new VisualElement()
            .SetLabelStyle(LabelStyle.Label)
            .SetText("Mouse"));
        #endregion


        #region Keyboard Toggle
        Toggle keyboardToggle = new Toggle();
        keyboardToggle.BindProperty(isKeyboardAllowed);

        var keyboardProgressionUI = KeyboardInputProgression();

        keyboardToggle.RegisterValueChangedCallback(evt => globalSettingsHelper.KeyboardToggleEvent(keyboardProgressionUI, evt.newValue));

        selectableInputContainer.Add(keyboardToggle);

        selectableInputContainer.Add(new VisualElement()
            .SetLabelStyle(LabelStyle.Label)
            .SetText("Keyboard"));
        #endregion

        root.Add(selectableInputContainer);
        root.Add(keyboardProgressionUI);
    }

    private VisualElement KeyboardInputProgression()
    {
        var keyboardInputContainer = new VisualElement();

        keyboardInputContainer.Add(new VisualElement()
            .SetLabelStyle(LabelStyle.Label)
            .SetText("Keyboard Inputs")
            .AddUnderline()
            .SetSize(width: 95)
            .Indent());

        var inputContainer = new VisualElement() { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.NoWrap } }.Indent();

        inputContainer.Add(new VisualElement()
            .SetLabelStyle(LabelStyle.Label)
            .SetMargins(marginRight: 13)
            .SetText("Progression Key(s)"));

        Button addInputButton = new Button() { text = "Add New Input" };

        inputContainer.Add(addInputButton);

        keyboardInputContainer.Add(inputContainer);

        addInputButton.clicked += () =>
        {
            globalSettingsData.progressionKeyInputs.Add(KeyCode.Space);

            AddNewInput(keyboardInputContainer, "Space");
        };

        LoadKeyInputField(keyboardInputContainer);

        globalSettingsHelper.SetVisibility(keyboardInputContainer, isKeyboardAllowed.boolValue);

        return keyboardInputContainer;
    }

    private void AddNewInput(VisualElement container, string keyName)
    {
        var keyMenuContainer = new VisualElement() { style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.NoWrap, } }.Indent();

        var keyMenu = new ToolbarMenu()
        .SetLabelStyle(LabelStyle.Label)
        .SetMargins(marginTop: 5)
        .FlexGrow() as ToolbarMenu;

        keyMenu.text = keyName;

        Button deleteInputButton = new Button() { text = "X", style = { fontSize = 8, height = 20, alignSelf = Align.Center, marginTop = 5 } };

        deleteInputButton.clicked += () =>
        {
            globalSettingsHelper.RemoveKeyFromList(keyMenu);
            globalSettingsHelper.Delete(keyMenuContainer);
        };

        keyMenuContainer.Add(keyMenu);
        keyMenuContainer.Add(deleteInputButton);

        // Add KeyCode values to the menu
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            keyMenu.menu.AppendAction(keyCode.ToString(), x => globalSettingsHelper.OnKeyValueChange(keyCode, keyMenu, (KeyCode)Enum.Parse(typeof(KeyCode), keyMenu.text)));
        }

        container.Add(keyMenuContainer);
    }

    public void LoadKeyInputField(VisualElement container)
    {
        for (int i = 0; i < globalSettingsData.progressionKeyInputs.Count; i++)
        {
            AddNewInput(container, globalSettingsData.progressionKeyInputs[i].ToString());
        }
    }

}
