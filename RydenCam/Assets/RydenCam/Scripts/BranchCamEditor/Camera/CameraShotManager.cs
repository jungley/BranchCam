using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.RydenCam.Scripts.Editor.CameraShotEditor
{
    [System.Serializable]
    [ExecuteAlways]

    public class CameraShotsManager
    {
        private const string PortraitDefaultId = "ryden-default-portrait";
        private const string OverShoulderDefaultId = "ryden-default-over-shoulder";
        private const string FrameShareDefaultId = "ryden-default-frame-share";
        private const string CustomDefaultId = "ryden-default-custom";

        private List<CameraShotConfiguration> cameraShots = new List<CameraShotConfiguration>();
        public bool InitialStateLoaded { get; private set; }
        public List<CameraShotConfiguration> CameraShots
        {
            get
            {
                EnsureBuiltInShots();
                return cameraShots;
            }
            set
            {
                cameraShots = value?.Where(shot => shot != null).ToList() ?? new List<CameraShotConfiguration>();
                foreach (CameraShotConfiguration shot in cameraShots.Where(shot => string.IsNullOrWhiteSpace(shot.ShotId)))
                    shot.ShotId = Guid.NewGuid().ToString();
                foreach (CameraShotConfiguration shot in cameraShots)
                    shot.IsDefault = IsBuiltInId(shot.ShotId);
                EnsureBuiltInShots();
            }
        }

        public bool HasUserDefinedShots => CameraShots.Any(shot => !shot.IsDefault);

        public void MarkInitialStateLoaded()
        {
            InitialStateLoaded = true;
        }

        private CameraShotConfiguration defaultShot;
        public CameraShotConfiguration DefaultShot
        {
            get
            {
                EnsureBuiltInShots();
                return defaultShot;
            }
        }

        private void EnsureBuiltInShots()
        {
            cameraShots ??= new List<CameraShotConfiguration>();
            EnsureBuiltInShot("Portrait", PortraitDefaultId, CameraGoal.Portrait);
            EnsureBuiltInShot("Over Shoulder", OverShoulderDefaultId, CameraGoal.OverShoulder);
            EnsureBuiltInShot("Frame Share", FrameShareDefaultId, CameraGoal.FrameShare);
            EnsureBuiltInShot("Custom", CustomDefaultId, CameraGoal.Custom);
            defaultShot = cameraShots.First(shot => shot.ShotId == PortraitDefaultId);
        }

        private void EnsureBuiltInShot(string name, string id, CameraGoal goal)
        {
            CameraShotConfiguration shot = cameraShots.FirstOrDefault(candidate => candidate.ShotId == id);
            if (shot == null)
            {
                shot = new CameraShotConfiguration(name)
                {
                    ShotId = id,
                    GoalType = goal,
                    IsDefault = true
                };
                cameraShots.Insert(Mathf.Min((int)goal, cameraShots.Count), shot);
            }
            else
            {
                shot.ShotName = name;
                shot.GoalType = goal;
                shot.IsDefault = true;
            }
        }

        private static bool IsBuiltInId(string id)
        {
            return id == PortraitDefaultId || id == OverShoulderDefaultId ||
                   id == FrameShareDefaultId || id == CustomDefaultId;
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
