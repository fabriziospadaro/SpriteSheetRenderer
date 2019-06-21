using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class CopyUVCellDataSystem : BufferDataSystem<UVCellBuffer> {
  EntityQuery sourceQuery;

  [BurstCompile]
  struct CopyDataToRenderBuffer : IJob {
    public Entity bufferEntity;
    public BufferFromEntity<UVCellBuffer> bufferFromEntity;

    [ReadOnly,DeallocateOnJobCompletion]
    public NativeArray<UVCell> data;
    public void Execute() {
      var buffer = bufferFromEntity[bufferEntity];
      for (int i = 0; i < data.Length; i++) 
        buffer[i] = data[i].value;
    }
  }

  protected override void OnCreate() {
    base.OnCreate();
    sourceQuery = GetEntityQuery(ComponentType.ReadOnly<UVCell>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
  }

  protected override JobHandle PopulateBuffer(Entity bufferEntity, SpriteSheetMaterial filterMat, JobHandle inputDeps) {
    sourceQuery.SetFilter(filterMat);
    var sourceData = sourceQuery.ToComponentDataArray<UVCell>(Allocator.TempJob);

    var buffer = EntityManager.GetBuffer<UVCellBuffer>(bufferEntity);
    if (buffer.Length != sourceData.Length)
      buffer.ResizeUninitialized(sourceData.Length);

    var copyJob = new CopyDataToRenderBuffer {
      bufferEntity = bufferEntity,
      bufferFromEntity = GetBufferFromEntity<UVCellBuffer>(false),
      data = sourceData,
    }.Schedule();
    
    return JobHandle.CombineDependencies(copyJob, inputDeps);
  }

}
