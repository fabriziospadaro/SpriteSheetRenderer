using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class SpriteSheetUvSystem : ComponentSystem {
  Entity indexBufferEntity;
  EntityQuery indexQuery;
  private readonly ComponentType cmpTypeB = ComponentType.ReadOnly<SpriteIndex>();
  //todo menage multiple materials
  protected override void OnCreate() {
    indexBufferEntity = EntityManager.CreateEntity(typeof(SpriteIndexBuffer));
    indexQuery = GetEntityQuery(cmpTypeB);
  }
  //todo menage destruction of buffers
  [BurstCompile]
  struct UpdateJob : IJob {
    [NativeDisableParallelForRestriction]
    public DynamicBuffer<SpriteIndexBuffer> indexBuffer;
    [DeallocateOnJobCompletion]
    [ReadOnly]
    public NativeArray<SpriteIndex> data;
    public void Execute() {
      int bufferSize = indexBuffer.Length;
      for(int i = 0; i < data.Length; i++) {
        if(i >= bufferSize)
          indexBuffer.Add(data[i].Value);
        else
          indexBuffer[i] = data[i].Value;
      }
    }
  }

  protected override void OnUpdate() {
    indexQuery.SetFilterChanged(cmpTypeB);
    DynamicBuffer<SpriteIndexBuffer> matrixbuffer = EntityManager.GetBuffer<SpriteIndexBuffer>(indexBufferEntity);
    NativeArray<SpriteIndex> data = indexQuery.ToComponentDataArray<SpriteIndex>(Allocator.TempJob);
    var job = new UpdateJob() {
      indexBuffer = matrixbuffer,
      data = data
    };
    job.Schedule().Complete();
  }
}
