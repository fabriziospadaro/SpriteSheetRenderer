using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class SpriteSheetUvSystem : ComponentSystem {
  EntityQuery indexQuery;
  private readonly ComponentType cmpTypeB = ComponentType.ReadOnly<SpriteIndex>();
  //todo menage multiple materials
  protected override void OnCreate() {
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
      for(int i = 0; i < data.Length; i++)
        indexBuffer[data[i].bufferID] = data[i].Value;
    }
  }

  protected override void OnUpdate() {
    indexQuery.SetFilterChanged(cmpTypeB);
    NativeArray<SpriteIndex> data = indexQuery.ToComponentDataArray<SpriteIndex>(Allocator.TempJob);
    var job = new UpdateJob() {
      indexBuffer = DynamicBufferManager.GetIndexBuffer(),
      data = data
    };
    job.Schedule().Complete();
  }
}
