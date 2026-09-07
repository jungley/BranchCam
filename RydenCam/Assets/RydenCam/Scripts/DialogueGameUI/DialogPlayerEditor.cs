#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RydenCam.DialogueGameUI
{
    [CustomEditor(typeof(DialoguePlayer))]
    [ExecuteAlways]
    public class DialogPlayerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DialoguePlayer dialoguePlayer = (DialoguePlayer)target;

            string label = !string.IsNullOrEmpty(dialoguePlayer.DialogueFilePath)
                ? "Dialogue: " + dialoguePlayer.DialogueFilePath.Split('/').Last().Replace(".json", "")
                : "None Chosen";
            GUILayout.Label(label);

            if (GUILayout.Button("Choose Dialogue Folder"))
            {
                dialoguePlayer.ChooseFolder();
            }
        }
    }
}
#endif
