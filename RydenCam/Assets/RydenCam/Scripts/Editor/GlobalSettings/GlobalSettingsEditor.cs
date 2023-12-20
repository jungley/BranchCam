using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;

/// <summary>
/// Overrides the GlobalSettingsData inspector and draws a new custom one. 
/// </summary>

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

        // Adds ALL keycodes to the menu. Could be redone to only include desired keycodes.
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyboardKey)))
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


    public enum KeyboardKey
    {
        None = KeyCode.None,
        A = KeyCode.A,
        B = KeyCode.B,
        C = KeyCode.C,
        D = KeyCode.D,
        E = KeyCode.E,
        F = KeyCode.F,
        G = KeyCode.G,
        H = KeyCode.H,
        I = KeyCode.I,
        J = KeyCode.J,
        K = KeyCode.K,
        L = KeyCode.L,
        M = KeyCode.M,
        N = KeyCode.N,
        O = KeyCode.O,
        P = KeyCode.P,
        Q = KeyCode.Q,
        R = KeyCode.R,
        S = KeyCode.S,
        T = KeyCode.T,
        U = KeyCode.U,
        V = KeyCode.V,
        W = KeyCode.W,
        X = KeyCode.X,
        Y = KeyCode.Y,
        Z = KeyCode.Z,
        Alpha0 = KeyCode.Alpha0,
        Alpha1 = KeyCode.Alpha1,
        Alpha2 = KeyCode.Alpha2,
        Alpha3 = KeyCode.Alpha3,
        Alpha4 = KeyCode.Alpha4,
        Alpha5 = KeyCode.Alpha5,
        Alpha6 = KeyCode.Alpha6,
        Alpha7 = KeyCode.Alpha7,
        Alpha8 = KeyCode.Alpha8,
        Alpha9 = KeyCode.Alpha9,
        BackQuote = KeyCode.BackQuote,
        Backslash = KeyCode.Backslash,
        Backspace = KeyCode.Backspace,
        CapsLock = KeyCode.CapsLock,
        Colon = KeyCode.Colon,
        Comma = KeyCode.Comma,
        Delete = KeyCode.Delete,
        DownArrow = KeyCode.DownArrow,
        End = KeyCode.End,
        Enter = KeyCode.Return,
        Equals = KeyCode.Equals,
        Escape = KeyCode.Escape,
        F1 = KeyCode.F1,
        F2 = KeyCode.F2,
        F3 = KeyCode.F3,
        F4 = KeyCode.F4,
        F5 = KeyCode.F5,
        F6 = KeyCode.F6,
        F7 = KeyCode.F7,
        F8 = KeyCode.F8,
        F9 = KeyCode.F9,
        F10 = KeyCode.F10,
        F11 = KeyCode.F11,
        F12 = KeyCode.F12,
        Home = KeyCode.Home,
        Insert = KeyCode.Insert,
        Keypad0 = KeyCode.Keypad0,
        Keypad1 = KeyCode.Keypad1,
        Keypad2 = KeyCode.Keypad2,
        Keypad3 = KeyCode.Keypad3,
        Keypad4 = KeyCode.Keypad4,
        Keypad5 = KeyCode.Keypad5,
        Keypad6 = KeyCode.Keypad6,
        Keypad7 = KeyCode.Keypad7,
        Keypad8 = KeyCode.Keypad8,
        Keypad9 = KeyCode.Keypad9,
        KeypadDivide = KeyCode.KeypadDivide,
        KeypadEnter = KeyCode.KeypadEnter,
        KeypadMinus = KeyCode.KeypadMinus,
        KeypadMultiply = KeyCode.KeypadMultiply,
        KeypadPeriod = KeyCode.KeypadPeriod,
        KeypadPlus = KeyCode.KeypadPlus,
        LeftAlt = KeyCode.LeftAlt,
        LeftArrow = KeyCode.LeftArrow,
        LeftBracket = KeyCode.LeftBracket,
        LeftControl = KeyCode.LeftControl,
        LeftShift = KeyCode.LeftShift,
        Minus = KeyCode.Minus,
        Numlock = KeyCode.Numlock,
        PageDown = KeyCode.PageDown,
        PageUp = KeyCode.PageUp,
        Pause = KeyCode.Pause,
        Period = KeyCode.Period,
        Plus = KeyCode.Plus,
        Print = KeyCode.Print,
        Quote = KeyCode.Quote,
        RightAlt = KeyCode.RightAlt,
        RightArrow = KeyCode.RightArrow,
        RightBracket = KeyCode.RightBracket,
        RightControl = KeyCode.RightControl,
        RightShift = KeyCode.RightShift,
        ScrollLock = KeyCode.ScrollLock,
        Semicolon = KeyCode.Semicolon,
        Slash = KeyCode.Slash,
        Space = KeyCode.Space,
        Tab = KeyCode.Tab,
        UpArrow = KeyCode.UpArrow
    }

}
