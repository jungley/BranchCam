using Assets.RydenCam.Scripts.BranchCamEditor.Managers;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender;
using Assets.RydenCam.Scripts.Editor.CameraShotEdtior;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.BranchCamEditor.Serialization;
using System.Linq;

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
    }

    public void Save()
    {
    }

    public void SaveAs()
    {
    }

    public void Open()
    {
    }

    public CameraShotViewModel()
    {
        //Get Last Saved
        string lastOpenedFile = FilePathSaveManager.Instance
            .GetLastFilePathSaved(FilePathSaveManager.LastOpened_CameraShotsKey);

        CameraShotConfigurationWrapper shotswrapper = SettingsService.Load<CameraShotConfigurationWrapper>(lastOpenedFile);
        if(shotswrapper != null && shotswrapper.Shots != null && shotswrapper.Shots.Any())
        {
            CameraShotsManager.Instance.CameraShots = shotswrapper.Shots;
        }

        CurrentShot = CameraShotsManager.Instance.DefaultShot;
        PreviewRenderer = new PreviewRenderer();
    }
}