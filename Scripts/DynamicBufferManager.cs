using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
public static class DynamicBufferManager {
  public static void BakeUvBuffer(EntityManager manager, SpriteSheetMaterial spriteSheetMaterial, KeyValuePair<Material, float4[]> atlasData) {
    Entity entity = manager.CreateEntity(typeof(SpriteSheetMaterial), typeof(UvBuffer));
    // Fill uv buffer
    manager.SetSharedComponentData(entity, spriteSheetMaterial);
    var buffer = manager.GetBuffer<UvBuffer>(entity);
    for(int j = 0; j < atlasData.Value.Length; j++)
      buffer.Add(atlasData.Value[j]);
  }
}
