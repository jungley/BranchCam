using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollBarEnabler : MonoBehaviour
{
    public Transform contentContainer;
    public ScrollRect scrollRect;

    private void OnEnable()
    {
        if(contentContainer.childCount >= 4)
        {
            scrollRect.vertical = true;
        }
        else
        {
            scrollRect.vertical = false;
        }
    }
}
