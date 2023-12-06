using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using RydenCam.SequenceData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RydenCam.BranchCamEditor.Controllers
{
    public class EditorController
    {
        public List<ActorInfo> ActorsInScene { get; set; }

        private static EditorController editorController;
        public static EditorController Instance
        {
            get
            {
                if(editorController == null)
                {
                    editorController = new EditorController();
                }

                return editorController;
            }
        }

        public EditorController()
        {
            ActorsInScene = new List<ActorInfo>();
        }

        // Use this for initialization
        public void ResetEverything()
        {
            ActorsInScene = new List<ActorInfo>();
            NodeManager.Instance.Clear();
            ConnectionManager.Instance.Clear();
#if UNITY_EDITOR
            BranchCamEditor.startNodeAdded = false;
            BranchCamEditor.ActiveNode = null;
#endif
        }

        //TODO Move this to 
        //a Drawing Utilities class?
        //Redraw all the nodes
        //Called when actor is deleted
        //When choice is deleted
        public void RedrawAll()
        {
            //Loop through all the nodes
            foreach (var item in NodeManager.Instance.GetList())
            {
                item.DrawContent();
                item.DrawForInspector();
            }
        }
    }
}
