using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RydenCam.Utilities
{
    public class MonoUtility : MonoBehaviour
    {
        private static MonoUtility _instance;

        public static MonoUtility Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MonoUtility>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject(nameof(MonoUtility));
                        _instance = obj.AddComponent<MonoUtility>();
                    }
                }
                return _instance;
            }
        }
    }
}
