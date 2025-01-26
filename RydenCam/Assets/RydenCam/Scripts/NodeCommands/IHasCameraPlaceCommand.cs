using Assets.RydenCam.Scripts.BranchCamCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.RydenCam.Scripts.NodeCommands
{
    
    public interface IHasCustomCameraCommand
    {
        public CustomCameraCommand CustomCameraCommand { get; set; }
    }
    
}
