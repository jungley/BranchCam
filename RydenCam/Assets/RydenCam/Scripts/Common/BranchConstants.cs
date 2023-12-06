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

        //In Game Dialog UI
        public static string RydenCanvas = "RydenCanvas";
        public static string DialoguePanel = "CT_DialoguePanel";

        public static string DecicionPanel = "CT_Container";
        //The object that decision options will be instantiated under
        public static string DecisionViewContainer = "CT_ContentDecisionContainer";

        public static string DecisionDialoguePanel = "CT_PrecDevPanel";

        //Prefab Paths
        public static string CamPrefabPath = "Assets/RydenCam/Prefabs/CustomRydenCam.prefab";//"Assets/Resources/Prefabs/CustomRydenCam.prefab";  //"Assets/RydenCam/Prefabs/CustomRydenCam";
        public static string ButtonPrefabPath = "DecOptionButton";
        //Dialog Files Path
        public static string DialogueFolder = "Assets/RydenCam/DialogueFiles/";

        //Tags
        public static string Tag_RydenConvo = "RydenConvo";

        //Editor
        public static string UnAssignedActor ="<NOT ASSIGNED>";
        public const string MainWindowName = "Window/BranchCam";
        public static string WindowTitle = "BranchCam";
    }
}
