using UnityEngine;
using System.Linq;
using System;

public class ValidInputs
{
    private static bool ValidMouseInputPressed => GlobalSettings.Settings.isMouseAllowed && Input.GetMouseButtonDown(0);
    private static bool ValidKeyPressed => GlobalSettings.Settings.isKeyboardAllowed && GlobalSettings.Settings.progressionKeyInputs.Any(Input.GetKeyDown);
    public static bool DownKeyPressed => Input.GetKeyDown(KeyCode.DownArrow);
    public static bool UpKeyPressed => Input.GetKeyDown(KeyCode.UpArrow);
    public static bool ProgressionInputPressed => ValidMouseInputPressed || ValidKeyPressed;
    public static bool ProgressionKeyPressed => ValidKeyPressed;

    // Event to notify subscribers of changes
    public static event Action OnValidInput;

    public static bool IsDecionsMakingLocked { get; set; }

    public static void Update()
    {
        if (IsDecionsMakingLocked) return;

        // Mouse and keyboard can be pressed in the same frame. Progress only once.
        if (ProgressionInputPressed)
            OnValidInput?.Invoke();
    }
}
