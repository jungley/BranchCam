using Assets.RydenCam.Scripts.BranchCamCC;
using RydenCam.Common;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace Assets.RydenCam.Scripts.NodeCommands
{
    public class ActionNodeCommand : NodeCommand
    {
        private ActionNode node { get; set; }

        public ActionNodeCommand(Node _node) : base(_node)
        {
            node = _node as ActionNode;
        }

        public void AssignActionObject(GameObject gameObj, int index)
        {
            GameActionData actionData = node.GameActionDatas[index];
            actionData.SelectedMethodIndex = -1;
            actionData.SelectedMethodName = null;
            actionData.SelectedMethodArgValues = Array.Empty<string>();
            actionData.ParameterInfo = null;
            actionData.GameObjectName = gameObj != null ? gameObj.name : null;
            actionData.GameObj = gameObj;
        }

        public void AssignMethod(int actionDataIndex, int methodIndex )
        {
            GameActionData actionData = node.GameActionDatas[actionDataIndex];
            if (actionData.Methods == null || methodIndex < 0 || methodIndex >= actionData.Methods.Length)
                return;
            actionData.SelectedMethodIndex = methodIndex;
            actionData.ParameterInfo = actionData.SelectedMethod.GetParameters();
            actionData.SelectedMethodName = actionData.MethodNames[methodIndex];
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
                    string[] serializedArguments = gameAction.SelectedMethodArgValues ?? Array.Empty<string>();
                    ParameterInfo[] parameters = gameAction.ParameterInfo ?? gameAction.SelectedMethod.GetParameters();
                    if (serializedArguments.Length != parameters.Length)
                    {
                        BranchLog.Error($"Action '{gameAction.SelectedMethod.Name}' has {serializedArguments.Length} saved arguments but expects {parameters.Length}.");
                        continue;
                    }

                    object[] methodArguments = new object[serializedArguments.Length];
                    for (int i = 0; i < serializedArguments.Length; i++)
                        methodArguments[i] = ConvertArgument(serializedArguments[i], parameters[i].ParameterType);

                    if(gameAction.SelectedMethod.IsStatic)
                    {
                        gameAction.SelectedMethod.Invoke(null, methodArguments);
                    }
                    else
                    {
                        object instance = gameAction.MonoBehaviours?
                            .FirstOrDefault(component => gameAction.SelectedMethod.DeclaringType.IsInstanceOfType(component));
                        if (instance == null)
                        {
                            BranchLog.Error($"Component for action '{gameAction.SelectedMethod.Name}' was not found on '{gameAction.GameObj.name}'.");
                            continue;
                        }
                        gameAction.SelectedMethod.Invoke(instance, methodArguments);
                    }
                }
                catch (Exception e)
                {
                    BranchLog.Error("Error with calling method", e);
                }
            }
        }

        private static object ConvertArgument(string value, Type targetType)
        {
            Type actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (actualType == typeof(string)) return value;
            if (actualType.IsEnum) return Enum.Parse(actualType, value, ignoreCase: true);
            return Convert.ChangeType(value, actualType, CultureInfo.InvariantCulture);
        }
    }
}
