using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public abstract class SpriteSheetManager {
  private static EntityManager entityManager;
  public static List<RenderInformation> renderInformation = new List<RenderInformation>();

  public static EntityManager EntityManager {
    get {
      if(entityManager == null)
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
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
    renderInformation.Add(new RenderInformation(material.material, DynamicBufferManager.GetEntityBuffer(material.material)));
  }

  public static void CleanBuffers() {
    for(int i = 0; i < renderInformation.Count; i++)
      renderInformation[i].DestroyBuffers();
    renderInformation.Clear();
  }

  public static void ReleaseBuffer(int bufferID) {
    if(renderInformation[bufferID].matrixBuffer != null)
      renderInformation[bufferID].matrixBuffer.Release();
    if(renderInformation[bufferID].colorsBuffer != null)
      renderInformation[bufferID].colorsBuffer.Release();
    if(renderInformation[bufferID].uvBuffer != null)
      renderInformation[bufferID].uvBuffer.Release();
    if(renderInformation[bufferID].indexBuffer != null)
      renderInformation[bufferID].indexBuffer.Release();
  }

}
