using Assets.RydenCam.Scripts.BranchCamCC;
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

        var buttonManager = ButtonManager.Instance;
        var scrollView = buttonManager?.DialogueUIDocument?.rootVisualElement.Q<ScrollView>("ScrollView");
        var template = Resources.Load<VisualTreeAsset>("option-button");
        if (scrollView == null || template == null)
        {
            Debug.LogError("[RydenCam] Cannot create a decision option: the ScrollView or option-button UXML is missing.");
            return;
        }

        Button = template.CloneTree().Q<Button>("option-button");
        if (Button == null)
        {
            Debug.LogError("[RydenCam] option-button UXML requires a Button named 'option-button'.");
            return;
        }

        Button.text = buttonText;
        var settings = GlobalSettings.Settings;
        if (settings != null)
        {
            Button.style.unityFont = settings.defaultFont ? settings.defaultFont : Resources.Load<Font>("Afacad-Regular");
            Button.style.fontSize = settings.defaultFontSize;
        }

        if (settings == null || settings.isMouseAllowed) Button.RegisterCallback<ClickEvent>(evt => ButtonAction());
        Button.RegisterCallback<MouseOverEvent>(evt => Hover());
        Button.RegisterCallback<MouseOutEvent>(evt => Unhover());

        scrollView.Add(Button);
    }

    public void ButtonAction()
    {
        var buttonManager = ButtonManager.Instance;
        var statePlayer = buttonManager?.DialoguePlayer?.StatePlayer;
        if (statePlayer == null) return;

        var decision = statePlayer.CurrentNode as DecisionNode;
        if (decision?.PointOut == null || optionIndex < 0 || optionIndex >= decision.PointOut.Count)
        {
            Debug.LogWarning($"[RydenCam] Decision option {optionIndex} has no matching output connection.");
            return;
        }

        buttonManager.Clear();
        statePlayer.MakeDecision(optionIndex);
    }


    //Below we unhover everything specifically in case the user is using both keyboard and mouse.
    //This prevents two elements being hovered at the same time.
    public void Hover()
    {
        Unhover();

        if (Button != null)
            Button.style.backgroundColor = new Color(0, 0, 0, backgroundTransparencyValue);
    }

    public void Unhover()
    {
        foreach(var buttonHolder in ButtonManager.Instance.ButtonList)
        {
            if (buttonHolder.Button != null)
                buttonHolder.Button.style.backgroundColor = new Color(0, 0, 0, 0);
        }
    }
}
