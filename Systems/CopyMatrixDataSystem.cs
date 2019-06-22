using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Copy TRS data to the render buffer.
/// </summary>
public class CopyMatrixDataSystem : RenderBufferSystem<MatrixBuffer>
{
  EntityQuery trsQuery;
  
  
  [BurstCompile]
  struct CopyMatrixDataJob : IJobForEachWithEntity<Position2D,Rotation2D,Scale> {
    [ReadOnly]
    public Entity bufferEntity;
    public BufferFromEntity<MatrixBuffer> bufferFromEntity;
    
    public void Execute(Entity e, int i, 
      [ReadOnly] ref Position2D t, [ReadOnly] ref Rotation2D r, [ReadOnly] ref Scale s) {
        var buffer = bufferFromEntity[bufferEntity];
        var matrix = buffer[i];
        matrix.value.c0.xy = t.Value;
        matrix.value.c0.z = r.angle;
        matrix.value.c0.w = s.Value;
        buffer[i] = matrix;
    }
  }

  protected override void OnCreate() {
    base.OnCreate();
    trsQuery = GetEntityQuery(
      ComponentType.ReadOnly<Position2D>(),
      ComponentType.ReadOnly<Rotation2D>(),
      ComponentType.ReadOnly<Scale>(),
      ComponentType.ReadOnly<SpriteSheetMaterial>()
      );
  }

  protected override void OnDestroy() {
    base.OnDestroy();
  }

  protected override JobHandle PopulateBuffer(Entity bufferEntity, SpriteSheetMaterial filterMat, JobHandle inputDeps) {
    trsQuery.SetFilter(filterMat);

    EntityManager.GetBuffer<MatrixBuffer>(bufferEntity).ResizeUninitialized(trsQuery.CalculateLength());
    inputDeps = new CopyMatrixDataJob {
      bufferEntity = bufferEntity,
      bufferFromEntity = GetBufferFromEntity<MatrixBuffer>(false),
    }.ScheduleSingle(trsQuery, inputDeps);

    return inputDeps;
  }

  void Initialize<T>(out NativeArray<T> arr, int len, T val) where T : struct, IComponentData {
    arr = new NativeArray<T>(len, Allocator.TempJob);
    for (int i = 0; i < arr.Length; ++i)
      arr[i] = val;
  }
}
