using Assets.RydenCam.Scripts.BranchCamCC;
using System.Collections.Generic;

namespace RydenCam.BranchCamEditor.Controllers
{
    public interface ISequenceController
    {
        Stack<string> PreviousDialogue { get; set; }
        Node CurrentNode { get; set; }
    }
}
