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
        public List<CameraShotConfiguration> CameraShots { get; set; } = new List<CameraShotConfiguration>();

        private CameraShotConfiguration defaultShot;
        public CameraShotConfiguration DefaultShot
        {
            get
            {
                if (defaultShot == null)
                {
                    defaultShot = new CameraShotConfiguration("Default Shot")
                    {
                        IsDefault = true
                    };
                    CameraShots.Add(defaultShot);
                    return defaultShot;
                }
                return defaultShot;
            }
        }

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
