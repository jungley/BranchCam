using System.Collections.Generic;
using UnityEngine;
using RydenCam.SequenceData;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Managers;
using Cinemachine;
using Assets.RydenCam.Scripts.DialogueGameUI;
using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.NodeCommands;

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

            controller.SetPreDefinedActorPositions(controller.CurrentNode as StartNode);
            controller.ActorsLookAtEachOther();
            controller.SetDepthOfField(true);

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

        public NodeStateController(GameObject dcamera, GameObject dcameraBrain)
        {
            dialogueCamera = dcamera.GetComponent<CinemachineVirtualCamera>();
            CamCalculator = new CameraCalculator();
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
                Pose placement = CamCalculator.CalculatePlacement(posNode.NodeConvodata.ShotConfig);
                dialogueCamera.transform.SetPositionAndRotation(placement.position, placement.rotation);
            }
        }

        public void EndSequence()
        {
            ReturnActorsToOriginalPositionsIfEnabled();
            SetDepthOfField(false);
            UIView.ClearPanels();
            ToggleRelevantObjects(false);
            PreviousDialogue.Clear();
            IsDialogueRunning = false;
            ValidInputs.IsDecionsMakingLocked = false;
        }

        public void ToggleRelevantObjects(bool visibility)
        {
            dialogueCamera.enabled = visibility;
        }

        public void SetPreDefinedActorPositions(StartNode startNode)
        {
            if (!startNode.StartPositionsEnabled) return;

            foreach (ActorInfo actorInfo in NodeManager.Instance.ActorsInScene())
            {
                if (startNode.ReturnToOriginalPositions)
                {
                    actorInfo.OriginalPositionAtStartOfDialogue = new Pose(actorInfo.ActorGO.transform.root.position, actorInfo.ActorGO.transform.root.rotation);
                }
                actorInfo.ActorGO.transform.root.position = actorInfo.PreDefinedStartPosition.position;

                if (!startNode.OverrideRotation)
                {
                    actorInfo.ActorGO.transform.root.rotation = actorInfo.PreDefinedStartPosition.rotation;
                }
            }
        }

        public void ActorsLookAtEachOther()
        {
            Vector3 midPoint = CamCalculator.CalculateMidPoint();
            foreach (ActorInfo actorInfo in NodeManager.Instance.ActorsInScene())
            {
                actorInfo.ActorGO.transform.root.LookAt(new Vector3(midPoint.x, actorInfo.ActorGO.transform.root.position.y, midPoint.z));
            }
        }

        public void ReturnActorsToOriginalPositionsIfEnabled()
        {
            if (NodeManager.Instance.StartNode == null || !NodeManager.Instance.StartNode.ReturnToOriginalPositions) return;

            foreach (ActorInfo actorInfo in NodeManager.Instance.ActorsInScene())
            {
                actorInfo.ActorGO.transform.root.position = actorInfo.OriginalPositionAtStartOfDialogue.position;
            }
            ActorsLookAtEachOther();
        }

        //RS TODO Automatically setting the depth of field
        public void SetDepthOfField(bool enabled)
        {
            /*
            PostProcessVolume volume = cameraBrain.GetComponent<PostProcessVolume>();

            if (volume.profile.TryGetSettings(out DepthOfField depth))
            {
             depth.enabled.value = depthEnabled;
                if (depthEnabled)
                {
                    depth.focusDistance.value = 50.0f; // Calculate based on distance
                }
            }
            */
        }
    }
}
