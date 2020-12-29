using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;
using Unity.Entities;
using System.Linq;

public class EntitySpawner : MonoBehaviour
{
    QuadTree qt = null;
    private Text spriteCount;

    private void Start()
    {
        qt = FractalInitializer.qt;
        spriteCount = GameObject.Find("EntityCount").GetComponent<Text>();
    }

    public void Update()
    {
        if (Input.GetMouseButton(0))
        {
            var screenPoint = Input.mousePosition;
            screenPoint.z = 10.0f; //distance of the plane from the camera
            screenPoint = Camera.main.ScreenToWorldPoint(screenPoint);
            qt.Insert(new float2(screenPoint.x, screenPoint.y));
        }
        if (Input.GetMouseButtonDown(1))
        {
            var screenPoint = Input.mousePosition;
            screenPoint.z = 10.0f; //distance of the plane from the camera
            screenPoint = Camera.main.ScreenToWorldPoint(screenPoint);
            qt.Insert(new float2(screenPoint.x, screenPoint.y));
        }
        spriteCount.text = "Entity Count: " + (World.DefaultGameObjectInjectionWorld.EntityManager.Debug.EntityCount - 2).ToString();
    }

    public void Subdivide()
    {
        qt.Insert(float2.zero, true);
    }
}
