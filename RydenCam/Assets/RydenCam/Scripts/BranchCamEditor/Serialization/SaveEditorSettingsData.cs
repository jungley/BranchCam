using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.RydenCam.Scripts.Editor.CameraShotEdtior
{
    [System.Serializable]
    [ExecuteAlways]
    public class SaveEditorSettingsData
    {
        //Serializable Settings
        public bool IsCornerPreviewEnabled;
        public bool IsNodePreviewEnabled;

        public List<CamShotConfig> SavedUserConfig;

        public SaveEditorSettingsData()
        {
            SavedUserConfig = new List<CamShotConfig>();
        }
    }
}
