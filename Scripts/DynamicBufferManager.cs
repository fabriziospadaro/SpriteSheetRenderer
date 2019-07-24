using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using System;
using System.Linq;

//todo entity is a dictionary with spritesheetmaterial and is used to separate buffers from different material

public static class DynamicBufferManager {
  public static EntityManager manager;
  //list of all the "Enities with all the buffers"
  //Each different material have a different Enity
  public static List<Entity> bufferEntities = new List<Entity>();
  //contains the index of a bufferEntity inside the bufferEntities from a material
  public static Dictionary<Material, int> materialEntityBufferID = new Dictionary<Material, int>();

  public static Dictionary<Material, List<int>> availableEntityID = new Dictionary<Material, List<int>>();

  //only use this when you didn't bake the uv yet
  public static void BakeUvBuffer(SpriteSheetMaterial spriteSheetMaterial, KeyValuePair<Material, float4[]> atlasData) {
    Entity entity = GetEntityBuffer(spriteSheetMaterial.material);
    var buffer = manager.GetBuffer<UvBuffer>(entity);
    for(int j = 0; j < atlasData.Value.Length; j++)
      buffer.Add(atlasData.Value[j]);
  }

  public static void GenerateBuffers(SpriteSheetMaterial material, int entityCount) {
    if(!materialEntityBufferID.ContainsKey(material.material)) {
      CreateBuffersContainer(material);
      availableEntityID.Add(material.material, new List<int>());
      for(int i = 0; i < entityCount; i++)
        availableEntityID[material.material].Add(i);
      MassAddBuffers(bufferEntities.Last(), entityCount);
    }
  }

  //use this when it's the first time you are using that material
  //use this just to generate the buffers container
  public static void CreateBuffersContainer(SpriteSheetMaterial material) {
    var archetype = manager.CreateArchetype(
      typeof(SpriteIndexBuffer),
      typeof(MatrixBuffer),
      typeof(SpriteColorBuffer),
      typeof(SpriteSheetMaterial),
      typeof(UvBuffer)
    );
    Entity e = manager.CreateEntity(archetype);
    bufferEntities.Add(e);
    manager.SetSharedComponentData(e, material);
    materialEntityBufferID.Add(material.material, materialEntityBufferID.Count);
  }

  //when u create a new entity you need a new buffer for him
  //use this to add new dynamicbuffer
  public static void MassAddBuffers(Entity bufferEntity, int entityCount) {
    var indexBuffer = manager.GetBuffer<SpriteIndexBuffer>(bufferEntity);
    var colorBuffer = manager.GetBuffer<SpriteColorBuffer>(bufferEntity);
    var matrixBuffer = manager.GetBuffer<MatrixBuffer>(bufferEntity);
    for(int i = 0; i < entityCount; i++) {
      indexBuffer.Add(new SpriteIndexBuffer());
      matrixBuffer.Add(new MatrixBuffer());
      colorBuffer.Add(new SpriteColorBuffer());
    }
  }

  public static int AddDynamicBuffers(Entity bufferEntity, Material material) {
    int bufferId = NextIDForEntity(material);
    var indexBuffer = manager.GetBuffer<SpriteIndexBuffer>(bufferEntity);
    var colorBuffer = manager.GetBuffer<SpriteColorBuffer>(bufferEntity);
    var matrixBuffer = manager.GetBuffer<MatrixBuffer>(bufferEntity);
    if(indexBuffer.Length <= bufferId) {
      indexBuffer.Add(new SpriteIndexBuffer());
      colorBuffer.Add(new SpriteColorBuffer());
      matrixBuffer.Add(new MatrixBuffer());
    }
    return bufferId;
  }

  public static BufferHook GetBufferHook(SpriteSheetMaterial material) {
    return new BufferHook { bufferEnityID = materialEntityBufferID[material.material], bufferID = NextIDForEntity(material.material) };
  }

  public static int GetEntityBufferID(SpriteSheetMaterial material) {
    return materialEntityBufferID[material.material];
  }

  public static Entity GetEntityBuffer(Material material) {
    return bufferEntities[materialEntityBufferID[material]];
  }

  public static int NextIDForEntity(Material material) {
    var ids = availableEntityID[material];
    var availableIds = Enumerable.Range(0, ids.Count + 1).Except(ids);
    int smallerID = availableIds.First();
    ids.Add(smallerID);
    return smallerID;
  }

  public static void RemoveBuffer(Material material, int bufferID) {
    Entity bufferEntity = GetEntityBuffer(material);
    availableEntityID[material].Remove(bufferID);
    AddDynamicBuffers(bufferEntity, material);
  }

  public static DynamicBuffer<SpriteIndexBuffer>[] GetIndexBuffers() {
    DynamicBuffer<SpriteIndexBuffer>[] buffers = new DynamicBuffer<SpriteIndexBuffer>[bufferEntities.Count];
    for(int i = 0; i < buffers.Length; i++)
      buffers[i] = manager.GetBuffer<SpriteIndexBuffer>(bufferEntities[i]);
    return buffers;
  }
  public static DynamicBuffer<MatrixBuffer>[] GetMatrixBuffers() {
    DynamicBuffer<MatrixBuffer>[] buffers = new DynamicBuffer<MatrixBuffer>[bufferEntities.Count];
    for(int i = 0; i < buffers.Length; i++)
      buffers[i] = manager.GetBuffer<MatrixBuffer>(bufferEntities[i]);
    return buffers;
  }
  public static DynamicBuffer<SpriteColorBuffer>[] GetColorBuffers() {
    DynamicBuffer<SpriteColorBuffer>[] buffers = new DynamicBuffer<SpriteColorBuffer>[bufferEntities.Count];
    for(int i = 0; i < buffers.Length; i++)
      buffers[i] = manager.GetBuffer<SpriteColorBuffer>(bufferEntities[i]);
    return buffers;
  }
  public static DynamicBuffer<UvBuffer>[] GetUvBuffers() {
    DynamicBuffer<UvBuffer>[] buffers = new DynamicBuffer<UvBuffer>[bufferEntities.Count];
    for(int i = 0; i < buffers.Length; i++)
      buffers[i] = manager.GetBuffer<UvBuffer>(bufferEntities[i]);
    return buffers;
  }

}
