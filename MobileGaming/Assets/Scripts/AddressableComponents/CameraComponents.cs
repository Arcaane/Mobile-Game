using System;
using UnityEngine;

namespace Addressables.Components
{
    public class CameraComponents : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject _perspCameraGo;
        [SerializeField] private Camera _othoCamera;
        [SerializeField] private Camera _perspCamera;
        
        public GameObject perspCameraGo => _perspCameraGo;
        public Camera perspCamera => _perspCamera;
        public Camera othoCamera => _othoCamera;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}


