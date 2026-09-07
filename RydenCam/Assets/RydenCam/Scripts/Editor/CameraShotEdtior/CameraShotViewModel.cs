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
        get => currentShot ??= CameraShotsManager.Instance.DefaultShot;
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
            string lastOpenedFile = FilePathSaveManager.Instance
                .GetLastFilePathSaved(FilePathSaveManager.LastOpened_CameraShotsKey);

            CameraShotConfigurationWrapper shotsWrapper = SettingsService.Load<CameraShotConfigurationWrapper>(lastOpenedFile);
            if (shotsWrapper?.Shots != null && shotsWrapper.Shots.Any())
                CameraShotsManager.Instance.CameraShots = shotsWrapper.Shots;
            CameraShotsManager.Instance.MarkInitialStateLoaded();
        }

        CurrentShot = CameraShotsManager.Instance.DefaultShot;
        PreviewRenderer = new PreviewRenderer();
    }

    public void RemoveShot(CameraShotConfiguration shot)
    {
        if(shot.IsDefault)
        {
            return;
        }

        int index = CameraShotsManager.Instance.CameraShots.FindIndex(s => s.ShotId == shot.ShotId);
        if (index < 0) return;

        CameraShotsManager.Instance.CameraShots.Remove(shot);
        CurrentShot = CameraShotsManager.Instance.CameraShots.Count > 0
            ? CameraShotsManager.Instance.CameraShots[Mathf.Max(0, index - 1)]
            : CameraShotsManager.Instance.DefaultShot;
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
        var fileresult = FilePathSaveManager.Instance.GetLastFilePathSaved(FilePathSaveManager.LastOpened_CameraShotsKey);
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
