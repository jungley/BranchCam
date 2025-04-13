using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Extensions
{
    public static class TransformExtensions
    {
        public static Transform FindMostParent(this Transform transform)
        {
            while (transform.parent != null)
            {
                transform = transform.parent;
            }
            return transform; 
        }
    }
}
