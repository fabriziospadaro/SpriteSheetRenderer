using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;
namespace ECSSpriteSheetAnimation.Examples {
  public class MakeSpriteEntities : MonoBehaviour, IConvertGameObjectToEntity {
    public int spriteCount = 5000;
    public Sprite[] sprites;
    public float2 spawnArea = new float2(100, 100);
    Rect GetSpawnArea() {
      Rect r = new Rect(0, 0, spawnArea.x, spawnArea.y);
      r.center = transform.position;
      return r;
    }

    public void Convert(Entity entity, EntityManager eManager, GameObjectConversionSystem conversionSystem) {
      EntityArchetype archetype = eManager.CreateArchetype(
         typeof(Position2D),
         typeof(Rotation2D),
         typeof(Scale),
         //required params
         typeof(SpriteIndex),
         typeof(SpriteSheetAnimation),
         typeof(SpriteSheetMaterial),
         typeof(SpriteSheetColor),
         typeof(SpriteMatrix),
         typeof(BufferHook)
      );

      NativeArray<Entity> entities = new NativeArray<Entity>(spriteCount, Allocator.Temp);
      eManager.CreateEntity(archetype, entities);

      //only needed for the first time to bake the material and create the uv map
      SpriteSheetManager.RecordSpriteSheet(sprites, "emoji", entities.Length);


      Rect area = GetSpawnArea();
      Random rand = new Random((uint)UnityEngine.Random.Range(0, int.MaxValue));
      int cellCount = SpriteSheetCache.GetLength("emoji");
      SpriteSheetMaterial material = new SpriteSheetMaterial { material = SpriteSheetCache.GetMaterial("emoji") };

      for(int i = 0; i < entities.Length; i++) {
        Entity e = entities[i];
        eManager.SetComponentData(e, new SpriteIndex { Value = rand.NextInt(0, cellCount) });
        eManager.SetComponentData(e, new Scale { Value = 10 });
        eManager.SetComponentData(e, new Position2D { Value = rand.NextFloat2(area.min, area.max) });
        eManager.SetComponentData(e, new SpriteSheetAnimation { maxSprites = cellCount, play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, samples = 10 });
        var color = UnityEngine.Random.ColorHSV(.15f, .75f);
        SpriteSheetColor col = new SpriteSheetColor { color = new float4(color.r, color.g, color.b, color.a) };
        eManager.SetComponentData(e, col);
        eManager.SetComponentData(e, new BufferHook { bufferID = i, bufferEnityID = DynamicBufferManager.GetEntityBufferID(material) });
        eManager.SetSharedComponentData(e, material);
      }
    }
    private void OnDrawGizmosSelected() {
      var r = GetSpawnArea();
      Gizmos.color = new Color(0, .35f, .45f, .24f);
      Gizmos.DrawCube(r.center, r.size);
    }
  }
}