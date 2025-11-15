using RydenCam.Editor.Ribbon;
using System;

namespace RydenCam.Editor.Ribbon.RibbonItem
{
    public class RibbonButton : IRibbonItem
    {
        public string Label { get; set; }
        public Action Action { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }
}