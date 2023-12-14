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
            DialoguePanel.GetComponentInChildren<TextMeshProUGUI>().text = dialogue;
        }

        public void DisplayDecisionNode()
        {
            ClearPanels();

            EditorDecisionNode node = Controller.CurrentNode as EditorDecisionNode; 

            for (int i = 0; i < node.DecisionOptions.Count; i++)
            {
                var widthRatio = 0.35f;
                var heightRatio = 0.0725f;

                var buttonWidth = Screen.width * widthRatio;
                var buttonHeight = Screen.height * heightRatio;

                new ButtonCreator("Button_" + i)
                    .AddUIImage(buttonWidth, buttonHeight)
                    .AddHoverImage(buttonWidth, buttonHeight)
                    .AddText(node.DecisionOptions[i])
                    .SetParent(DecisionViewContainer.transform)
                    .AddButtonScript(i);
            }

            //Keep DecisionPanel.SetActive(true) after nodes are created.
            DecisionPanel.SetActive(true);

            if (Controller.PreviousDialogue.Count != 0 && node.ShowPreviousDialog)
            {
                DecisionDialoguePanel.SetActive(true);
                DecisionDialoguePanel.GetComponentInChildren<TextMeshProUGUI>().text = Controller.PreviousDialogue.Peek();
            }

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
