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
        scrollRect.vertical = (contentContainer.childCount >= 4) ? true : false;
    }
}
