using Assets.RydenCam.Scripts.BranchCamEditor.Managers;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender;
using Assets.RydenCam.Scripts.Editor.CameraShotEditor;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Serialization;
using System.Linq;
using UnityEditor;
using UnityEngine;
using SettingsService = RydenCam.BranchCamEditor.Serialization.SettingsService;


public class CameraShotViewModel
{    
    private CameraShotConfiguration currentShot;
    public CameraShotConfiguration CurrentShot
    {
        get => currentShot = CameraShotsManager.Instance.CameraShots.FirstOrDefault(s => s.ShotId == currentShot?.ShotId) ?? CameraShotsManager.Instance.DefaultShot;
        set => currentShot = value;
    }
    
    public PreviewRenderer PreviewRenderer { get; set; }
    public float DistancePreviewSlider { get; set; } = 1f;

    public CameraShotViewModel()
    {
        // Preserve unsaved shots when the window is reopened or redocked. The static
        // manager is empty after a domain reload, which is when disk state should load.
        if (!CameraShotsManager.Instance.InitialStateLoaded)
        {
            CameraShotSettingsManager.Load(NodeManager.Instance.StartNode?.CameraShotFilePath ?? RydenCam.Common.BranchConstants.DefaultCameraShotFile);
        }

        CurrentShot = CameraShotsManager.Instance.DefaultShot;
        PreviewRenderer = new PreviewRenderer();
    }

    public void RemoveShot(CameraShotConfiguration shot)
    {
        var manager = CameraShotsManager.Instance;
        if (!manager.RemoveShot(shot)) return;
        if (CurrentShot == null || CurrentShot.ShotId == shot.ShotId)
            CurrentShot = manager.DefaultShot;
    }

    public void NewFile()
    {
        bool shouldReset = EditorUtility.DisplayDialog("Confirmation", "Are you sure you want to reset everything?", "Yes", "No");
        if (shouldReset)
        {
            FilePathSaveManager.Instance.ClearLastFilePath(FilePathSaveManager.LastOpened_CameraShotsKey);
            CameraShotSettingsManager.New();
        }
    }

    public void Save()
    {
        var fileresult = CameraShotsManager.Instance.CurrentFilePath;
        CameraShotSettingsManager.Save(fileresult);
    }

    public void SaveAs()
    {
        CameraShotSettingsManager.SaveAs();
    }

    public void Open()
    {
        CameraShotSettingsManager.OpenAndLoad();
        CurrentShot = CameraShotsManager.Instance.DefaultShot;
    }

}
