using System;
using UnityEngine;

namespace RydenCam.Common
{
    public static class BranchLog
    {
        /// <summary>
        /// Set to false to suppress informational log messages from RydenCam.
        /// </summary>
        public static bool DebugEnableLogging = true;

        /// <summary>
        /// Set to false to suppress error log messages from RydenCam.
        /// </summary>
        public static bool EnableProductErrorLogging = true;

        private const string Prefix = "[RydenCam] ";

        public static void Log(string message)
        {
            if (DebugEnableLogging)
            {
                Debug.Log(Prefix + message);
            }
        }

        public static void Error(string message, Exception ex = null)
        {
            if (EnableProductErrorLogging)
            {
                if (ex != null)
                    Debug.LogError($"{Prefix}{message}\n{ex}");
                else
                    Debug.LogError(Prefix + message);
            }
        }
    }
}
