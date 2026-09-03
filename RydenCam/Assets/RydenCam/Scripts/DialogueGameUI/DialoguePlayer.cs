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
            if (!StatePlayer.IsInitialized)
            {
                StatePlayer = null;
                return;
            }
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
            {
                ValidInputs.OnValidInput -= StatePlayer.TraverseNodeNetwork;
                if (StatePlayer.IsDialogueRunning)
                    StatePlayer.EndSequence();
            }
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

            if (!LoadConversation())
                return;

            var startNode = NodeManager.Instance.StartNode;
            if (startNode == null)
            {
                Debug.LogError("[RydenCam] The loaded conversation has no Start node.");
                return;
            }

            StatePlayer.IsDialogueRunning = true;
            StatePlayer.CurrentNode = startNode;
            StatePlayer.TraverseNodeNetwork();
        }

        public bool LoadConversation()
        {
            if (string.IsNullOrWhiteSpace(DialogueFilePath))
            {
                Debug.LogWarning("[RydenCam] DialogueFilePath is empty. Cannot load conversation.");
                return false;
            }
            return NodeGraphSettingsManager.Load(DialogueFilePath);
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
