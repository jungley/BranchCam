using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.SequenceData;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.RydenCam.Scripts.NodeCommands
{
    public class StartNodeCommand : INodeCommand
    {
        private StartNode node { get; set; }

        public StartNodeCommand(Node _node)
        {
            node = _node as StartNode;
        }
            
        public void AddActor()
        {
            node.ActorsInScene.Add(new ActorInfo());

            //Check if there any nodes with null Actors and assign them the 
            //first in the list
            var firstActor = node.ActorsInScene.FirstOrDefault(x => x?.ActorID != null);
            if (firstActor != null)
            {
                foreach (var node in NodeManager.Instance.Nodes.OfType<ITalkable>())
                {
                    if (node.NodeConvodata.Actor == null)
                    {
                        node.NodeConvodata.Actor = firstActor;
                        node.NodeConvodata.ShotConfig.actor = firstActor.ActorName;
                    }
                }
            }
            
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
                    }
                }
            }

            //Remove Actor from Actors List
            node.ActorsInScene.RemoveAt(index);

        }

        public string GetPreDefinedStartPositionDisplayData(ActorInfo actorInfo)
        {
            if (actorInfo.PreDefinedStartPosition.position == Vector3.zero)
            {
                return "<Not Assigned>";
            }
            else
            {
                // Format the position components to two decimal places
                float x = Mathf.Round(actorInfo.PreDefinedStartPosition.position.x * 100) / 100;
                float y = Mathf.Round(actorInfo.PreDefinedStartPosition.position.y * 100) / 100;
                float z = Mathf.Round(actorInfo.PreDefinedStartPosition.position.z * 100) / 100;

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



        public void RemoveNode(Node node)
        {
            NodeManager.Instance.RemoveNode(node);
            ConnectionManager.Instance.RemoveAssociatedConnections(node);
            NodeManager.Instance.ActiveNode = null;

        }
    }
}
