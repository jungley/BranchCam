using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class InputWrapper 
{
    private static bool ValidMouseInputPressed => GlobalSettings.Settings.isMouseAllowed && Input.GetMouseButtonDown(0);
    private static bool ValidKeyBoardKeyPressed => GlobalSettings.Settings.isKeyboardAllowed && GlobalSettings.Settings.progressionKeyInputs.Any(Input.GetKeyDown);
    
    public static bool DownKeyPressed => Input.GetKeyDown(KeyCode.DownArrow);
    public static bool UpKeyPressed => Input.GetKeyDown(KeyCode.UpArrow);
    public static bool ProgressionKeyPressed => ValidMouseInputPressed || ValidKeyBoardKeyPressed;
}
