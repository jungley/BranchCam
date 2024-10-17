using RydenCam.BranchCamEditor.Serialization;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.Common;
using UnityEditor;
using UnityEngine;

namespace RydenCam.DialogueGameUI
{
    public class DialoguePlayer : MonoBehaviour
    {

        [SerializeField] private GameObject _requiredDialogueCamera;
        public GameObject DialogueCamera
        {
            get { return _requiredDialogueCamera; }
            set
            {
                if (value != null)
                {
                    _requiredDialogueCamera = value;
                }
                else
                {
                    Debug.LogWarning("RequiredDialogueCamera cannot be set to null.");
                }
            }
        }

        [SerializeField] private GameObject _requiredDialogueCameraBrain;
        public GameObject DialogueCameraBrain
        {
            get
            { 
                return _requiredDialogueCameraBrain; 
            }
            set
            {
                _requiredDialogueCamera = value;
            }
        }


        [HideInInspector]
        public string DialogueFolder;
        public SequenceController SequenceControls;
        public bool IsDialogueRunning => SequenceControls.DialogueIsRunning;

        public void Start()
        {
            SequenceControls = new SequenceController(DialogueCamera, DialogueCameraBrain);
            SequenceControls.ToggleRelevantObjects(visibility: false);
        }

        
        public void Update()
        {
            if (!ValidInputs.ProgressionInputPressed) return;
            if (!SequenceControls.DialogueIsRunning) return;

            if (!SequenceControls.DecisionBeingMadeLock)
                SequenceControls.TraverseNodeNetwork();
        }


        /// <summary>
        /// StartSequence should be triggered by a trigger collider
        /// </summary>
        public void StartSequence()
        {
            LoadConversation();
            SequenceControls.SetUpSequence();
            SequenceControls.TraverseNodeNetwork();
        }

        public void LoadConversation()
        {
            if (LoadFile.IsValidDialogueTriggerPath(DialogueFolder))
            {
                LoadFile.LoadSaveables();
            }
            else
            {
                BranchLog.Error("This is not a valid JSON Folder");
            }
        }

#if UNITY_EDITOR
        public void ChooseFolder()
        {
            DialogueFolder = EditorUtility
                .OpenFolderPanel("Choose a folder containing Dialogue JSON files only",
                 BranchConstants.DefaultDialogueFolder,
                 "Choose a folder containing Dialogue JSON files only");
        }
#endif
    }
}
