using RydenCam.Editor.Ribbon.RibbonItem;
using System.Collections.Generic;

namespace RydenCam.Editor.Ribbon.RibbonItem
{
    public class RibbonDefinition
    {
        public List<IRibbonItem> Items { get; set; } = new List<IRibbonItem>();
    }
}