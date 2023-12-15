using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class SelectableImage : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler
{
    private Action onClickEvent;
    private Image hoverImage;

    public void SetOnClickEvent(Action onClickEvent)
    {
        this.onClickEvent = onClickEvent;
    }

    public void SetHoverImage(Image hoverImage)
    {
        this.hoverImage = hoverImage;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        onClickEvent.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverImage.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverImage.gameObject.SetActive(false);
    }
}
