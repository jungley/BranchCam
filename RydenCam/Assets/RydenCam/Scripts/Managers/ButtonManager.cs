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
        //public bool HasHoveredImage => currentHoveredImage != null;
        public UIDocument ScrollViewUIDocument;

        //private SelectableImage currentHoveredImage;
        private int scrollIndex;
        private ScrollView scrollView;

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

            scrollView = ScrollViewUIDocument.rootVisualElement.Q<ScrollView>("ScrollView");
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

            var isInUpperBounds = scrollIndex - 1 < 0;
            var isInLowerBounds = scrollIndex + 1 >= ButtonList.Count;

            if (InputWrapper.UpKeyPressed && !isInUpperBounds) Scroll(-1);
            if (InputWrapper.DownKeyPressed && !isInLowerBounds) Scroll(1);

            if (InputWrapper.ProgressionKeyPressed) ButtonList[scrollIndex].ButtonAction();
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