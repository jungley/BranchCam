using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.RydenCam.Scripts.NodeCommands
{
    public abstract class TalkableCommand: NodeCommand
    {
        private ITalkable talkableNode { get; set; }

        public TalkableCommand(Node node) : base(node) 
        {
            talkableNode = node as ITalkable;  
        }

        public abstract void AddSpeakingEntry(int index);
        public abstract void RemoveSpeakingEntry(int index);
        public CustomCameraCommand CustomCameraCommand { get; set; }

        public void AssignNewActor(int actorIndex)
        {
            var actor = NodeManager.Instance.ActorsInScene[actorIndex].ActorID;
            talkableNode.NodeConvodata.Actor = NodeManager.Instance.ActorsInScene.Where(x => x.ActorID == actor).FirstOrDefault();
            talkableNode.NodeConvodata.ShotConfig.actor = talkableNode.NodeConvodata.Actor.ActorName;
        }

    }

}
