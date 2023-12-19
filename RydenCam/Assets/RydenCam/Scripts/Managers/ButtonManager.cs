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
        private bool isLockedOut;

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
            if (isLockedOut) return;

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (scrollIndex - 1 < 0) return;

                var scrollUp = -1;
                Scroll(scrollUp);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (scrollIndex + 1 >= ButtonList.Count) return;

                var scrollDown = 1;
                Scroll(scrollDown);
            }

            if (!Input.anyKey) return;

            foreach (var key in GlobalSettings.Settings.progressionKeyInputs)
            {
                if (Input.GetKeyDown(key))
                {
                    currentHoveredImage.Select();
                }
            }
        }

        public void StartLockOut()
        {
            StartCoroutine(Co_DecisionLockout());
        }

        private IEnumerator Co_DecisionLockout()
        {
            isLockedOut = true;
            yield return new WaitForEndOfFrame();
            isLockedOut = false;
        }

        private void Scroll(int scrollValue)
        {
            currentHoveredImage.UnHover();
            scrollIndex += scrollValue;
            ButtonList[scrollIndex].Hover();

            scrollBar.value = 1 - (scrollIndex / ((float)ButtonList.Count - 1));
        }

        public void HoverOverButton(SelectableImage selectableImage)
        {
            currentHoveredImage = selectableImage;
        }
    }
}