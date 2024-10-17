using Assets.RydenCam.Scripts.BranchCamCC;

namespace Assets.RydenCam.Scripts.NodeCommands
{

    //RSTODO Needs to be an abstract class, duplicate code for RemoveNode
    public interface INodeCommand
    {
        public void RemoveNode(Node node);

    }
}
