using RydenCam.BranchCamEditor.Serialization;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.Common;
using UnityEngine;
using RydenCam.BranchCamEditor.Managers;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
                _requiredDialogueCameraBrain = value;
            }
        }


        [HideInInspector]
        public string DialogueFilePath;
        public NodeStateController StatePlayer;
        public bool IsDialogueRunning => StatePlayer != null && StatePlayer.IsDialogueRunning;

        public void Awake()
        {
            if (DialogueCamera == null)
            {
                Debug.LogError("[RydenCam] DialoguePlayer requires a DialogueCamera reference. Assign it in the Inspector.");
                return;
            }

            StatePlayer = new NodeStateController(DialogueCamera, DialogueCameraBrain);
            StatePlayer.ToggleRelevantObjects(visibility: false);
        }

        private void OnEnable()
        {
            if (StatePlayer != null)
                ValidInputs.OnValidInput += StatePlayer.TraverseNodeNetwork;
        }

        private void OnDisable()
        {
            if (StatePlayer != null)
                ValidInputs.OnValidInput -= StatePlayer.TraverseNodeNetwork;
        }

        private void Update()
        {
            ValidInputs.Update();
        }

        /// <summary>
        /// StartSequence should be triggered by a trigger collider
        /// </summary>
        public void StartSequence()
        {
            if (StatePlayer == null)
            {
                Debug.LogError("[RydenCam] DialoguePlayer not initialized. Ensure DialogueCamera is assigned.");
                return;
            }

            LoadConversation();
            StatePlayer.IsDialogueRunning = true;
            StatePlayer.CurrentNode = NodeManager.Instance.StartNode;
            StatePlayer.TraverseNodeNetwork();
        }

        public void LoadConversation()
        {
            if (string.IsNullOrEmpty(DialogueFilePath))
            {
                Debug.LogWarning("[RydenCam] DialogueFilePath is empty. Cannot load conversation.");
                return;
            }
            NodeGraphSettingsManager.Load(DialogueFilePath);
        }

#if UNITY_EDITOR
        public void ChooseFolder()
        {
            DialogueFilePath = EditorUtility
                .OpenFilePanel("Choose the conversation file",
                 BranchConstants.DefaultDialogueFolder,
                 "json");
        }
#endif
    }
}
