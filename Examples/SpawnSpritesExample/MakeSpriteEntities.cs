using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using UnityEngine.U2D;
namespace ECSSpriteSheetAnimation.Examples {
  public class MakeSpriteEntities : UnityEngine.MonoBehaviour, IConvertGameObjectToEntity {
    [SerializeField]
    int spriteCount = 5000;

    [SerializeField]
    Sprite[] sprites;

    [SerializeField]
    float2 spawnArea = new float2(100, 100);
    [SerializeField]
    float minScale = .25f;
    [SerializeField]
    float maxScale = 2f;


    Rect GetSpawnArea() {
      Rect r = new Rect(0, 0, spawnArea.x, spawnArea.y);
      r.center = transform.position;
      return r;
    }

    public void Convert(Entity entity, EntityManager eManager, GameObjectConversionSystem conversionSystem) {
      var archetype = eManager.CreateArchetype(
         typeof(Position2D),
         typeof(Rotation2D),
         typeof(Scale),
         typeof(Bound2D),
         typeof(SpriteSheet),
         typeof(SpriteSheetAnimation),
         typeof(SpriteSheetMaterial),
         typeof(UvBuffer),
         typeof(SpriteColor),
         typeof(RenderData)
      );

      NativeArray<Entity> entities = new NativeArray<Entity>(spriteCount, Allocator.Temp);
      eManager.CreateEntity(archetype, entities);

      KeyValuePair<Material, float4[]> atlasData = SpriteSheetCache.BakeSprites(sprites);
      int cellCount = atlasData.Value.Length;

      Random rand = new Random((uint)UnityEngine.Random.Range(0, int.MaxValue));
      Rect area = GetSpawnArea();
      SpriteSheetMaterial material = new SpriteSheetMaterial { material = atlasData.Key };
      for(int i = 0; i < entities.Length; i++) {
        Entity e = entities[i];

        SpriteSheet sheet = new SpriteSheet { spriteIndex = rand.NextInt(0, cellCount), maxSprites = cellCount };
        Scale scale = new Scale { Value = rand.NextFloat(minScale, maxScale) };
        Position2D pos = new Position2D { Value = rand.NextFloat2(area.min, area.max) };
        SpriteSheetAnimation anim = new SpriteSheetAnimation { play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, samples = 10 };
        var color = UnityEngine.Random.ColorHSV(.15f, .75f);
        SpriteColor col = new SpriteColor { value = new float4(color.r, color.g, color.b, color.a) };
        eManager.SetComponentData(e, sheet);
        eManager.SetComponentData(e, scale);
        eManager.SetComponentData(e, pos);
        eManager.SetComponentData(e, anim);
        eManager.SetComponentData(e, col);
        eManager.SetSharedComponentData(e, material);

        // Fill uv buffer
        var buffer = eManager.GetBuffer<UvBuffer>(entities[i]);
        for(int j = 0; j < atlasData.Value.Length; j++)
          buffer.Add(atlasData.Value[j]);

      }
    }

    private void OnDrawGizmosSelected() {
      var r = GetSpawnArea();
      Gizmos.color = new Color(0, .35f, .45f, .24f);
      Gizmos.DrawCube(r.center, r.size);
    }

  }
}