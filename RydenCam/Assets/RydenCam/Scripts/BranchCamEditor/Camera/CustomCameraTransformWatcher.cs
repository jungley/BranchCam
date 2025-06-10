using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Camera
{

    /// <summary>
    /// A component that watches for changes in the camera's transform (position and rotation)
    /// </summary>
    public class CustomCameraTransformWatcher : MonoBehaviour
    {
        public event Action<Pose> OnTransformChanged;

        private Vector3 lastPosition;
        private Quaternion lastRotation;

        public void SetPose(Vector3 position, Quaternion rotation)
        {
            lastPosition = position;
            lastRotation = rotation;
        }

        void LateUpdate()
        {
            if (transform.position != lastPosition || transform.rotation != lastRotation)
            {
                lastPosition = transform.position;
                lastRotation = transform.rotation;
                OnTransformChanged?.Invoke(new Pose(lastPosition, lastRotation));
            }
        }
    }
}
