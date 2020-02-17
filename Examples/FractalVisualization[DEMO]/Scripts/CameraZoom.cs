using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour {
  float minFov = 20;
  float maxFov = 90f;
  float sensitivity = 1;

  private void Start() {
    minFov = 0.000000001f;
  }
  void Update() {
    var fov = Camera.main.orthographicSize;
    fov += Input.GetAxis("Mouse ScrollWheel") * fov;
    fov = Mathf.Clamp(fov, minFov, maxFov);
    Camera.main.orthographicSize = fov;
  }
}
