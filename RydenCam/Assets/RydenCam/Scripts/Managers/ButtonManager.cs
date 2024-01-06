using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RydenCam.Common;
using System;

namespace RydenCam.DialogueGameUI
{
    /// <summary>
    /// Handles the navigation and selection of decision options when presented.
    /// </summary>
    public class ButtonManager : MonoBehaviour
    {
        public static ButtonManager Instance;

        public delegate void OnButtonSelected();
        public OnButtonSelected OnButtonSelectedCallBack;
        [HideInInspector]public List<SelectableImage> ButtonList = new List<SelectableImage>();
        public DialoguePlayer DialoguePlayer;

        private SelectableImage currentHoveredImage;
        [SerializeField] private Scrollbar scrollBar;
        private int scrollIndex;

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

            if (scrollBar == null)
            {
                Scrollbar scriptComponent = FindObjectOfType<Scrollbar>();
            }
            #endregion

            OnButtonSelectedCallBack += Clear;
        }

        public void Clear()
        {
            scrollIndex = 0;
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

            SelectHoveredButton();
        }

        private void SelectHoveredButton()
        {
            if (InputWrapper.ProgressionKeyPressed) currentHoveredImage.Select();
        }

        private void Scroll(int scrollValue)
        {
            currentHoveredImage.UnHover();
            scrollIndex += scrollValue;
            ButtonList[scrollIndex].Hover();

            scrollBar.value = 1 - (scrollIndex / ((float)ButtonList.Count - 1));
        }

        public void HoverOverButton(SelectableImage selectableImage) => currentHoveredImage = selectableImage;
    }
}