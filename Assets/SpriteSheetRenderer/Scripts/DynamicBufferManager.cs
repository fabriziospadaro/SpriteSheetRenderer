using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using System;
using System.Linq;

//todo entity is a dictionary with spritesheetmaterial and is used to separate buffers from different material

public static class DynamicBufferManager {

  public static EntityManager EntityManager {get { return SpriteSheetManager.EntityManager;}}
  
  //list of all the "Enities with all the buffers"
  //Each different material have a different bufferEnity
  private static List<Entity> bufferEntities = new List<Entity>();
  //contains the index of a bufferEntity inside the bufferEntities from a material
  private static Dictionary<Material, int> materialEntityBufferID = new Dictionary<Material, int>();

  //only use this when you didn't bake the uv yet
  public static void BakeUvBuffer(SpriteSheetMaterial spriteSheetMaterial, KeyValuePair<Material, float4[]> atlasData) {
    Entity entity = GetEntityBuffer(spriteSheetMaterial.material);
    var buffer = EntityManager.GetBuffer<UvBuffer>(entity);
    for(int j = 0; j < atlasData.Value.Length; j++)
      buffer.Add(atlasData.Value[j]);
  }

  public static void GenerateBuffers(SpriteSheetMaterial material, int entityCount = 0) {
    if(!materialEntityBufferID.ContainsKey(material.material)) {
      CreateBuffersContainer(material);
      MassAddBuffers(bufferEntities.Last(), entityCount);
    }
  }

  //use this when it's the first time you are using that material
  //use this just to generate the buffers container
  public static void CreateBuffersContainer(SpriteSheetMaterial material) {
    var archetype = EntityManager.CreateArchetype(
      typeof(SpriteIndexBuffer),
      typeof(MatrixBuffer),
      typeof(SpriteColorBuffer),
      typeof(SpriteSheetMaterial),
      typeof(UvBuffer),
      typeof(IdsBuffer),
      typeof(EntityIDComponent)
    );
    Entity e = EntityManager.CreateEntity(archetype);
    bufferEntities.Add(e);
    EntityManager.SetSharedComponentData(e, material);
    EntityManager.SetComponentData(e, new EntityIDComponent { id = materialEntityBufferID.Count });
    EntityManager.SetName(e, $"EntityBuffer[{materialEntityBufferID.Count}]");
    materialEntityBufferID.Add(material.material, materialEntityBufferID.Count);
  }

  //when u create a new entity you need a new buffer for him
  //use this to add new dynamicbuffer
  public static void MassAddBuffers(Entity bufferEntity, int entityCount) {
    var indexBuffer = EntityManager.GetBuffer<SpriteIndexBuffer>(bufferEntity);
    var colorBuffer = EntityManager.GetBuffer<SpriteColorBuffer>(bufferEntity);
    var matrixBuffer = EntityManager.GetBuffer<MatrixBuffer>(bufferEntity);
    var idsBuffer = EntityManager.GetBuffer<IdsBuffer>(bufferEntity);
    for(int i = 0; i < entityCount; i++) {
      indexBuffer.Add(new SpriteIndexBuffer());
      matrixBuffer.Add(new MatrixBuffer());
      colorBuffer.Add(new SpriteColorBuffer());
      idsBuffer.Add(new IdsBuffer {  value = i});
    }
  }

  public static int AddDynamicBuffers(Entity bufferEntity) {
    var la = EntityManager.GetBuffer<IdsBuffer>(bufferEntity).Reinterpret<int>();
    int bufferId = 0;
    if(la.Length > 0)
      bufferId = NextID(la);
    
    var indexBuffer = EntityManager.GetBuffer<SpriteIndexBuffer>(bufferEntity);

    var colorBuffer = EntityManager.GetBuffer<SpriteColorBuffer>(bufferEntity);
    var matrixBuffer = EntityManager.GetBuffer<MatrixBuffer>(bufferEntity);
    var idsBuffer = EntityManager.GetBuffer<IdsBuffer>(bufferEntity);
    indexBuffer.Add(new SpriteIndexBuffer());
    colorBuffer.Add(new SpriteColorBuffer());
    matrixBuffer.Add(new MatrixBuffer());
    idsBuffer.Add(new IdsBuffer { value = bufferId });
    return bufferId;
  }

  public static int GetEntityBufferID(SpriteSheetMaterial material) {
    return materialEntityBufferID[material.material];
  }

  public static Entity GetEntityBuffer(Material material) {
    return bufferEntities[materialEntityBufferID[material]];
  }

  static int NextID(DynamicBuffer<int> A){
    // Our original array
    int maxVal = int.MinValue;
    int i;
    for(i = 0; i < A.Length; i++)
      if(A[i] > maxVal)
        maxVal = A[i];

    int m = maxVal+1; // Storing maximum value

    // In case all values in our array are negative
    if(m < 1) {
      return 1;
    }
    if(A.Length == 1) {

      // If it contains only one element
      if(A[0] == 1) {
        return 2;
      }
      else {
        return 1;
      }
    }
    i = 0;
    int[] l = new int[m];
    for(i = 0; i < A.Length; i++) {
      if(A[i] > 0) {
        // Changing the value status at the index of
        // our list
        if(l[A[i] - 1] != 1) {
          l[A[i] - 1] = 1;
        }
      }
    }

    // Encountering first 0, i.e, the element with least
    // value
    for(i = 0; i < l.Length; i++) {
      if(l[i] == 0) {
        return i + 1;
      }
    }

    // In case all values are filled between 1 and m
    return i + 2;
  }

  static int findFirstMissing(DynamicBuffer<int> array,int start, int end){
    if(start > end)
      return end + 1;

    if(start != array[start])
      return start;

    int mid = (start + end) / 2;

    if(array[mid] == mid)
      return findFirstMissing(array, mid + 1, end);

    return findFirstMissing(array, start, mid);
  }

  public static Material GetMaterial(int bufferEntityID) {
    foreach(KeyValuePair<Material, int> e in materialEntityBufferID)
      if(e.Value == bufferEntityID)
        return e.Key;
    return null;
  }

}
