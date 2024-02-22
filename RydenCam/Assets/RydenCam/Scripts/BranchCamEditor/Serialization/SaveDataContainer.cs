using Newtonsoft.Json;
using RydenCam.BranchCamEditor.Controllers;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.Common;
using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static RydenCam.BranchCamEditor.Nodes.EditorActionNode;

namespace RydenCam.BranchCamEditor.Serialization
{
    [System.Serializable]
    public class SaveDataContainer
    {
        [SerializeField]
        public List<string> JsonList = new List<string>();

        public SaveDataContainer(List<string> jsonList)
        {
            JsonList = jsonList;
        }
    }
}
