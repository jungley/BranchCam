using RydenCam.BranchCamEditor.Serialization;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.Common;
using UnityEditor;
using UnityEngine;
using RydenCam.BranchCamEditor.Managers;

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
        public NodeStateController SatePlayer;
        public bool IsDialogueRunning => SatePlayer.IsDialogueRunning;

        public void Awake()
        {
            SatePlayer = new NodeStateController(DialogueCamera, DialogueCameraBrain);
            SatePlayer.ToggleRelevantObjects(visibility: false);
        }

        private void OnEnable()
        {
            ValidInputs.OnValidInput += SatePlayer.TraverseNodeNetwork;
        }

        private void OnDisable()
        {
            ValidInputs.OnValidInput -= SatePlayer.TraverseNodeNetwork;
        }

        private void Update()
        {
            // Call the Update method of ValidInputs to check for changes
            ValidInputs.Update();
        }

        /// <summary>
        /// StartSequence should be triggered by a trigger collider
        /// </summary>
        public void StartSequence()
        {
            LoadConversation();
            SatePlayer.IsDialogueRunning = true;
            SatePlayer.CurrentNode = NodeManager.Instance.StartNode;
            SatePlayer.TraverseNodeNetwork();
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
