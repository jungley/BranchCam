using System.Collections.Generic;
using UnityEngine;
using RydenCam.SequenceData;
using RydenCam.BranchCamEditor.BranchFile;
using RydenCam.Common;
using RydenCam.BranchCamEditor.BranchCam;
using RydenCam.BranchCamEditor.Nodes;
using RydenCam.BranchCamEditor.Managers;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;
using Assets.RydenCam.Scripts.DialogueGameUI;

namespace RydenCam.BranchCamEditor.Controllers
{
    [ExecuteAlways]
    public class SequenceController : ISequenceController
    {
        //Relevant GameObjects
        [SerializeField] [HideInInspector] private CinemachineVirtualCamera dialogueCamera;
        [SerializeField] [HideInInspector] private GameObject cameraBrain;
        [SerializeField] [HideInInspector] public GameObject CanvasMain { get; set; }
        [SerializeField] [HideInInspector] private Queue<string> dialogQueue;

        public EditorBaseNode CurrentNode { get; set; }
        public int DialogueIndex = -1;
        public Stack<string> PreviousDialogue { get; set; } = new Stack<string>();

        public InGameDialogUIView UIView { get; set; }
        public CameraCalculator CamCalculator { get; set; }
        public List<ActorInfo> ActorsInScene => EditorController.Instance.ActorsInScene;

        public bool DialogueIsRunning = false;

        public bool DecisionBeingMadeLock = false;
        public SequenceController(GameObject dcamera, GameObject dcameraBrain, GameObject canvas)
        {
            dialogueCamera = dcamera.GetComponent<CinemachineVirtualCamera>();
            cameraBrain = dcameraBrain;
            CanvasMain = canvas;
            CamCalculator = new CameraCalculator(this);
            UIView = new InGameDialogUIView(this);
        }

        public void SetUpSequence()
        {
            ToggleRelevantObjects(visibility: true);
            CurrentNode = NodeManager.Instance.StartNode;
            SetPreDefinedActorPositions(CurrentNode as EditorStartNode);
            ActorsLookAtEachOther();
            CamCalculator.SetSide(NodeManager.Instance.StartNode.CameraSide);
            SetDepthOfField(depthEnabled: true);
            PreviousDialogue = new Stack<string>();
            DialogueIsRunning = true;
            DecisionBeingMadeLock = false;
        }

        public void MakeDecision(int choiceIndex)
        {
            DecisionBeingMadeLock = true;
            if (CurrentNode is EditorDecisionNode node)
            {
                CurrentNode = node.MakeDecision(choiceIndex);
                TraverseNodeNetwork();
            }
        }

        private void HandleDialogueText(EditorDialogueNode dialogueNode)
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
                    case EditorStartNode _:
                    case EditorGotoNode _:
                        CurrentNode = CurrentNode.GetNextNode();
                        TraverseNodeNetwork();
                        return;
                    case EditorDialogueNode dialogueNode:
                        HandleDialogueText(dialogueNode);
                        SetCamera();
                        if (dialogueNode.ReachedLastDialogueText(DialogueIndex))
                        {
                            DialogueIndex = -1;
                            CurrentNode = CurrentNode.GetNextNode();
                        }
                        return;
                    case EditorDecisionNode _:
                        UIView.DisplayDecisionNode();
                        SetCamera();
                        return;
                    case EditorActionNode actionNode:
                        actionNode.InvokeAction();
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
            if (CurrentNode is IPositionalNode posNode)
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
            CanvasMain.GetComponent<Canvas>().enabled = visibility;
        }

        //TODO move this as part of Calculator or CameraUtility somewhere else
        public void SetPreDefinedActorPositions(EditorStartNode startNode)
        {
            if (startNode.StartPositionsEnabled)
            {
                foreach (ActorInfo actorInfo in ActorsInScene)
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

                foreach (ActorInfo actorInfo in ActorsInScene)
                {
                    actorInfo.ActorGO.transform.root.LookAt(new Vector3(midPoint.x, actorInfo.ActorGO.transform.root.position.y, midPoint.z));
                }
            }
        }

        public void ReturnActorsToOriginalPositionsIfEnabled()
        {
            if (NodeManager.Instance.StartNode is EditorStartNode startNode && startNode.ReturnToOriginalPositions)
            {
                foreach (ActorInfo actorInfo in ActorsInScene)
                {
                    actorInfo.ActorGO.transform.root.position = actorInfo.OriginalPositionAtStartOfDialogue.position;
                }

                Vector3 midPoint = CamCalculator.CalculateMidPoint();

                foreach (ActorInfo actorInfo in ActorsInScene)
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


