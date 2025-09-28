using Assets.RydenCam.Scripts.Editor.CameraShotEdtior;
using RydenCam.BranchCamEditor.BranchCam;
using System.Collections.Generic;
using System.Linq;

public class CameraShotViewModel
{
    private string defaultShotId { get; set; }

    public CamShotConfig CurrentShot { get; set; }

    public void RemoveShot(CamShotConfig shot)
    {
        if(shot.ShotId == defaultShotId)
        {
            return;
        }

        CameraShotsManager.Instance.CameraShots.Remove(shot);
    }


    public CameraShotViewModel()
    {
        //If Settings has any saved shots, load them
        //Else


        CameraShotsManager.Instance.CameraShots = new List<CamShotConfig>();
        if (CameraShotsManager.Instance.CameraShots.Count == 0)
        {
            CamShotConfig defaultShot = new CamShotConfig(shotName: "Default");
            defaultShotId = defaultShot.ShotId; 
            CameraShotsManager.Instance.CameraShots.Add(defaultShot);

            CameraShotsManager.Instance.CameraShots.Add(new CamShotConfig(shotName: "Shot 1"));
            CameraShotsManager.Instance.CameraShots.Add(new CamShotConfig(shotName: "Shot 2"));
            CameraShotsManager.Instance.CameraShots.Add(new CamShotConfig(shotName: "Shot 3"));
        }

            CurrentShot = CameraShotsManager.Instance.CameraShots
            .Where(x => x.ShotId == defaultShotId)
            .FirstOrDefault();

    }
}