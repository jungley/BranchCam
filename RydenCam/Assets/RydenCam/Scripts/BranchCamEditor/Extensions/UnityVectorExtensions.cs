using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RydenCam.BranchCamEditor.Extensions
{
    public static class UnityVectorExtensions
    {
        public static Vector3 GetClosest(this Vector3 midpoint, Vector3 o1, Vector3 o2)
        {
            return Vector3.Distance(midpoint, o1) < Vector3.Distance(midpoint, o2) ? o1 : o2;
        }

        public static Vector3 GetFarthest(this Vector3 midpoint, Vector3 o1, Vector3 o2)
        {
            return Vector3.Distance(midpoint, o1) > Vector3.Distance(midpoint, o2) ? o1 : o2;
        }
    }
}
