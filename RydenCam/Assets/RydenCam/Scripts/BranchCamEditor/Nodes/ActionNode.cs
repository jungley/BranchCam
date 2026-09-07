using RydenCam.BranchCamEditor.Nodes.Connections;
using RydenCam.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamCC
{
    [Serializable]
    public class ActionNode : Node
    {
        public override float NodeHeight
        {
            get
            {
                var namesCount = (GameActionDatas ?? new List<GameActionData>())
                    .Count(x => x != null && !string.IsNullOrEmpty(x.SelectedMethodName));
                return namesCount > 2 ? namesCount * 20 + 50: 80;
            }
        }

        public List<GameActionData> GameActionDatas;

        public ActionNode(Vector2 position) : base(position)
        {
            TypeOfNode = NodeType.ActionNode;
            GameActionDatas = new List<GameActionData>();

            PointIn = new ConnectionPoint(this, ConnectionPointType.In);
            PointOut = new List<ConnectionPoint>() { new ConnectionPoint(this, ConnectionPointType.Out) };
        }
    }


    [Serializable]
    public class GameActionData
    {
        private GameObject _gameObj;

        public GameObject GameObj
        {
            get
            {
                if (_gameObj == null && !string.IsNullOrEmpty(GameObjectName))
                {
                    _gameObj = GameObject.Find(GameObjectName);
                    if (_gameObj != null)
                        AssignLoadedValues();
                }
                return _gameObj;
            }
            set
            {
                _gameObj = value;
                AssignLoadedValues();
            }
        }

        public void AssignLoadedValues()
        {
            if (_gameObj == null && !string.IsNullOrEmpty(GameObjectName))
                _gameObj = GameObject.Find(GameObjectName);

            if (_gameObj == null)
            {
                MonoBehaviours = Array.Empty<MonoBehaviour>();
                Methods = Array.Empty<MethodInfo>();
                MethodNames = new List<string>();
                ParameterInfo = null;
                return;
            }

            GameObjectName = _gameObj.name;
            MonoBehaviours = _gameObj.GetComponents<MonoBehaviour>();
            Methods = MonoBehaviours
                .Where(mb => mb != null)
                .SelectMany(mb => mb.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .Where(method => method.GetParameters().All(parameter => IsSupportedParameterType(parameter.ParameterType)))
                .ToArray();
            MethodNames = Methods
            .Select(m => m.Name)
            .ToList();

            // Reflection order is not a stable serialized contract. Prefer the saved name
            // and argument count, while retaining the index as a legacy fallback.
            if (!string.IsNullOrEmpty(SelectedMethodName))
            {
                int argumentCount = SelectedMethodArgValues?.Length ?? 0;
                int resolvedIndex = Array.FindIndex(Methods, method =>
                    method.Name == SelectedMethodName && method.GetParameters().Length == argumentCount);
                if (resolvedIndex >= 0)
                    SelectedMethodIndex = resolvedIndex;
            }

            if (SelectedMethodIndex >= 0 && SelectedMethodIndex < Methods.Length)
            {
                ParameterInfo = Methods[SelectedMethodIndex]?.GetParameters();
                SelectedMethodName = Methods[SelectedMethodIndex]?.Name;
            }
        }

        private static bool IsSupportedParameterType(Type type)
        {
            Type actualType = Nullable.GetUnderlyingType(type) ?? type;
            return actualType == typeof(string) || actualType == typeof(bool) ||
                   actualType == typeof(int) || actualType == typeof(float) ||
                   actualType == typeof(double) || actualType.IsEnum;
        }

        public MethodInfo SelectedMethod
        {
            get
            {
                if(Methods == null || SelectedMethodIndex < 0 || SelectedMethodIndex >= Methods.Length)
                {
                    return null;
                }
                return Methods[SelectedMethodIndex];
            }
        }


        public MethodInfo[] Methods { get; set; }
        public List<string> MethodNames { get; set; }
        public MonoBehaviour[] MonoBehaviours { get; set; }
        public ParameterInfo[] ParameterInfo { get; set; }

        //Saveable Fields
        [SerializeField]
        public string GameObjectName;
        [SerializeField]
        public string SelectedMethodName;
        [SerializeField]
        public int SelectedMethodIndex;
        [SerializeField]
        public string[] SelectedMethodArgValues;


        public GameActionData()
        {
            SelectedMethodIndex = -1;
            SelectedMethodArgValues = Array.Empty<string>();
        }
    }
}
