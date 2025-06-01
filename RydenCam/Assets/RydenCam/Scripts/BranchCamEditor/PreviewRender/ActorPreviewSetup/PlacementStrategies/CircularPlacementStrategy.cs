using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup
{
    public class CircularPlacementStrategy : IActorPlacementStrategy
    {
        public float defaultPreviewDistance = 2.0f;

        public List<PreviewActorData> GeneratePreviewData(List<ActorInfo> actors, float distanceBetween)
        {
            float radius = distanceBetween;
            float angleStep = 360f / actors.Count;
            var result = new List<PreviewActorData>();

            for (int i = 0; i < actors.Count; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 origin = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Vector3 forward = -origin.normalized;

                result.Add(PreviewPlacementMeshGenerator.Create(actors[i], origin, Quaternion.LookRotation(forward), forward));
            }

            return result;
        }
    }
}
