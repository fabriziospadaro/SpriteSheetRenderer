using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

// Started to split out TRS data to separate buffers. Not used yet.
[DisableAutoCreation]
public class CopyPosDataSystem : RenderBufferSystem<PosBuffer> {
  EntityQuery positions;

  [BurstCompile]
  struct CopyPosData : IJob {
    public Entity bufferEntity;
    public BufferFromEntity<PosBuffer> bufferFromEntity;

    [ReadOnly,DeallocateOnJobCompletion]
    public NativeArray<Position2D> positions;
    public void Execute() {
      var buffer = bufferFromEntity[bufferEntity];
      for (int i = 0; i < positions.Length; i++) 
        buffer[i] = positions[i].Value;
    }
  }

  struct CopyPosForEach : IJobForEachWithEntity<Position2D> {

    public Entity bufferEntity;
    public BufferFromEntity<PosBuffer> bufferFromEntity;

    public void Execute(Entity e, int i, [ReadOnly] ref Position2D c0) {
      var buffer = bufferFromEntity[bufferEntity];
      buffer[i] = c0.Value;
    }
  }

  protected override void OnCreate() {
    base.OnCreate();
    positions = GetEntityQuery(ComponentType.ReadOnly<Position2D>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
  }

  protected override JobHandle PopulateBuffer(Entity bufferEntity, SpriteSheetMaterial filterMat, JobHandle inputDeps) {
    positions.SetFilter(filterMat);

    EntityManager.GetBuffer<PosBuffer>(bufferEntity).ResizeUninitialized(positions.CalculateLength());
    var copyJob = new CopyPosForEach {
      bufferEntity = bufferEntity,
      bufferFromEntity = GetBufferFromEntity<PosBuffer>(false),
    }.ScheduleSingle(this, inputDeps);
    
    return JobHandle.CombineDependencies(copyJob, inputDeps);
  }

}
