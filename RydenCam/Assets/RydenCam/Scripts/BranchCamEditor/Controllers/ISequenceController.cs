using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RydenCam.BranchCamEditor.Controllers
{
    public interface ISequenceController
    {
        Stack<string> PreviousDialogue { get; set; }
        NodeCC CurrentNode { get; set; }
    }
}
