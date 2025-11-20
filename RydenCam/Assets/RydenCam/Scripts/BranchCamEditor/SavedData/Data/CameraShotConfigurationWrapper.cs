using RydenCam.BranchCamEditor.BranchCam;
using System;
using System.Collections.Generic;


namespace RydenCam.BranchCamEditor.Serialization
{
    [Serializable]
    public class CameraShotConfigurationWrapper
    {
        // JsonUtility serializes fields only
        public List<CameraShotConfiguration> Shots;
    }
}
