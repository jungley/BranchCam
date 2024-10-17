using System.Collections.Generic;
using UnityEngine;
using RydenCam.SequenceData;

using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.BranchCamEditor.Managers;
using Cinemachine;
using Assets.RydenCam.Scripts.DialogueGameUI;
using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.NodeCommands;

namespace RydenCam.BranchCamEditor.Controllers
{
    [ExecuteAlways]
    public class SequenceController : ISequenceController
    {
        //Relevant GameObjects
        [SerializeField] [HideInInspector] private CinemachineVirtualCamera dialogueCamera;
        [SerializeField] [HideInInspector] private GameObject cameraBrain;
        [SerializeField] [HideInInspector] private Queue<string> dialogQueue;

        public NodeCC CurrentNode { get; set; }
        public int DialogueIndex = -1;
        public Stack<string> PreviousDialogue { get; set; } = new Stack<string>();

        public InGameDialogUIView UIView { get; set; }
        public CameraCalculator CamCalculator { get; set; }

        public bool DialogueIsRunning = false;

        public bool DecisionBeingMadeLock = false;
        public SequenceController(GameObject dcamera, GameObject dcameraBrain)
        {
            dialogueCamera = dcamera.GetComponent<CinemachineVirtualCamera>();
            cameraBrain = dcameraBrain;
            CamCalculator = new CameraCalculator();
            UIView = new InGameDialogUIView(this);
        }

        public void SetUpSequence()
        {
            ToggleRelevantObjects(visibility: true);
            CurrentNode = NodeManager.Instance.StartNode;
            SetPreDefinedActorPositions(CurrentNode as StartNode);
            ActorsLookAtEachOther();
            SetDepthOfField(depthEnabled: true);
            PreviousDialogue = new Stack<string>();
            DialogueIsRunning = true;
            DecisionBeingMadeLock = false;
        }

        public void MakeDecision(int choiceIndex)
        {
            DecisionBeingMadeLock = false;
            if (CurrentNode is DecisionNode node)
            {
                CurrentNode = node.MakeDecision(choiceIndex);
                TraverseNodeNetwork();
            }
        }

        private void HandleDialogueText(DialogueNode dialogueNode)
        {
            if (dialogueNode.NodeConvodata.DialogTextList.Count > 0)
            {
                DialogueIndex++;
                if (DialogueIndex < dialogueNode.NodeConvodata.DialogTextList.Count)
                {
                    string currentDialogue = dialogueNode.NodeConvodata.DialogTextList[DialogueIndex];
                    UIView.DisplayDialogueText(currentDialogue);
                    PreviousDialogue.Push(currentDialogue);
                }
            }
        }

        public void TraverseNodeNetwork()
        {
            if (CurrentNode != null)
            {
                switch (CurrentNode)
                {
                    case StartNode _:
                        CurrentNode = CurrentNode.GetNextNode();
                        TraverseNodeNetwork();
                        return;
                    case DialogueNode dialogueNode:
                        HandleDialogueText(dialogueNode);
                        SetCamera();
                        if(DialogueIndex == dialogueNode.NodeConvodata.DialogTextList.Count -1)
                        {
                            DialogueIndex = -1;
                            CurrentNode = CurrentNode.GetNextNode();
                        }
                        return;
                    case DecisionNode _:
                        DecisionBeingMadeLock = true;
                        UIView.DisplayDecisionNode();
                        SetCamera();
                        return;
                    case ActionNode actionNode:
                        ActionNodeCommand command = new ActionNodeCommand(actionNode);
                        command.InvokeCommands();
                        CurrentNode = CurrentNode.GetNextNode();
                        TraverseNodeNetwork();
                        return;
                }
            }
            else
            {
                EndSequence();
            }
        }

        private void SetCamera()
        {
            if (CurrentNode is ITalkable posNode)
            {
                ConversationData convoData = posNode.NodeConvodata;
                Pose placement = CamCalculator.CalculatePlacement(convoData.ShotConfig);
                dialogueCamera.transform.SetPositionAndRotation(placement.position, placement.rotation);
            }
        }

        public void EndSequence()
        {
            ReturnActorsToOriginalPositionsIfEnabled();
            SetDepthOfField(depthEnabled: false);
            UIView.ClearPanels();
            ToggleRelevantObjects(visibility: false);
            PreviousDialogue = new Stack<string>();
            DialogueIsRunning = false;
            DecisionBeingMadeLock = false;
        }

        public void ToggleRelevantObjects(bool visibility)
        {
            dialogueCamera.enabled = visibility;
        }

        //TODO move this as part of Calculator or CameraUtility somewhere else
        public void SetPreDefinedActorPositions(StartNode startNode)
        {
            if (startNode.StartPositionsEnabled)
            {
                foreach (ActorInfo actorInfo in NodeManager.Instance.ActorsInScene())
                {
                    if (startNode.ReturnToOriginalPositions)
                    {
                        actorInfo.OriginalPositionAtStartOfDialogue = new Pose(actorInfo.ActorGO.transform.root.position, actorInfo.ActorGO.transform.root.rotation);
                    }
                    actorInfo.ActorGO.transform.root.position = actorInfo.PreDefinedStartPosition.position;

                    if(!startNode.OverrideRotation)
                    {
                        actorInfo.ActorGO.transform.root.rotation = actorInfo.PreDefinedStartPosition.rotation;
                    }
                }
            }
        }

        public void ActorsLookAtEachOther(bool rotationSet = false)
        {
            if (!rotationSet)
            {
                Vector3 midPoint = CamCalculator.CalculateMidPoint();

                foreach (ActorInfo actorInfo in NodeManager.Instance.ActorsInScene())
                {
                    actorInfo.ActorGO.transform.root.LookAt(new Vector3(midPoint.x, actorInfo.ActorGO.transform.root.position.y, midPoint.z));
                }
            }
        }

        public void ReturnActorsToOriginalPositionsIfEnabled()
        {
            if  (NodeManager.Instance.StartNode.ReturnToOriginalPositions)
            {
                foreach (ActorInfo actorInfo in NodeManager.Instance.ActorsInScene())
                {
                    actorInfo.ActorGO.transform.root.position = actorInfo.OriginalPositionAtStartOfDialogue.position;
                }

                Vector3 midPoint = CamCalculator.CalculateMidPoint();

                foreach (ActorInfo actorInfo in NodeManager.Instance.ActorsInScene())
                {
                    actorInfo.ActorGO.transform.root.LookAt(new Vector3(midPoint.x, actorInfo.ActorGO.transform.root.position.y, midPoint.z));
                }
            }
        }

        //TODO: This is for automatically setting the depth of field 
        private void SetDepthOfField(bool depthEnabled)
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


