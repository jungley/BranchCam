using RydenCam.DialogueGameUI;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonHolder
{
    public Button Button;

    private int optionIndex;
    private float backgroundTransparencyValue = 0.75f;

    public ButtonHolder(string buttonText, int optionIndex)
    {
        this.optionIndex = optionIndex;

        var container = ButtonManager.Instance.DialogueUIDocument.rootVisualElement.Q<VisualElement>("unity-content-container");

        Button = (Resources.Load("option-button") as VisualTreeAsset).CloneTree().Q<Button>();

        Button.text = buttonText;
        Button.style.unityFont = GlobalSettings.Settings.defaultFont ? GlobalSettings.Settings.defaultFont : Resources.Load<Font>("Afacad-Regular");
        Button.style.fontSize = GlobalSettings.Settings.defaultFontSize;

        if(GlobalSettings.Settings.isMouseAllowed) Button.RegisterCallback<ClickEvent>(evt => ButtonAction());
        Button.RegisterCallback<MouseOverEvent>(evt => Hover());
        Button.RegisterCallback<MouseOutEvent>(evt => Unhover());

        container.Add(Button);
    }

    public void ButtonAction()
    {
        ButtonManager.Instance.DialoguePlayer.StatePlayer.MakeDecision(optionIndex);
        ButtonManager.Instance.Clear();
    }


    //Below we unhover everything specifically in case the user is using both keyboard and mouse.
    //This prevents two elements being hovered at the same time.
    public void Hover()
    {
        Unhover();

        Button.style.backgroundColor = new Color(0, 0, 0, backgroundTransparencyValue);
    }

    public void Unhover()
    {
        foreach(var buttonHolder in ButtonManager.Instance.ButtonList)
        {
            buttonHolder.Button.style.backgroundColor = new Color(0, 0, 0, 0);
        }
    }
}
