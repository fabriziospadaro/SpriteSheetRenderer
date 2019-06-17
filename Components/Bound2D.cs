using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Bound2D : IComponentData {
  public float2 position;
  public float2 scale;
  public bool visibile;
}

public abstract class Bound2DExtension {
  public static bool Intersects(float2 positionA, float2 scaleA, float2 positionB, float2 scaleB) {
    return
      (Abs(positionA.x - positionB.x) * 2 < (scaleA.x + scaleB.x)) &&
      (Abs(positionA.y - positionB.y) * 2 < (scaleA.y + scaleB.y));
  }
  //should be faster than math.abs
  static float Abs(float x) {
    return (x >= 0) ? x : -x;
  }
  public static float2[] BoundValuesFromCamera(Camera camera) {
    float screenAspect = (float)Screen.width / (float)Screen.height;
    float cameraHeight = camera.orthographicSize * 2;
    return new float2[2] { (Vector2)camera.transform.position, new float2(cameraHeight * screenAspect, cameraHeight) };
  }
}
