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
    private ClickableImage clickableImage;
    public ButtonCreator(string buttonName)
    {
        buttonObject = new GameObject(buttonName);
        clickableImage = buttonObject.AddComponent<ClickableImage>();
    }

    public ButtonCreator AddUIImage(float width, float height)
    {
        var buttonImage = buttonObject.AddComponent<Image>();

        buttonImage.rectTransform.sizeDelta = new Vector2(width, height);

        buttonImage.color = Color.white;

        buttonImage.enabled = false;
        return this;
    }

    public ButtonCreator AddHoverImage(float width, float height)
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
        var textHolder = new GameObject();

        textHolder.transform.SetParent(buttonObject.transform);

        var textComponent = textHolder.AddComponent<TextMeshProUGUI>();

        textComponent.text = dialogueText;

        TMP_FontAsset font = Resources.Load("Afacad-Regular SDF") as TMP_FontAsset;

        textComponent.font = font;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontSize = 25;

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

        clickableImage.SetOnClickEvent(buttonScript.ChooseDecisionClick);
        clickableImage.SetHoverImage(hoverImage);

        return this;
    }

    public ButtonCreator AddOnClickEvent(Action onClickEvent)
    {
        var clickableImage = buttonObject.AddComponent<ClickableImage>();
        clickableImage.SetOnClickEvent(onClickEvent);
        return this;
    }
}
