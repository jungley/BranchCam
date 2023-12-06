using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using RydenCam.Utilities;

namespace RydenCam.DialogueGameUI
{
    public class ButtonScript : MonoBehaviour
    {
        public int AssociatedOption = 0;
        //In order to prevent "click through" issue with multiple decision nodes
        int lastFrameCount;

        public void ChooseDecisionClick()
        {
            if (lastFrameCount != Time.frameCount)
            {
                DialoguePlayer scriptComponent = FindObjectOfType<DialoguePlayer>();
                scriptComponent.SequenceControls.MakeDecision(AssociatedOption);
                lastFrameCount = Time.frameCount;
            }
        }
    }
}
