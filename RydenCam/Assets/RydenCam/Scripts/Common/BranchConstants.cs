using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RydenCam.Common
{
    public static class BranchConstants
    {
        //Dependent GameObject Names
        public static string CameraBrain = "RydenCameraBrain";
        public static string VirtualDialogueCamera = "VirtualDialogueCamera";
        public static string CustomCamera = "CustomRydenCam";

        //Prefab Paths
        public static string CamPrefabPath = "Assets/RydenCam/Prefabs/CustomRydenCam.prefab";
        //Dialog Files Path
        public static string DialogueFolder = "Assets/RydenCam/DialogueFiles/";

        //Tags
        public static string Tag_RydenConvo = "RydenConvo";

        //Editor
        public static string UnAssignedActor ="<NOT ASSIGNED>";
        public const string MainWindowName = "Window/BranchCam";
        public static string WindowTitle = "BranchCam";
        public static readonly string[] FileDropdownOptions = { "New", "Save As" };
        public static readonly string LoadFolderPanelTitle = "Choose a folder containing Dialogue files only";
        public static readonly string SaveAsTitle = "Select a folder";
    }
}
