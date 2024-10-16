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
        private GameObject _gameObj;

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
                AssignLoadedValues();
            }
        }

        public void AssignLoadedValues()
        {
            if (GameObj == null) return;

            GameObjectName = GameObj?.name;
            MonoBehaviours = GameObj.GetComponents<MonoBehaviour>();
            Methods = MonoBehaviours
                .SelectMany(mb => mb.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                .ToArray();
            MethodNames = Methods
            .Select(m => m.Name)
            .ToList();

            if (SelectedMethodIndex >= 0 && SelectedMethodIndex < Methods.Length)
            {
                ParameterInfo = Methods[SelectedMethodIndex]?.GetParameters();
            }
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

        }
    }

    [Serializable]
    public class ActionNode : NodeCC
    {
        public override float NodeHeight
        {
            get
            {
                var namesCount = GameActionDatas.Where(x => !string.IsNullOrEmpty(x.SelectedMethodName)).Count();
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
}
