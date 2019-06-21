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
  EntityQuery positions;
  EntityQuery scales;
  EntityQuery rotations;

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
  struct CopyMatrixDataJob : IJob {
    [ReadOnly]
    public Entity bufferEntity;
    public BufferFromEntity<MatrixBuffer> bufferFromEntity;

    [ReadOnly, DeallocateOnJobCompletion]
    public NativeArray<Position2D> positions;
    [ReadOnly, DeallocateOnJobCompletion]
    public NativeArray<Rotation2D> rotations;
    [ReadOnly, DeallocateOnJobCompletion]
    public NativeArray<Scale> scales;
    
    public void Execute() {
      var buffer = bufferFromEntity[bufferEntity];
      buffer.ResizeUninitialized(positions.Length);
      for (int i = 0; i < positions.Length; i++) {
        var matrix = buffer[i];
        matrix.value.c0.xy = positions[i].Value;
        matrix.value.c0.z = rotations[i].angle;
        matrix.value.c0.w = scales[i].Value;
        buffer[i] = matrix;
      }
    }
  }

  protected override void OnCreate() {
    base.OnCreate();
    positions = GetEntityQuery(ComponentType.ReadOnly<Position2D>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
    rotations = GetEntityQuery(ComponentType.ReadOnly<Rotation2D>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
    scales = GetEntityQuery(ComponentType.ReadOnly<Scale>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
    bounds = new NativeArray<float3>(2, Allocator.Persistent);
  }

  protected override void OnDestroy() {
    base.OnDestroy();
    bounds.Dispose();
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    positions.ResetFilter();
    scales.ResetFilter();

    calcBoundsJob = new CalcBounds {
      positions = positions.ToComponentDataArray<Position2D>(Allocator.TempJob),
      scales = scales.ToComponentDataArray<Scale>(Allocator.TempJob),
      bounds = bounds,
    }.Schedule();
    inputDeps = JobHandle.CombineDependencies(inputDeps, calcBoundsJob);

    return base.OnUpdate(inputDeps);
  }

  protected override JobHandle PopulateBuffer(Entity bufferEntity, SpriteSheetMaterial filterMat, JobHandle inputDeps) {
    NativeArray<Position2D> posData;
    NativeArray<Rotation2D> rotData;
    NativeArray<Scale> scaleData;

    positions.SetFilter(filterMat);
    int positionsCount = positions.CalculateLength();
    if (positionsCount == 0)
      return inputDeps;
    posData = positions.ToComponentDataArray<Position2D>(Allocator.TempJob);

    // Account for the possiblity that rotation or scale systems are disabled. If so, we set sensible
    // defaults so we still get visible sprites.
    rotations.SetFilter(filterMat);
    int rotCount = rotations.CalculateLength();
    if (rotCount != positionsCount)
      Initialize(out rotData, positionsCount, new Rotation2D { angle = 0 });
    else
      rotData = rotations.ToComponentDataArray<Rotation2D>(Allocator.TempJob);

    scales.SetFilter(filterMat);
    int scalesCount = scales.CalculateLength();
    if (scalesCount != positionsCount)
      Initialize(out scaleData, positionsCount, new Scale { Value = 1 });
    else
      scaleData = scales.ToComponentDataArray<Scale>(Allocator.TempJob);
    
    var copyJob = new CopyMatrixDataJob {
      bufferEntity = bufferEntity,
      bufferFromEntity = GetBufferFromEntity<MatrixBuffer>(false),
      positions = posData,
      rotations = rotData,
      scales = scaleData,
    }.Schedule();

    return JobHandle.CombineDependencies(inputDeps, copyJob);
  }

  void Initialize<T>(out NativeArray<T> arr, int len, T val) where T : struct, IComponentData {
    arr = new NativeArray<T>(len, Allocator.TempJob);
    for (int i = 0; i < arr.Length; ++i)
      arr[i] = val;
  }
}
