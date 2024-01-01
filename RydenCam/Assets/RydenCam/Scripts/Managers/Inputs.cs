using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Inputs 
{
    public static bool DownKey => Input.GetKeyDown(KeyCode.DownArrow);
    public static bool UpKey => Input.GetKeyDown(KeyCode.UpArrow);

    public static int LeftClick = 0;

    public static bool ProgressionKey()
    {
        var validKeyPressed = new bool();

        if (GlobalSettings.Settings.isKeyboardAllowed)
        {
            foreach (var key in GlobalSettings.Settings.progressionKeyInputs)
            {
                if (Input.GetKeyDown(key))
                {
                    validKeyPressed = true;
                }
            }
        }

        if (GlobalSettings.Settings.isMouseAllowed)
        {
            if (Input.GetMouseButtonDown(LeftClick))
            {
                validKeyPressed = true;
            }
        }

        return validKeyPressed;
    }
}
