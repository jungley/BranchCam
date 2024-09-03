using Assets.RydenCam.Scripts.BranchCamCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.RydenCam.Scripts.NodeCommands
{

    //RSTODO Needs to be an abstract class, duplicate code for RemoveNode
    public interface INodeCommand
    {
        public void RemoveNode(NodeCC node);

    }
}
