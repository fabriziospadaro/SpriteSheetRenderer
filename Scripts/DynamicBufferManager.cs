using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using System;

//todo entity is a dictionary with spritesheetmaterial and is used to separate buffers from different material

public static class DynamicBufferManager {
  public static EntityManager manager;
  public static List<Entity> bufferEntities = new List<Entity>();
  //todo: have the bufferentity id from the material(so we cache id and material)
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
    AddDynamicBuffers(bufferEntities[bufferEntities.Count - 1], entityCount);
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
    Entity e = manager.CreateEntity(archetype);
    bufferEntities.Add(e);
    manager.SetSharedComponentData(e, material);
  }


  //when u create a new entity you need a new buffer for him
  //use this to add new dynamicbuffer
  public static void AddDynamicBuffers(Entity bufferEntity, int entityCount = 1) {
    var indexBuffer = manager.GetBuffer<SpriteIndexBuffer>(bufferEntity);
    var colorBuffer = manager.GetBuffer<SpriteColorBuffer>(bufferEntity);
    var matrixBuffer = manager.GetBuffer<MatrixBuffer>(bufferEntity);
    for(int i = 0; i < entityCount; i++) {
      indexBuffer.Add(new SpriteIndexBuffer());
      matrixBuffer.Add(new MatrixBuffer());
      colorBuffer.Add(new SpriteColorBuffer());
    }
  }

  public static DynamicBuffer<SpriteIndexBuffer>[] GetIndexBuffer() {
    DynamicBuffer<SpriteIndexBuffer>[] buffers = new DynamicBuffer<SpriteIndexBuffer>[bufferEntities.Count];
    for(int i = 0; i < buffers.Length; i++)
      buffers[i] = manager.GetBuffer<SpriteIndexBuffer>(bufferEntities[i]);
    return buffers;
  }
  public static DynamicBuffer<MatrixBuffer>[] GetMatrixBuffer() {
    DynamicBuffer<MatrixBuffer>[] buffers = new DynamicBuffer<MatrixBuffer>[bufferEntities.Count];
    for(int i = 0; i < buffers.Length; i++)
      buffers[i] = manager.GetBuffer<MatrixBuffer>(bufferEntities[i]);
    return buffers;
  }
  public static DynamicBuffer<SpriteColorBuffer>[] GetColorBuffer() {
    DynamicBuffer<SpriteColorBuffer>[] buffers = new DynamicBuffer<SpriteColorBuffer>[bufferEntities.Count];
    for(int i = 0; i < buffers.Length; i++)
      buffers[i] = manager.GetBuffer<SpriteColorBuffer>(bufferEntities[i]);
    return buffers;
  }

}
