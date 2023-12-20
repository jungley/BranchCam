using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class SelectableImage : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler
{
    private Action onSelectEvent;
    private Image hoverImage;

    private Action onHoverEvent;
    private Action onUnhoverEvent;

    public void Initialize(Image hoverImage, Action onClickEvent, Action onHoverEvent, Action onUnhoverEvent)
    {
        this.hoverImage = hoverImage;
        this.onSelectEvent = onClickEvent;
        this.onHoverEvent = onHoverEvent;
        this.onUnhoverEvent = onUnhoverEvent;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(Input.GetMouseButtonDown(0)) Select();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UnHover();
    }

    public void Select()
    {
        onSelectEvent?.Invoke();
    }

    public void Hover()
    {
        onHoverEvent?.Invoke();
        hoverImage.gameObject.SetActive(true);
    }

    public void UnHover()
    {
        onUnhoverEvent?.Invoke();
        hoverImage.gameObject.SetActive(false);
    }
}
