using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;
using RydenCam.DialogueGameUI;
using RydenCam.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.RydenCam.Scripts.DialogueGameUI
{
    public class InGameDialogUIView
    {
        private GameObject _dialoguePanel;
        private GameObject DialoguePanel
        {
            get
            {
                if (_dialoguePanel == null)
                {
                    _dialoguePanel = Controller.CanvasMain.transform
                        .Find(BranchConstants.DialoguePanel).gameObject;
                }
                return _dialoguePanel;
            }
        }
        private GameObject _decisionPanel;
        private GameObject DecisionPanel
        {
            get
            {
                if (_decisionPanel == null)
                {
                    _decisionPanel = Controller.CanvasMain.transform
                        .Find(BranchConstants.DecicionPanel).gameObject;
                    
                }
                return _decisionPanel;
            }
        }

        private GameObject _decisionViewContainer;
        private GameObject DecisionViewContainer
        {
            get
            {
                if(_decisionViewContainer == null)
                {
                    _decisionViewContainer = Controller.CanvasMain.transform
                        .FindDeepChild(BranchConstants.DecisionViewContainer)
                        .gameObject;
                }
                return _decisionViewContainer;
            }
        }

        private GameObject _decisionDialoguePanel;
        private GameObject DecisionDialoguePanel
        {
            get
            {
                if (_decisionDialoguePanel == null)
                {
                    _decisionDialoguePanel = Controller.CanvasMain.transform
                        .Find(BranchConstants.DecisionDialoguePanel).gameObject;
                }
                return _decisionDialoguePanel;
            }
        }

        public ISequenceController Controller;

        public GameObject DecOptionButton
        {
            get
            {
                GameObject prefab = Resources.Load<GameObject>(BranchConstants.ButtonPrefabPath);
                if(prefab == null)
                {
                    Debug.LogError("Prefab not found at path: " + BranchConstants.ButtonPrefabPath);
                    return null;
                }
                return prefab;
            } 
        }

        public InGameDialogUIView(ISequenceController controller)
        {
            Controller = controller;
        }

        public void DisplayDialogueText(string dialogue)
        {
            ClearPanels();
            DialoguePanel.SetActive(true);
            var textComponent = DialoguePanel.GetComponentInChildren<TextMeshProUGUI>();
            SetText(textComponent, dialogue);
        }

        public void DisplayDecisionNode()
        {
            ClearPanels();
            EditorDecisionNode node = Controller.CurrentNode as EditorDecisionNode;
            CreateButtons(node);


            //Make sure this is called AFTER CreateButtons();
            DecisionPanel.SetActive(true);


            if (Controller.PreviousDialogue.Count != 0 && node.ShowPreviousDialog)
            {
                DecisionDialoguePanel.SetActive(true);
                var textComponent = DecisionDialoguePanel.GetComponentInChildren<TextMeshProUGUI>();
                SetText(textComponent, Controller.PreviousDialogue.Peek());
            }
        }

        private void SetText(TextMeshProUGUI textComponent, string text)
        {
            textComponent.text = text;
            textComponent.font = GlobalSettings.Settings.defaultFont;
            textComponent.fontSize = GlobalSettings.Settings.defaultFontSize;
        }

        private void CreateButtons(EditorDecisionNode node)
        {
            var buttonManager = ButtonManager.Instance;

            for (int i = 0; i < node.DecisionOptions.Count; i++)
            {
                var decisionButton = new ButtonCreator("Button_" + i, DecisionViewContainer.transform, node.DecisionOptions[i], i);

                buttonManager.ButtonList.Add(decisionButton.selectableImage);
            }

            //Automatically hover the first option.
            buttonManager.ButtonList[0].Hover();

        }

        public void ClearPanels()
        {
            //Clear Dialogue
            DialoguePanel.GetComponentInChildren<TextMeshProUGUI>().text = string.Empty;
            DialoguePanel.SetActive(false);

            //Clear Decision Text
            DecisionDialoguePanel.GetComponentInChildren<TextMeshProUGUI>().text = string.Empty;
            DecisionDialoguePanel.SetActive(false);

            //Remove Buttons
            foreach (Transform child in DecisionViewContainer.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            
            DecisionPanel.SetActive(false);
        }
    }
}
