using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.BranchCamEditor.PreviewRender.ActorPreviewSetup;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.SequenceData;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.RydenCam.Scripts.NodeCommands
{
    public class StartNodeCommand : NodeCommand
    {
        private StartNode node { get; set; }

        public StartNodeCommand(Node _node) : base(_node)
        {
            node = _node as StartNode;
        }
            
        public void AddActor()
        {
            node.ActorsInScene.Add(new ActorInfo());
        }

        public void RemoveActor(int index)
        {
            //Clear out Actor information from all nodes
            ActorInfo actor = node.ActorsInScene[index];
            foreach(var node in NodeManager.Instance.Nodes)
            {
                if(node is ITalkable talkable)
                {
                    if(talkable.NodeConvodata.Actor?.ActorID == actor.ActorID)
                    {
                        talkable.NodeConvodata.Actor = null;
                         talkable.NodeConvodata.ShotConfig = null;
                    }
                }
            }

            //Remove Actor from Actors List
            node.ActorsInScene.RemoveAt(index);

            //Recalculate Actor Positions in Preview
            SetupPreviewSceneData.CalculateActorsInPreviewSpace();

        }

        public string GetPreDefinedStartPositionDisplayData(ActorInfo actor)
        {
            if (actor.PreDefinedStartPosition.position == Vector3.zero)
            {
                return "<Not Assigned>";
            }
            else
            {
                // Format the position components to two decimal places
                float x = Mathf.Round(actor.PreDefinedStartPosition.position.x * 100) / 100;
                float y = Mathf.Round(actor.PreDefinedStartPosition.position.y * 100) / 100;
                float z = Mathf.Round(actor.PreDefinedStartPosition.position.z * 100) / 100;

                // Create a formatted string with the position data
                return $"Position Set ✓ X:{x:0.00} Y:{y:0.00} Z:{z:0.00}";
            }
        }

        public void ClearActorsStartPositinonData()
        {
            foreach (ActorInfo actor in node.ActorsInScene)
            {
                if (actor == null || actor.ActorGO == null)
                    continue;

                actor.PreDefinedStartPositionEnabled = false;
                actor.PreDefinedStartPosition = Pose.identity;
            }
            node.UnitySceneName = string.Empty;
        }

        public void AssignActorStartPositionData()
        {
            if (node.ActorsInScene.All(actor => actor.ActorGO == null))
                return;


            foreach (ActorInfo actor in node.ActorsInScene)
            {
                if (actor == null || actor.ActorGO == null)
                    continue;

                actor.PreDefinedStartPositionEnabled = true;
                actor.PreDefinedStartPosition = new Pose(actor.ActorGO.transform.root.position, actor.ActorGO.transform.root.rotation);
            }
            node.UnitySceneName = SceneManager.GetActiveScene().name;
        }
    }
}
