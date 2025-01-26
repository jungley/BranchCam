using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RydenCam.DialogueGameUI
{
#if UNITY_EDITOR
    [CustomEditor(typeof(DialoguePlayer))]
    [ExecuteAlways]
    public class DialogPlayerEditor : Editor
    {
        
        public string DialogueFolder;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DialoguePlayer dialoguePlayer = (DialoguePlayer)target;

            GUILayout.Label(dialoguePlayer.DialogueFolder != null 
                || dialoguePlayer.DialogueFolder == "" 
                ? "Dialogue: " + dialoguePlayer.DialogueFolder.Split('/').Last() 
                : "None Chosen");

            if (GUILayout.Button("Choose Dialogue Folder"))
            {
                dialoguePlayer.ChooseFolder();
            }
        }
    }
#endif
}
