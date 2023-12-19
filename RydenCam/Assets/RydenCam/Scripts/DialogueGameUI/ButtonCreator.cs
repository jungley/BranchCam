using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using RydenCam.DialogueGameUI;
using System;
/// <summary>
/// Creates the Visual button for the user to interact with.
/// </summary>
public class ButtonCreator
{
    public SelectableImage selectableImage;

    public ButtonCreator(string buttonName, Transform parent, string dialogueText, int optionIndex)
    {
        var widthRatio = 0.35f;
        var buttonWidth = Screen.width * widthRatio;

        var heightRatio = 0.02f;
        var heightPadding = Screen.height * heightRatio;

        var buttonObject = new GameObject(buttonName);
        var buttonImage = AddImage(buttonObject);

        selectableImage = buttonObject.AddComponent<SelectableImage>();

        var hoverImage = AddHoverImage(buttonObject);
        var textComponent = AddText(dialogueText);

        AddButtonScript(optionIndex, hoverImage);

        buttonObject.transform.SetParent(parent);
        textComponent.transform.SetParent(buttonObject.transform);
        hoverImage.transform.SetParent(buttonObject.transform);

        var UISize = new Vector2(buttonWidth, textComponent.preferredHeight + heightPadding);

        ResizeElementsByTextSize(hoverImage, buttonObject, buttonImage, textComponent, UISize);
    }


    //This acts as the parent for other objects and is the "HitBox".
    public Image AddImage(GameObject parentObject)
    {
        var buttonImage = parentObject.gameObject.AddComponent<Image>();

        buttonImage.color = new Color(0, 0, 0, 0);

        return buttonImage;
    }


    public Image AddHoverImage(GameObject parentObject)
    {
        var hoverImageHolder = new GameObject("HoverImage");

        hoverImageHolder.transform.SetParent(parentObject.transform);

        var hoverImage = hoverImageHolder.AddComponent<Image>();

        hoverImage.color = new Color(0, 0, 0, .8f);
        hoverImage.gameObject.SetActive(false);

        return hoverImage;
    }

    public TextMeshProUGUI AddText(string dialogueText)
    {
        var textHolder = new GameObject("Button Text");
        var buttonText = textHolder.AddComponent<TextMeshProUGUI>();
        
        buttonText.text = dialogueText;

        var fontSize = GlobalSettings.Settings.defaultFontSize;

        buttonText.fontSize = Mathf.RoundToInt(fontSize * Mathf.Min(Screen.width, Screen.height) / 1300);
        buttonText.font = GlobalSettings.Settings.defaultFont;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.raycastTarget = false;

        return buttonText;
    }

    public void ResizeElementsByTextSize(Image hoverImage, GameObject parentObject, Image buttonImage, TextMeshProUGUI textComponent, Vector2 UISize)
    {
        hoverImage.rectTransform.sizeDelta = UISize;
        parentObject.GetComponent<RectTransform>().sizeDelta = UISize;
        buttonImage.rectTransform.sizeDelta = UISize;
        textComponent.rectTransform.sizeDelta = UISize - new Vector2(5, 0);
    }

    public void AddButtonScript(int optionIndex, Image hoverImage)
    {
        var buttonManager = ButtonManager.Instance;

        Action onClick = () =>
        {
            buttonManager.DialoguePlayer.SequenceControls.MakeDecision(optionIndex);
            buttonManager.OnButtonSelectedCallBack.Invoke();
        };

        Action onHover = () => { buttonManager.HoverOverButton(selectableImage);};
        Action unHover = () => { }; // TODO : Make the current keyboard hovered unhover when using mouse.

        selectableImage.Initialize(hoverImage, onClick, onHover, unHover);


    }
}
