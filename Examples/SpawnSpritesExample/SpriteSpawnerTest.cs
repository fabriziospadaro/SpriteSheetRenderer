using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace ECSSpriteSheetAnimation.Examples {
  public class SpriteSpawnerTest : UnityEngine.MonoBehaviour {
    
    [System.Serializable]
    public struct SpawnData : IComponentData {
      public int spriteCount;
      public float2 spawnArea;
      public float minScale;
      public float maxScale;
      public float minFPS_;
      public float maxFPS_;
      public float rotation;
      [HideInInspector]
      public float3 origin;

      public Rect GetSpawnArea() {
        Rect r = new Rect(0, 0, spawnArea.x, spawnArea.y);
        r.center = Vector2.zero;
        return r;
      }
    }

    [SerializeField]
    SpawnData spawnData = new SpawnData {
      spriteCount = 10000,
      spawnArea = new float2(100, 100),
      minScale = .25f,
      maxScale = 2f,
      minFPS_ = 0.05f,
      maxFPS_ = 1.5f,
    };

    [SerializeField]
    Material mat_ = null;
    
    public void MakeEntities() {
      spawnData.origin = transform.position;
      var e = World.Active.EntityManager.CreateEntity(typeof(SpawnData));
      World.Active.EntityManager.SetComponentData(e, spawnData);
      World.Active.EntityManager.AddSharedComponentData(e, new SpriteSheetMaterial { material = mat_ });

    }

    private void Start() {
      MakeEntities();
    }

    private void OnDrawGizmos() {
      var r = spawnData.GetSpawnArea();
      r.center = transform.position;
      Gizmos.color = new Color(0, .35f, .45f, .24f);
      Gizmos.DrawCube(r.center, r.size);
    }

  }
}