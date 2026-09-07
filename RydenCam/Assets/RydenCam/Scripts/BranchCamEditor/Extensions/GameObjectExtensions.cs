using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Extensions
{
    public static class GameObjectExtensions
    {

        public static Pose GetPose(this GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("GameObject is null");
                return Pose.identity;
            }
            return new Pose(obj.transform.position, obj.transform.rotation);
        }

    }
}
