using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class ValidInputs 
{
    private static bool ValidMouseInputPressed => GlobalSettings.Settings.isMouseAllowed && Input.GetMouseButtonDown(0);
    private static bool ValidKeyPressed => GlobalSettings.Settings.isKeyboardAllowed && GlobalSettings.Settings.progressionKeyInputs.Any(Input.GetKeyDown);
    
    public static bool DownKeyPressed => Input.GetKeyDown(KeyCode.DownArrow);
    public static bool UpKeyPressed => Input.GetKeyDown(KeyCode.UpArrow);
    public static bool ProgressionInputPressed => ValidMouseInputPressed || ValidKeyPressed;
    public static bool ProgressionKeyPressed => ValidKeyPressed;
}
