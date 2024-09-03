using Assets.RydenCam.Scripts.BranchCamCC;
using Assets.RydenCam.Scripts.NodeCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.RydenCam.Scripts.Editor.NodeDrawer
{
    internal class ActionNodeDrawer : NodeDrawerBase
    {

        private ActionNode node { get; set; }
        private ActionNodeCommand command { get; set; }

        public ActionNodeDrawer(NodeCC _node) : base(_node) 
        {
            node = _node as ActionNode;
            Command = new ActionNodeCommand(node);

            //Editor Window Properties
            nodeWidth = 200;

            ColorUtility.TryParseHtmlString("#1700FF", out Color colorref);
            nodeColor = colorref;
        }


        public override void DrawNode( int index)
        {
            GUI.backgroundColor = Color.gray;

            windowRect = GUI.Window(index, new Rect(node.EditorPosition.x, node.EditorPosition.y, nodeWidth, nodeHeight),
                (windowId) =>
                {
                    GUI.DrawTextureWithTexCoords(new Rect(0, 0, 280.0f, 25f), HeaderTexture, new Rect(0, 0, 1, 1));



                }, "");

            Node.EditorPosition = new Vector2(windowRect.x, windowRect.y);

        }

        public override void DrawNodeInspector()
        {

            //throw new NotImplementedException();
        }
    }
}
