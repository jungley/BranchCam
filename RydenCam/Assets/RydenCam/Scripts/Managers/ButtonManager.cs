using System.Collections.Generic;
using UnityEngine;
using RydenCam.Common;
using UnityEngine.UIElements;

namespace RydenCam.DialogueGameUI
{
    /// <summary>
    /// Handles the navigation and selection of decision options when presented.
    /// </summary>
    public class ButtonManager : MonoBehaviour
    {
        public static ButtonManager Instance;

        [HideInInspector]
        public List<ButtonHolder> ButtonList = new List<ButtonHolder>();
        public DialoguePlayer DialoguePlayer;
        public UIDocument DialogueUIDocument;

        private int scrollIndex;
        private ScrollView scrollView;
        private bool isInUpperBounds => scrollIndex - 1 < 0;
        private bool isInLowerBounds => scrollIndex + 1 >= ButtonList.Count;

        private void Awake()
        {
            #region Variable Initialization
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                BranchLog.Error("Multiple ButtonManagers found.");
                enabled = false;
                return;
            }

            if(DialoguePlayer == null)
            {
                DialoguePlayer = FindObjectOfType<DialoguePlayer>();
            }

            if (DialogueUIDocument == null)
            {
                BranchLog.Error("ButtonManager requires a DialogueUIDocument reference.");
                enabled = false;
                return;
            }

            scrollView = DialogueUIDocument.rootVisualElement.Q<ScrollView>("ScrollView");
            if (scrollView == null)
            {
                BranchLog.Error("DialogueUIDocument requires a ScrollView named 'ScrollView'.");
                enabled = false;
            }
            #endregion
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }


        public void Clear()
        {
            scrollIndex = 0;

            foreach(var buttonHolder in ButtonList)
            {
                buttonHolder.Button?.RemoveFromHierarchy();
            }

            ButtonList.Clear();
        }

        private void Update()
        {
            if (ButtonList.Count <= 0) return;
            var settings = GlobalSettings.Settings;
            if (settings == null || !settings.isKeyboardAllowed) return;
            if (!Input.anyKey) return;

            if (ValidInputs.UpKeyPressed && !isInUpperBounds) Scroll(-1);
            if (ValidInputs.DownKeyPressed && !isInLowerBounds) Scroll(1);

            if (ValidInputs.ProgressionKeyPressed) ButtonList[scrollIndex].ButtonAction();
        }

        private void Scroll(int scrollValue)
        {
            ButtonList[scrollIndex].Unhover();

            scrollIndex += scrollValue;

            scrollView.ScrollTo(ButtonList[scrollIndex].Button);
            ButtonList[scrollIndex].Hover();
        }
    }
}
