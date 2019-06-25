using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class MatrixBufferSystem : ComponentSystem {
  Entity colorBufferEntity;
  EntityQuery matrixQuery;
  private readonly ComponentType cmpTypeB = ComponentType.ReadOnly<RenderData>();
  //todo menage multiple materials
  protected override void OnCreate() {
    colorBufferEntity = EntityManager.CreateEntity(typeof(MatrixBuffer));
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
      int bufferSize = matrixBuffer.Length;
      for(int i = 0; i < data.Length; i++) {
        if(i >= bufferSize)
          matrixBuffer.Add(data[i].matrix);
        else
          matrixBuffer[i] = data[i].matrix;
      }
    }
  }

  protected override void OnUpdate() {
    matrixQuery.SetFilterChanged(cmpTypeB);
    DynamicBuffer<MatrixBuffer> matrixbuffer = EntityManager.GetBuffer<MatrixBuffer>(colorBufferEntity);
    NativeArray<RenderData> data = matrixQuery.ToComponentDataArray<RenderData>(Allocator.TempJob);
    var job = new UpdateJob() {
      matrixBuffer = matrixbuffer,
      data = data
    };
    job.Schedule().Complete();
  }
}
