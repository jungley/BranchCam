using Assets.RydenCam.Scripts.BranchCamEditor.Managers;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender;
using Assets.RydenCam.Scripts.Editor.CameraShotEdtior;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Serialization;
using System.Linq;
using UnityEditor;
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
        //Clear previously set if set
        CameraShotsManager.Instance.CameraShots.Clear();


        //Get Last Saved
        string lastOpenedFile = FilePathSaveManager.Instance
            .GetLastFilePathSaved(FilePathSaveManager.LastOpened_CameraShotsKey);

        CameraShotConfigurationWrapper shotswrapper =SettingsService.Load<CameraShotConfigurationWrapper>(lastOpenedFile);
        if(shotswrapper != null && shotswrapper.Shots != null && shotswrapper.Shots.Any())
        {
            CameraShotsManager.Instance.CameraShots = shotswrapper.Shots;
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
        CurrentShot = CameraShotsManager.Instance.CameraShots[index - 1];
        CameraShotsManager.Instance.CameraShots.Remove(shot);
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
    }

}