using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSpriteSheetAnimation.Examples
{
    public class CameraZoom : MonoBehaviour
    {
        [SerializeField]
        private float minFov = 20;
        [SerializeField]
        private float maxFov = 90f;
        [SerializeField]
        private float sensitivity = 1;

        private void Start()
        {
            minFov = 0.000000001f;
        }
        void Update()
        {
            var fov = Camera.main.orthographicSize;
            fov -= Input.GetAxis("Mouse ScrollWheel") * fov * sensitivity;
            fov = Mathf.Clamp(fov, minFov, maxFov);
            Camera.main.orthographicSize = fov;
        }
    } 
}
