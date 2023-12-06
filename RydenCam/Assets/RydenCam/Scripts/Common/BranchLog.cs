using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RydenCam.Common
{

    public static class BranchLog
    {
       static bool DebugEnableLogging = true;

       static bool EnableProductErrorLogging = true;

        public static void Log(string message)
        {
            if (DebugEnableLogging)
            {
                Debug.Log(message);
            }
        }

        public static void Error(string message, Exception ex = null)
        {
            if (EnableProductErrorLogging)
            {
                Debug.LogError(message);
            }
        }

    }
}
