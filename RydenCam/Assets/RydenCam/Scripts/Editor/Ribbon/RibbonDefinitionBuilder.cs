using RydenCam.Editor.Ribbon.RibbonItem;
using System;

namespace RydenCam.Editor.Ribbon
{
    public class RibbonDefinitionBuilder
    {
        private readonly RibbonDefinition definition;

        private const float buttonWidth = 80f;
        private const float buttonHeight = 25f;
        private const float dropdownOptionHeight = 20f;

        public RibbonDefinitionBuilder()
        {
            definition = new RibbonDefinition();
        }

        public RibbonDefinitionBuilder AddButton(string label, Action action, float width = buttonWidth, float height = buttonHeight)
        {
            definition.Items.Add(new RibbonButton { Label = label, Action = action, Width = width, Height = height});
            return this;
        }

        public RibbonDefinitionBuilder AddDropdown(string label, Action toggleAction = null, float width = buttonWidth, float height = buttonHeight)
        {
            var dropdown = new RibbonDropdown { Label = label, Width = width, Height = height };
            definition.Items.Add(dropdown);
            return this;
        }

        public RibbonDefinitionBuilder AddDropdownOption(string dropdownLabel, string optionLabel, Action action, float width = buttonWidth, float height = dropdownOptionHeight)
        {
            var dropdown = FindDropdown(dropdownLabel);
            if (dropdown == null)
            {
                dropdown = new RibbonDropdown { Label = dropdownLabel, Width = width, Height = height};
                definition.Items.Add(dropdown);
            }
            dropdown.Options.Add(new RibbonButton { Label = optionLabel, Action = action, Width = width, Height = height });
            return this;
        }

        private RibbonDropdown FindDropdown(string label)
        {
            foreach (var item in definition.Items)
            {
                if (item is RibbonDropdown dropdown && dropdown.Label == label)
                {
                    return dropdown;
                }
            }
            return null;
        }

        public RibbonDefinition Build() => definition;
    }

 }