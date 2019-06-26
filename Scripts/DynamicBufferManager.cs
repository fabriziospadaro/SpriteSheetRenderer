using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using System;

//todo entity is a dictionary with spritesheetmaterial and is used to separate buffers from different material

public static class DynamicBufferManager {
  public static EntityManager manager;
  public static Entity entity = Entity.Null;
  public static void BakeUvBuffer(SpriteSheetMaterial spriteSheetMaterial, KeyValuePair<Material, float4[]> atlasData) {
    Entity entityA = manager.CreateEntity(typeof(SpriteSheetMaterial), typeof(UvBuffer));
    // Fill uv buffer
    manager.SetSharedComponentData(entityA, spriteSheetMaterial);
    var buffer = manager.GetBuffer<UvBuffer>(entityA);
    for(int j = 0; j < atlasData.Value.Length; j++)
      buffer.Add(atlasData.Value[j]);
  }

  public static void GenerateBuffers(SpriteSheetMaterial material, int entityCount) {
    CreateBuffersContainer(material);
    AddDynamicBuffers(entityCount);
  }
  //use this when it's the first time you are using that material
  //use this just to generate the buffers container
  public static void CreateBuffersContainer(SpriteSheetMaterial material) {
    var archetype = manager.CreateArchetype(
      typeof(SpriteIndexBuffer),
      typeof(MatrixBuffer),
      typeof(SpriteColorBuffer),
      typeof(SpriteSheetMaterial)
    );
    entity = manager.CreateEntity(archetype);
    manager.SetSharedComponentData(entity, material);
  }


  //when u create a new entity you need a new buffer for him
  //use this to add new dynamicbuffer
  public static void AddDynamicBuffers(int entityCount = 1) {
    var indexBuffer = manager.GetBuffer<SpriteIndexBuffer>(entity);
    var colorBuffer = manager.GetBuffer<SpriteColorBuffer>(entity);
    var matrixBuffer = manager.GetBuffer<MatrixBuffer>(entity);
    for(int i = 0; i < entityCount; i++) {
      indexBuffer.Add(new SpriteIndexBuffer());
      matrixBuffer.Add(new MatrixBuffer());
      colorBuffer.Add(new SpriteColorBuffer());
    }
  }

  public static DynamicBuffer<SpriteIndexBuffer> GetIndexBuffer() {
    return manager.GetBuffer<SpriteIndexBuffer>(entity);
  }
  public static DynamicBuffer<MatrixBuffer> GetMatrixBuffer() {
    return manager.GetBuffer<MatrixBuffer>(entity);
  }
  public static DynamicBuffer<SpriteColorBuffer> GetColorBuffer() {
    return manager.GetBuffer<SpriteColorBuffer>(entity);
  }

}
