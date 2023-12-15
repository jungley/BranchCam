using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using RydenCam.DialogueGameUI;
using System;
/// <summary>
/// Simple Button creator using the builder pattern. 
/// </summary>
public class ButtonCreator
{
    private GameObject buttonObject;
    private Image hoverImage;
    private SelectableImage selectableImage;
    private float width, height;
    private TextMeshProUGUI buttonText;
    private Image buttonImage;

    public ButtonCreator(string buttonName, float width, float height)
    {
        buttonObject = new GameObject(buttonName);
        //Because this is a button we always want to be able to click on it.
        selectableImage = buttonObject.AddComponent<SelectableImage>();

        this.width = width;
        this.height = height;
    }

    private Vector2 GetTextBoxSize
    {
        get
        {
            return new Vector2(width, buttonText.preferredHeight * 1.1f);
        }
    }

    //This acts as the "HitBox" for the object.
    public ButtonCreator AddUIImage()
    {
        buttonImage = buttonObject.AddComponent<Image>();

        buttonImage.rectTransform.sizeDelta = new Vector2(width, height);

        buttonImage.color = new Color(0, 0, 0, 0);

        return this;
    }

    public ButtonCreator AddHoverImage()
    {
        var hoverImageHolder = new GameObject("HoverImage");
        hoverImageHolder.transform.SetParent(buttonObject.transform);

        hoverImage = hoverImageHolder.AddComponent<Image>();

        hoverImage.rectTransform.sizeDelta = new Vector2(width, height);

        hoverImage.color = new Color(0, 0, 0, .8f);

        hoverImage.gameObject.SetActive(false);

        return this;
    }

    public ButtonCreator AddText(string dialogueText)
    {
        var textHolder = new GameObject("Button Text");
        buttonText = textHolder.AddComponent<TextMeshProUGUI>();
        
        textHolder.transform.SetParent(buttonObject.transform);

        buttonText.text = dialogueText;

        buttonText.fontSize = Mathf.RoundToInt(25 * Mathf.Min(Screen.width, Screen.height) / 800);

        TMP_FontAsset font = Resources.Load("Afacad-Regular SDF") as TMP_FontAsset;

        buttonText.font = font;

        buttonText.alignment = TextAlignmentOptions.Center;

        buttonText.rectTransform.sizeDelta = GetTextBoxSize - new Vector2(5, 0);

        buttonText.raycastTarget = false;

        return this;
    }

    //This method has to be called AFTER AddText method.
    public ButtonCreator ResizeElementsByTextSize()
    {
        if (buttonObject == null)
        {
            Debug.LogWarning("No Text Component Found.");
            return this;
        }

        hoverImage.rectTransform.sizeDelta = GetTextBoxSize;
        buttonObject.GetComponent<RectTransform>().sizeDelta = GetTextBoxSize;
        buttonImage.rectTransform.sizeDelta = GetTextBoxSize;

        return this;
    }

    public ButtonCreator SetParent(Transform parent)
    {
        buttonObject.transform.SetParent(parent);
        return this;
    }

    public ButtonCreator AddButtonScript(int optionIndex)
    {
        var buttonScript = buttonObject.AddComponent<ButtonScript>();
        buttonScript.AssociatedOption = optionIndex;

        selectableImage.SetOnClickEvent(buttonScript.ChooseDecisionClick);
        selectableImage.SetHoverImage(hoverImage);

        return this;
    }

    public ButtonCreator AddOnClickEvent(Action onClickEvent)
    {
        var clickableImage = buttonObject.AddComponent<SelectableImage>();

        clickableImage.SetOnClickEvent(onClickEvent);
        return this;
    }
}
