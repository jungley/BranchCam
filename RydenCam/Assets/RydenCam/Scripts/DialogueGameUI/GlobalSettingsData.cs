using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Global Settings", menuName = "BranchCam/Global Settings", order = 1)]
public class GlobalSettingsData : ScriptableObject
{
    public Font defaultFont;
    public float defaultFontSize = 32;

    public bool isMouseAllowed = true;
    public bool isKeyboardAllowed;

    public List<KeyCode> progressionKeyInputs = new List<KeyCode>();
}
