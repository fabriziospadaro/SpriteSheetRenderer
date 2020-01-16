using UnityEngine;
using Unity.Mathematics;
public class EntitySpawner : MonoBehaviour {
  QuadTree qt = null;

  private void Start() {
    qt = QuadTreeWorldInitializer.qt;

  }

  public void Update() {
    if(Input.GetMouseButton(0)) {
      var screenPoint = Input.mousePosition;
      screenPoint.z = 10.0f; //distance of the plane from the camera
      screenPoint = Camera.main.ScreenToWorldPoint(screenPoint);
      qt.Insert(new float2(screenPoint.x, screenPoint.y));
    }
    if(Input.GetMouseButtonDown(1)) {
      var screenPoint = Input.mousePosition;
      screenPoint.z = 10.0f; //distance of the plane from the camera
      screenPoint = Camera.main.ScreenToWorldPoint(screenPoint);
      qt.Insert(new float2(screenPoint.x, screenPoint.y));
    }
  }
}
