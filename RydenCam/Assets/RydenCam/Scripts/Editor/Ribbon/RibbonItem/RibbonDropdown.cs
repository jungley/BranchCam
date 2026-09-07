using RydenCam.Editor.Ribbon;
using System.Collections.Generic;


namespace RydenCam.Editor.Ribbon.RibbonItem
{
    public class RibbonDropdown : IRibbonItem
    {
        public string Label { get; set; }
        public List<RibbonButton> Options { get; } = new List<RibbonButton>();
        public float Width { get; set; }
        public float Height { get; set; }
    }
}