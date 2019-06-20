using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace ECSSpriteSheetAnimation.Examples {
  public class MakeSpriteEntities : UnityEngine.MonoBehaviour, IConvertGameObjectToEntity {
    [SerializeField]
    int spriteCount = 5000;

    [SerializeField]
    Material material = null;

    [SerializeField]
    float2 spawnArea = new float2(100, 100);

    Rect GetSpawnArea() {
      Rect r = new Rect(0, 0, spawnArea.x, spawnArea.y);
      r.center = transform.position;
      return r;
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
      var arch = dstManager.CreateArchetype(
            typeof(Position2D),
            typeof(Rotation2D),
            typeof(Scale),
            typeof(Bound2D),
            typeof(SpriteSheet),
            typeof(SpriteSheetAnimation),
            typeof(SpriteSheetMaterial),
            typeof(UvBuffer)
          );

      NativeArray<Entity> entities = new NativeArray<Entity>(spriteCount, Allocator.Temp);
      dstManager.CreateEntity(arch, entities);

      float4[] uvs = SpriteSheetCache.BakeUv(material);
      int cellCount = uvs.Length;

      Random rand = new Random((uint)UnityEngine.Random.Range(0, int.MaxValue));
      Rect area = GetSpawnArea();
      SpriteSheetMaterial mat = new SpriteSheetMaterial { material = material };

      for(int i = 0; i < entities.Length; i++) {
        Entity e = entities[i];

        SpriteSheet sheet = new SpriteSheet { spriteIndex = rand.NextInt(0, cellCount), maxSprites = cellCount };
        Scale scale = new Scale { Value = rand.NextFloat(.25f, 2) };
        Position2D pos = new Position2D { Value = rand.NextFloat2(area.min, area.max) };
        SpriteSheetAnimation anim = new SpriteSheetAnimation { play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, samples = 10 };

        dstManager.SetComponentData(e, sheet);
        dstManager.SetComponentData(e, scale);
        dstManager.SetComponentData(e, pos);
        dstManager.SetComponentData(e, anim);
        dstManager.SetSharedComponentData(e, mat);

        // Fill uv buffer
        var buffer = dstManager.GetBuffer<UvBuffer>(entities[i]);
        for(int j = 0; j < uvs.Length; j++)
          buffer.Add(uvs[j]);

      }
    }

    private void OnDrawGizmosSelected() {
      var r = GetSpawnArea();
      Gizmos.color = new Color(0, .35f, .45f, .24f);
      Gizmos.DrawCube(r.center, r.size);
    }

  }
}