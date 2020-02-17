using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public abstract class SpriteSheetManager {
  private static EntityManager entityManager;

  public static EntityManager EntityManager {
    get {
      if(entityManager == null)
        entityManager = World.Active.EntityManager;
      return entityManager;
    }
  }
  public static Entity Instantiate(EntityArchetype archetype, List<IComponentData> componentDatas, string spriteSheetName) {
    Entity e = EntityManager.CreateEntity(archetype);
    Material material = SpriteSheetCache.GetMaterial(spriteSheetName);
    int bufferID = DynamicBufferManager.AddDynamicBuffers(DynamicBufferManager.GetEntityBuffer(material), material);
    foreach(IComponentData Idata in componentDatas)
      EntityManager.SetComponentData(e, (dynamic)Idata);

    var spriteSheetMaterial = new SpriteSheetMaterial { material = material };
    BufferHook bh = new BufferHook { bufferID = bufferID, bufferEnityID = DynamicBufferManager.GetEntityBufferID(spriteSheetMaterial) };
    EntityManager.SetComponentData(e, bh);
    EntityManager.SetSharedComponentData(e, spriteSheetMaterial);
    return e;
  }

  public static void UpdateEntity(Entity entity, IComponentData componentData) {
    EntityManager.SetComponentData(entity, (dynamic)componentData);
  }

  public static void DestroyEntity(Entity e, string materialName) {
    Material material = SpriteSheetCache.GetMaterial(materialName);
    int bufferID = EntityManager.GetComponentData<BufferHook>(e).bufferID;
    DynamicBufferManager.RemoveBuffer(material, bufferID);
    EntityManager.DestroyEntity(e);
  }

  public static void DestroyEntity(Entity e, Material material, int bufferID) {
    EntityManager.DestroyEntity(e);
    DynamicBufferManager.RemoveBuffer(material, bufferID);
  }

  public static void RecordSpriteSheet(Sprite[] sprites, string spriteSheetName, int spriteCount = 0) {
    KeyValuePair<Material, float4[]> atlasData = SpriteSheetCache.BakeSprites(sprites, spriteSheetName);
    SpriteSheetMaterial material = new SpriteSheetMaterial { material = atlasData.Key };
    DynamicBufferManager.GenerateBuffers(material, spriteCount);
    DynamicBufferManager.BakeUvBuffer(material, atlasData);
  }
}
