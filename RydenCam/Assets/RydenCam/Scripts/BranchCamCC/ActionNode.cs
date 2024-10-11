using Newtonsoft.Json;
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
    public class GameActionData
    {
        //Generic Properties
        [SerializeField]
        public string GameObjectName;

        [JsonIgnore]
        private GameObject _gameObj;

        [JsonIgnore]
        public GameObject GameObj
        {
            get
            {
                if (_gameObj == null)
                {
                    _gameObj = GameObject.Find(GameObjectName);
                }
                return _gameObj;
            }
            set
            {
                _gameObj = value;
                GameObjectName = _gameObj?.name;
                MonoBehaviours = GameObj.GetComponents<MonoBehaviour>();
                Methods = MonoBehaviours
                    .SelectMany(mb => mb.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    .ToArray();

                MethodNames = Methods
                .Select(m => m.Name)
                .ToList();

            }
        }

        [JsonIgnore]
        public MethodInfo[] Methods { get; set; }
        [JsonIgnore]
        public List<string> MethodNames { get; set; }
        [JsonIgnore]
        public MonoBehaviour[] MonoBehaviours { get; set; }
        [JsonIgnore]
        public ParameterInfo[] ParameterInfo;

        //Selected Properties
        [SerializeField]
        public string SelectedMethodName;
        [SerializeField]
        public int SelectedMethodIndex;
        [SerializeField]
        public string[] SelectedMethodArgValues;
        [JsonIgnore]
        public MethodInfo SelectedMethod
        {
            get
            {
                return (Methods == null
                 || SelectedMethodIndex < 0
                 || SelectedMethodIndex >= Methods.Length
                 ) ? null 
                 
                 : Methods[SelectedMethodIndex];

            }
        }

        public GameActionData()
        {

        }
    }


    public class ActionNode : NodeCC
    {

        public override float NodeHeight => 70; //needs to eventually dynamically change?


        public List<GameActionData> GameActionDatas { get; set; }

        public ActionNode(Vector2 position) : base(position)
        {
            TypeOfNode = NodeType.ActionNode;
            GameActionDatas = new List<GameActionData>();

            PointIn = new ConnectionPoint(this, ConnectionPointType.In);
            PointOut = new List<ConnectionPoint>() { new ConnectionPoint(this, ConnectionPointType.Out) };
        }
    }
}
