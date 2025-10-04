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

    public class CameraShotsManager
    {
        public List<CamShotConfig> CameraShots { get; set; }

        private static CameraShotsManager instance;
        public static CameraShotsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CameraShotsManager();
                }
                return instance;
            }
        }
    }
}
