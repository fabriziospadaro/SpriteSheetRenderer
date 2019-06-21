using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

/// <summary>
/// Copy uv cell data to the render buffer.
/// </summary>
public class CopyUVCellDataSystem : RenderBufferSystem<UVCellBuffer> {
  EntityQuery sourceQuery;

  //[BurstCompile]
  //struct CopyDataToRenderBuffer : IJob {
  //  public Entity bufferEntity;
  //  public BufferFromEntity<UVCellBuffer> bufferFromEntity;

  //  [ReadOnly,DeallocateOnJobCompletion]
  //  public NativeArray<UVCell> data;
  //  public void Execute() {
  //    var buffer = bufferFromEntity[bufferEntity];
  //    buffer.ResizeUninitialized(data.Length);
  //    for (int i = 0; i < data.Length; i++) 
  //      buffer[i] = data[i].value;
  //  }
  //}

  [BurstCompile]
  struct CopyData : IJobForEachWithEntity<UVCell> {
    public Entity bufferEntity;
    public BufferFromEntity<UVCellBuffer> bufferFromEntity;

    public void Execute(Entity e, int index, [ReadOnly] ref UVCell c) {
      var buffer = bufferFromEntity[bufferEntity];
      buffer[index] = c.value;
    }
  }

  protected override void OnCreate() {
    base.OnCreate();
    sourceQuery = GetEntityQuery(ComponentType.ReadOnly<UVCell>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
  }

  protected override JobHandle PopulateBuffer(Entity bufferEntity, SpriteSheetMaterial filterMat, JobHandle inputDeps) {
    sourceQuery.SetFilter(filterMat);

    EntityManager.GetBuffer<UVCellBuffer>(bufferEntity).ResizeUninitialized(sourceQuery.CalculateLength());
    inputDeps = new CopyData {
      bufferEntity = bufferEntity,
      bufferFromEntity = GetBufferFromEntity<UVCellBuffer>(false),
    }.ScheduleSingle(sourceQuery, inputDeps);

    return inputDeps;
  }

}
