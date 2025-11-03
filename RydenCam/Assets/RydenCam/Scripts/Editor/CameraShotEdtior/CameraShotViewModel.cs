using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender;
using Assets.RydenCam.Scripts.Editor.CameraShotEdtior;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using System.Collections.Generic;
using System.Linq;

public class CameraShotViewModel
{    
    private CamShotConfig currentShot;
    public CamShotConfig CurrentShot
    {
        get => currentShot ??= CameraShotsManager.Instance.DefaultShot;
        set => currentShot = value;
    }
    

    public PreviewRenderer PreviewRenderer { get; set; }
    public float DistancePreviewSlider { get; set; } = 1f;

    public void RemoveShot(CamShotConfig shot)
    {
        if(shot.IsDefault)
        {
            return;
        }

        int index = CameraShotsManager.Instance.CameraShots.FindIndex(s => s.ShotId == shot.ShotId);
        CurrentShot = CameraShotsManager.Instance.CameraShots[index - 1];
        CameraShotsManager.Instance.CameraShots.Remove(shot);
    }


    public CameraShotViewModel()
    {
        CurrentShot = CameraShotsManager.Instance.DefaultShot;
        PreviewRenderer = new PreviewRenderer();
    }
}