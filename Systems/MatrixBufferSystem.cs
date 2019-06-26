using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class MatrixBufferSystem : ComponentSystem {
  EntityQuery matrixQuery;
  private readonly ComponentType cmpTypeB = ComponentType.ReadOnly<RenderData>();
  //todo menage multiple materials
  protected override void OnCreate() {
    matrixQuery = GetEntityQuery(cmpTypeB);
  }
  //todo menage destruction of buffers
  [BurstCompile]
  struct UpdateJob : IJob {
    [NativeDisableParallelForRestriction]
    public DynamicBuffer<MatrixBuffer> matrixBuffer;
    [DeallocateOnJobCompletion]
    [ReadOnly]
    public NativeArray<RenderData> data;
    public void Execute() {
      for(int i = 0; i < data.Length; i++)
        matrixBuffer[data[i].bufferID] = data[i].matrix;
    }
  }

  protected override void OnUpdate() {
    matrixQuery.SetFilterChanged(cmpTypeB);
    NativeArray<RenderData> data = matrixQuery.ToComponentDataArray<RenderData>(Allocator.TempJob);
    var job = new UpdateJob() {
      matrixBuffer = DynamicBufferManager.GetMatrixBuffer(),
      data = data
    };
    job.Schedule().Complete();
  }
}
