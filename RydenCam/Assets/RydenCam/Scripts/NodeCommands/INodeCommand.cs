using Assets.RydenCam.Scripts.BranchCamCC;

namespace Assets.RydenCam.Scripts.NodeCommands
{

    //RS TODO Needs to be an abstract class, duplicate code for RemoveNode
    public interface INodeCommand
    {
        void RemoveNode(Node node);

    }
}
