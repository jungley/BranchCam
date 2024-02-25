using Cinemachine;
using RydenCam.BranchCamEditor;
using RydenCam.BranchCamEditor.BranchFile;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [SerializeField] private GameObject _requiredCanvasMain;
        public GameObject CanvasMain
        {
            get 
            { 
                if(_requiredCanvasMain == null)
                {
                    Debug.LogWarning("RequiredCanvasMain cannot be set to null.");
                }
                return _requiredCanvasMain; 
            }
            set
            {
                _requiredCanvasMain = value;
            }
        }

        [HideInInspector]
        public string DialogueFolder;
        public SequenceController SequenceControls;
        public bool IsDialogueRunning => SequenceControls.DialogueIsRunning;

        public void Start()
        {
            SequenceControls = new SequenceController(DialogueCamera, DialogueCameraBrain, CanvasMain);
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
                EditorController.Instance.ResetEverything();
                NodeManager.Instance.ConvertSaveables(LoadFile.LoadSaveables());
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
                 BranchConstants.DialogueFolder,
                 "Choose a folder containing Dialogue JSON files only");
        }
#endif
    }
}
