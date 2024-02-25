using Assets.RydenCam.Scripts.BranchCamEditor.Extensions;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;
using RydenCam.DialogueGameUI;
using RydenCam.Utilities;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Assets.RydenCam.Scripts.DialogueGameUI
{
    public class InGameDialogUIView
    {
        private VisualElement _dialoguePanel;
        private VisualElement DialoguePanel
        {
            get
            {
                if (_dialoguePanel == null)
                {
                    _dialoguePanel = ButtonManager.Instance.DialogueUIDocument.rootVisualElement.Q<VisualElement>("dialogue-panel");
                }
                return _dialoguePanel;
            }
        }
        private VisualElement _decisionPanel;
        private VisualElement DecisionPanel
        {
            get
            {
                if (_decisionPanel == null)
                {
                    _decisionPanel = ButtonManager.Instance.DialogueUIDocument.rootVisualElement.Q<ScrollView>("ScrollView");

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

        private VisualElement _decisionDialoguePanel;
        private VisualElement DecisionDialoguePanel
        {
            get
            {
                if (_decisionDialoguePanel == null)
                {
                    _decisionDialoguePanel = ButtonManager.Instance.DialogueUIDocument.rootVisualElement.Q<VisualElement>("previous-dialogue-panel");
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
            DialoguePanel.visible = true;
            var textComponent = ButtonManager.Instance.DialogueUIDocument.rootVisualElement.Q<Label>("dialogue-text");
            SetText(textComponent, dialogue);
        }

        public void DisplayDecisionNode()
        {
            ClearPanels();
            EditorDecisionNode node = Controller.CurrentNode as EditorDecisionNode;
            DecisionPanel.visible = true;
            CreateButtons(node);

            if (Controller.PreviousDialogue.Count != 0 && node.ShowPreviousDialog)
            {
                DecisionDialoguePanel.visible = true;

                var textComponent = ButtonManager.Instance.DialogueUIDocument.rootVisualElement.Q<Label>("previous-dialogue-text");
                SetText(textComponent, Controller.PreviousDialogue.Peek());
            }
        }

        private void SetText(Label textLabel, string text)
        {
            textLabel.text = text;
            textLabel.style.unityFont = GlobalSettings.Settings.defaultFont;
            textLabel.style.fontSize = GlobalSettings.Settings.defaultFontSize;
        }

        private void CreateButtons(EditorDecisionNode node)
        {
            var buttonManager = ButtonManager.Instance;

            for (int i = 0; i < node.DecisionOptions.Count; i++)
            {
                buttonManager.ButtonList.Add(new ButtonHolder(node.DecisionOptions[i], i));
            }

            //Automatically hover the first option.
            buttonManager.ButtonList[0].Hover();

        }

        public void ClearPanels()
        {
            DialoguePanel.visible = false;

            DecisionDialoguePanel.visible = false;

            DecisionPanel.visible = false;
        }
    }
}
