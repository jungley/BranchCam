using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RydenCam.Common;
using System;
using UnityEngine.UIElements;
using System.Xml;
using System.Linq;

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
            }

            if(DialoguePlayer == null)
            {
                DialoguePlayer scriptComponent = FindObjectOfType<DialoguePlayer>();
            }

            scrollView = DialogueUIDocument.rootVisualElement.Q<ScrollView>("ScrollView");
            #endregion
        }


        public void Clear()
        {
            scrollIndex = 0;

            foreach(var buttonHolder in ButtonList)
            {
                scrollView.Remove(buttonHolder.Button);
            }

            ButtonList.Clear();
        }

        private void Update()
        {
            if (ButtonList.Count <= 0) return;
            if (!GlobalSettings.Settings.isKeyboardAllowed) return;
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