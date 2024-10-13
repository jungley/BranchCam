using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.BranchCamEditor.Managers;
using RydenCam.Common;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace Assets.RydenCam.Scripts.NodeCommands
{
    public class ActionNodeCommand : INodeCommand
    {
        private ActionNode node { get; set; }

        public ActionNodeCommand(NodeCC _node)
        {
            node = _node as ActionNode;
        }

        public void AssignActionObject(GameObject gameObj, int index)
        {
            node.GameActionDatas[index].GameObj = gameObj;
            node.GameActionDatas[index].SelectedMethodIndex = -1;
        }

        public void AssignMethod(int actionDataIndex, int methodIndex )
        {
            GameActionData actionData = node.GameActionDatas[actionDataIndex];
            actionData.SelectedMethodIndex = methodIndex;
            actionData.ParameterInfo = actionData.SelectedMethod.GetParameters();
            actionData.SelectedMethodArgValues = new string[actionData.ParameterInfo.Length];

        }

        public void AddCommand()
        {
            node.GameActionDatas.Add(new GameActionData());
        }

        public void InvokeCommands()
        {
            foreach (GameActionData gameAction in node.GameActionDatas)
            {
                try
                {
                    if (gameAction.GameObj == null || gameAction.SelectedMethod == null)
                        continue;

                    // Convert the arguments from strings to the appropriate types
                    object[] methodArguments = new object[gameAction.SelectedMethodArgValues.Count()];
                    for (int i = 0; i < gameAction.SelectedMethodArgValues.Length; i++)
                    {
                        methodArguments[i] = Convert.ChangeType(gameAction.SelectedMethodArgValues[i], gameAction.ParameterInfo[i].ParameterType);
                    }

                    if (gameAction.SelectedMethod != null)
                    {
                        //If it is instance, need to create an instance??
                        if(gameAction.SelectedMethod.IsStatic)
                        {
                            gameAction.SelectedMethod.Invoke(null, methodArguments);
                        }
                        else
                        {
                            Type myClassType = gameAction.SelectedMethod.ReflectedType;
                            object instance = Activator.CreateInstance(myClassType);
                            gameAction.SelectedMethod.Invoke(instance, methodArguments);

                        }
                    }
                }
                catch (Exception e)
                {
                    BranchLog.Error("Error with calling method", e);
                }
            }
        }

        public void RemoveNode(NodeCC node)
        {
            NodeManager.Instance.RemoveNode(node);
            ConnectionManager.Instance.RemoveAssociatedConnections(node);
            NodeManager.Instance.ActiveNode = null;
        }
    }
}