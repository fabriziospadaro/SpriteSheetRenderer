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
  EntityQuery boundsQuery;
  EntityQuery trsQuery;

  JobHandle calcBoundsJob;
  NativeArray<float3> bounds;
  public Bounds Bounds {
    get {
      if (!Enabled )
        return new Bounds(Vector3.zero, Vector3.one * 500);
      calcBoundsJob.Complete();
      float3 min = bounds[0];
      float3 max = bounds[1];
      float3 extents = (max - min) * .5f;
      return new Bounds(min + extents, extents * 2);
    }
  }


  [BurstCompile]
  struct CalcBounds : IJob {
    [ReadOnly, DeallocateOnJobCompletion]
    public NativeArray<Position2D> positions;
    [ReadOnly, DeallocateOnJobCompletion]
    public NativeArray<Scale> scales;
    // 0 = min, 1 = max
    public NativeArray<float3> bounds;

    public void Execute() {
      float2 min = new float2(float.MaxValue);
      float2 max = new float2(float.MinValue);
      // Assumes pivot is bottom left
      for (int i = 0; i < positions.Length; i++) {
        min = math.min(min, positions[i].Value);
        max = math.max(max, positions[i].Value + scales[i].Value);
      }
      bounds[0] = new float3(min, -10);
      bounds[1] = new float3(max, 10);
    }
  }


  // Note we can't use a parallel job here - Buffers don't support parallel writing.
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
    boundsQuery = GetEntityQuery(
      ComponentType.ReadOnly<Position2D>(), 
      ComponentType.ReadOnly<Scale>(), 
      ComponentType.ReadOnly<SpriteSheetMaterial>());

    trsQuery = GetEntityQuery(
      ComponentType.ReadOnly<Position2D>(),
      ComponentType.ReadOnly<Rotation2D>(),
      ComponentType.ReadOnly<Scale>(),
      ComponentType.ReadOnly<SpriteSheetMaterial>()
      );
    bounds = new NativeArray<float3>(2, Allocator.Persistent);
  }

  protected override void OnDestroy() {
    base.OnDestroy();
    bounds.Dispose();
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    calcBoundsJob = new CalcBounds {
      positions = boundsQuery.ToComponentDataArray<Position2D>(Allocator.TempJob),
      scales = boundsQuery.ToComponentDataArray<Scale>(Allocator.TempJob),
      bounds = bounds,
    }.Schedule();
    inputDeps = JobHandle.CombineDependencies(inputDeps, calcBoundsJob);

    return base.OnUpdate(inputDeps);
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
