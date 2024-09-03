using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.RydenCam.Scripts.BranchCamCC
{
    public interface ITalkable
    {
        public ConversationData NodeConvodata { get; set; }
    }
}
