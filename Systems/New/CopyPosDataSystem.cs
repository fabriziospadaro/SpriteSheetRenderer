using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[DisableAutoCreation]
public class CopyPosDataSystem : BufferDataSystem<PosBuffer> {
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

  protected override void OnCreate() {
    base.OnCreate();
    positions = GetEntityQuery(ComponentType.ReadOnly<Position2D>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
  }

  protected override JobHandle PopulateBuffer(Entity bufferEntity, SpriteSheetMaterial filterMat, JobHandle inputDeps) {
    positions.SetFilter(filterMat);
    var positionData = positions.ToComponentDataArray<Position2D>(Allocator.TempJob);

    var buffer = EntityManager.GetBuffer<PosBuffer>(bufferEntity);
    if (buffer.Length != positionData.Length)
      buffer.ResizeUninitialized(positionData.Length);

    var copyJob = new CopyPosData {
      bufferEntity = bufferEntity,
      bufferFromEntity = GetBufferFromEntity<PosBuffer>(false),
      positions = positionData,
    }.Schedule();
    
    return JobHandle.CombineDependencies(copyJob, inputDeps);
  }

}
