using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "Global Settings", menuName = "BranchCam/Global Settings", order = 1)]
public class GlobalSettingsData : ScriptableObject
{
    public TMP_FontAsset defaultFont;
    public float defaultFontSize = 25;

    public bool isMouseAllowed;
    public bool isKeyboardAllowed;

    public List<KeyCode> progressionKeyInputs = new List<KeyCode>();

    private void OnEnable()
    {
       defaultFont = defaultFont == null ? Resources.Load("Afacad-Regular SDF") as TMP_FontAsset : defaultFont;
    }
}
