using RydenCam.Editor.Ribbon;
using System.Collections.Generic;

namespace RydenCam.Editor.Ribbon.RibbonItem
{
    public class RibbonGroup
    {
        public string Name { get; set; }
        public List<IRibbonItem> Items { get; } = new List<IRibbonItem>();
        public float Width { get; set; } = 100f;
        public float Height { get; set; } = 30f;
    }
}