using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RydenCam.Common
{
    public enum NodeType
    {
        None,
        StartNode,
        DialogueNode,
        DecisionNode,
        ActionNode,
        ConditionalNode
    }

    public enum ConnectionPointType { In, Out, UserHandleOnGUI }
    public enum Side { Left, Right };
    public enum CameraGoal { Portrait, OverShoulder, FrameShare, Custom };
    public enum CameraDistance { Close, Mid, Far };
    public enum CameraAngle { EyeLevel, Low, High };
    public enum CustomCameraType {None, Global, Local };

}
