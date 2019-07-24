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
    private EntityArchetype archetype;
    public static MakeSpriteEntities instance;
    Rect GetSpawnArea() {
      Rect r = new Rect(0, 0, spawnArea.x, spawnArea.y);
      r.center = transform.position;
      return r;
    }

    public void Convert(Entity entity, EntityManager eManager, GameObjectConversionSystem conversionSystem) {
      instance = this;
      archetype = eManager.CreateArchetype(
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

      //only needed for first tiem material baking
      KeyValuePair<Material, float4[]> atlasData = SpriteSheetCache.BakeSprites(sprites, "emoji");
      SpriteSheetMaterial material = new SpriteSheetMaterial { material = atlasData.Key };
      DynamicBufferManager.manager = eManager;
      DynamicBufferManager.GenerateBuffers(material, entities.Length);
      DynamicBufferManager.BakeUvBuffer(material, atlasData);

      Rect area = GetSpawnArea();
      Random rand = new Random((uint)UnityEngine.Random.Range(0, int.MaxValue));
      int cellCount = atlasData.Value.Length;
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
    public static Entity SpawnEntity(EntityCommandBuffer eManager, string materialName) {
      var e = eManager.CreateEntity(instance.archetype);
      //only needed for first tiem material baking
      Material material = SpriteSheetCache.materialNameMaterial[materialName];

      int bufferID = DynamicBufferManager.AddDynamicBuffers(DynamicBufferManager.GetEntityBuffer(material), material);
      Entity uvBuffer = DynamicBufferManager.GetEntityBuffer(material);
      int maxSprites = DynamicBufferManager.manager.GetBuffer<UvBuffer>(uvBuffer).Length;

      Rect area = instance.GetSpawnArea();
      Random rand = new Random((uint)UnityEngine.Random.Range(0, int.MaxValue));
      eManager.SetComponent(e, new SpriteIndex { Value = 0 });
      eManager.SetComponent(e, new Scale { Value = 20 });
      eManager.SetComponent(e, new Position2D { Value = rand.NextFloat2(area.min, area.max) });
      eManager.SetComponent(e, new SpriteSheetAnimation { maxSprites = maxSprites, play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, samples = 10 });
      var color = UnityEngine.Random.ColorHSV(.15f, .75f);
      SpriteSheetColor col = new SpriteSheetColor { color = new float4(color.r, color.g, color.b, color.a) };
      eManager.SetComponent(e, col);
      var spriteSheetMaterial = new SpriteSheetMaterial { material = material };
      eManager.SetComponent(e, new BufferHook { bufferID = bufferID, bufferEnityID = DynamicBufferManager.GetEntityBufferID(spriteSheetMaterial) });
      eManager.SetSharedComponent(e, spriteSheetMaterial);
      return e;
    }

    public static void DestroyEntity(EntityCommandBuffer eManager, Entity e, string materialName) {
      eManager.DestroyEntity(e);
      Material material = SpriteSheetCache.materialNameMaterial[materialName];
      int bufferID = DynamicBufferManager.manager.GetComponentData<BufferHook>(e).bufferID;
      DynamicBufferManager.RemoveBuffer(material, bufferID);
    }

    public static void DestroyEntity(EntityCommandBuffer eManager, Entity e, Material material, int bufferID) {
      eManager.DestroyEntity(e);
      DynamicBufferManager.RemoveBuffer(material, bufferID);
    }
  }
}