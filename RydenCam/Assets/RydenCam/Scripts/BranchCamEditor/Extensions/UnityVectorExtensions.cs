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

        public static bool IsEqual(this Vector3? v1, Vector3? v2)
        {
            //both are null
            if (!v1.HasValue && !v2.HasValue)
                return true;

            //one is null, one is not
            if (!v1.HasValue || !v2.HasValue)
                return false;

            return v1.Value.x.Equals(v2.Value.x) && v1.Value.y.Equals(v2.Value.y) && v1.Value.z.Equals(v2.Value.z);

        }

        public static bool IsEqual(this Vector3 v1, Vector3 v2)
        {
            return v1.x.Equals(v2.x) && v1.y.Equals(v2.y) && v1.z.Equals(v2.z);
        }
    }
}
