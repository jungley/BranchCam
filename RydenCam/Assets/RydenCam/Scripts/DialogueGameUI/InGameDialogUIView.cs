using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.DialogueGameUI;
using UnityEngine;
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
                    var root = ButtonManager.Instance?.DialogueUIDocument?.rootVisualElement;
                    if (root == null) return null;
                    _dialoguePanel = root.Q<VisualElement>("dialogue-panel");
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
                    var root = ButtonManager.Instance?.DialogueUIDocument?.rootVisualElement;
                    if (root == null) return null;
                    _decisionPanel = root.Q<ScrollView>("ScrollView");
                }
                return _decisionPanel;
            }
        }

        private VisualElement _decisionDialoguePanel;
        private VisualElement DecisionDialoguePanel
        {
            get
            {
                if (_decisionDialoguePanel == null)
                {
                    var root = ButtonManager.Instance?.DialogueUIDocument?.rootVisualElement;
                    if (root == null) return null;
                    _decisionDialoguePanel = root.Q<VisualElement>("previous-dialogue-panel");
                }
                return _decisionDialoguePanel;
            }
        }

        public NodeStateController Controller;

        public InGameDialogUIView(NodeStateController controller)
        {
            Controller = controller;
        }

        public void DisplayDialogueText(string dialogue)
        {
            ClearPanels();
            if (DialoguePanel == null) return;

            DialoguePanel.visible = true;
            var root = ButtonManager.Instance?.DialogueUIDocument?.rootVisualElement;
            if (root == null) return;

            var textComponent = root.Q<Label>("dialogue-text");
            if (textComponent != null)
                SetText(textComponent, dialogue);
        }

        public void DisplayDecisionNode()
        {
            ClearPanels();
            DecisionNode node = Controller?.CurrentNode as DecisionNode;
            if (node == null || DecisionPanel == null) return;

            DecisionPanel.visible = true;
            CreateButtons(node);

            if (Controller.PreviousDialogue.Count != 0 && node.ShowPreviousDialog && DecisionDialoguePanel != null)
            {
                DecisionDialoguePanel.visible = true;

                var root = ButtonManager.Instance?.DialogueUIDocument?.rootVisualElement;
                if (root == null) return;

                var textComponent = root.Q<Label>("previous-dialogue-text");
                if (textComponent != null)
                    SetText(textComponent, Controller.PreviousDialogue.Peek());
            }
        }

        private void SetText(Label textLabel, string text)
        {
            textLabel.text = text;

            var settings = GlobalSettings.Settings;
            if (settings != null)
            {
                textLabel.style.unityFont = settings.defaultFont
                    ? settings.defaultFont
                    : Resources.Load<Font>("Afacad-Regular");
                textLabel.style.fontSize = settings.defaultFontSize;
            }
        }

        private void CreateButtons(DecisionNode node)
        {
            var buttonManager = ButtonManager.Instance;
            if (buttonManager == null) return;

            buttonManager.ButtonList.Clear();

            if (node.DecisionOptions == null || node.DecisionOptions.Count == 0) return;

            for (int i = 0; i < node.DecisionOptions.Count; i++)
            {
                buttonManager.ButtonList.Add(new ButtonHolder(node.DecisionOptions[i], i));
            }

            if (buttonManager.ButtonList.Count > 0)
            {
                buttonManager.ButtonList[0].Hover();
            }
        }

        public void ClearPanels()
        {
            if (DialoguePanel != null) DialoguePanel.visible = false;
            if (DecisionDialoguePanel != null) DecisionDialoguePanel.visible = false;
            if (DecisionPanel != null) DecisionPanel.visible = false;
        }
    }
}
