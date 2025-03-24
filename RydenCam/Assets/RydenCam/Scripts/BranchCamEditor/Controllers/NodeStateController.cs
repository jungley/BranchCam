using System.Collections.Generic;
using UnityEngine;
using RydenCam.SequenceData;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using Cinemachine;
using Assets.RydenCam.Scripts.DialogueGameUI;
using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.NodeCommands;
using Assets.RydenCam.Scripts.BranchCamEditor.Controllers;
using Assets.RydenCam.Scripts.BranchCamEditor.Camera;

namespace RydenCam.BranchCamEditor.Controllers
{
    public interface INodePlayer
    {
        void Traverse(NodeStateController controller);
    }

    public class StartNodePlayer : INodePlayer
    {
        public void Traverse(NodeStateController controller)
        {
            //Set Up Sequence
            controller.ToggleRelevantObjects(true);

            controller.SetupManager.SetPreDefinedActorPositions(controller.CurrentNode as StartNode);
            controller.SetupManager.ActorsLookAtEachOther();
            controller.SetupManager.SetDepthOfField(true);

            controller.CurrentNode = controller.CurrentNode.GetNextNode();
            controller.TraverseNodeNetwork();
        }
    }

    public class DialogueNodePlayer : INodePlayer
    {
        public void Traverse(NodeStateController controller)
        {
            HandleDialogue(controller.CurrentNode, controller);
        }

        public void HandleDialogue(Node node, NodeStateController controller)
        {

            DialogueNode dialogueNode = node as DialogueNode;
            if (dialogueNode == null) return;

            if (dialogueNode.NodeConvodata.DialogTextList.Count == 0) return;

            controller.DialogueIndex++;
            if (controller.DialogueIndex < dialogueNode.NodeConvodata.DialogTextList.Count)
            {
                string currentDialogue = dialogueNode.NodeConvodata.DialogTextList[controller.DialogueIndex];
                controller.UIView.DisplayDialogueText(currentDialogue);
                controller.PreviousDialogue.Push(currentDialogue);
            }
            controller.SetCamera();

            if (controller.DialogueIndex == dialogueNode.NodeConvodata.DialogTextList.Count -1)
            {
                controller.DialogueIndex = -1;
                controller.CurrentNode = controller.CurrentNode.GetNextNode();
            }
        }
    }

    public class DecisionNodePlayer : INodePlayer
    {
        public void Traverse(NodeStateController controller)
        {
            controller.UIView.DisplayDecisionNode();
            controller.SetCamera();
            ValidInputs.IsDecionsMakingLocked = true;
        }
    }

    public class ActionNodePlayer : INodePlayer
    {
        public void Traverse(NodeStateController controller)
        {
            new ActionNodeCommand(controller.CurrentNode).InvokeCommands();
            controller.CurrentNode = controller.CurrentNode.GetNextNode();
            controller.TraverseNodeNetwork();
        }
    }

    [ExecuteAlways]
    public class NodeStateController
    {
        private CinemachineVirtualCamera dialogueCamera;

        public Node CurrentNode { get; set; }
        public int DialogueIndex { get; set; } = -1;
        public Stack<string> PreviousDialogue { get; private set; } = new Stack<string>();

        public InGameDialogUIView UIView { get; private set; }
        public CameraCalculator CamCalculator { get; private set; }

        public bool IsDialogueRunning { get; set; } = false;

        public SequenceSetupManager SetupManager { get; set; }

        public NodeStateController(GameObject dcamera, GameObject dcameraBrain)
        {
            dialogueCamera = dcamera.GetComponent<CinemachineVirtualCamera>();
            CamCalculator = new CameraCalculator();
            SetupManager = new SequenceSetupManager(CamCalculator);
            UIView = new InGameDialogUIView(this);
        }


        public void TraverseNodeNetwork()
        {
            if(!IsDialogueRunning || ValidInputs.IsDecionsMakingLocked) return;

            while (CurrentNode != null)
            {
                INodePlayer nodePlayer = CreateNodePlayer(CurrentNode);
                nodePlayer.Traverse(this);
                return; // Exit after handling the current node
            }
            EndSequence();
        }

        public void MakeDecision(int choiceIndex)
        {
            var decisionNode = CurrentNode as DecisionNode;
            CurrentNode = decisionNode.MakeDecision(choiceIndex);
            ValidInputs.IsDecionsMakingLocked = false;
            TraverseNodeNetwork();
        }


        private INodePlayer CreateNodePlayer(Node node)
        {
            return node switch
            {
                StartNode => new StartNodePlayer(),
                DialogueNode => new DialogueNodePlayer(),
                DecisionNode => new DecisionNodePlayer(),
                ActionNode => new ActionNodePlayer(),
                _ => null
            };
        }

        public void SetCamera()
        {
            if (CurrentNode is ITalkable posNode)
            {
                Pose placement = CamCalculator.CalculatePlacement(posNode.NodeConvodata.ShotConfig, new PreviewActorPositionData(posNode));
                dialogueCamera.transform.SetPositionAndRotation(placement.position, placement.rotation);
            }
        }

        public void EndSequence()
        {
            SetupManager.ReturnActorsToOriginalPositionsIfEnabled();
            SetupManager.SetDepthOfField(enabled: false);
            UIView.ClearPanels();
            ToggleRelevantObjects(visibility: false);
            PreviousDialogue.Clear();
            IsDialogueRunning = false;
            ValidInputs.IsDecionsMakingLocked = false;
        }

        public void ToggleRelevantObjects(bool visibility)
        {
            dialogueCamera.enabled = visibility;
        }
    }
}
