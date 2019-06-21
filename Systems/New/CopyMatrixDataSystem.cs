using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class CopyMatrixDataSystem : BufferDataSystem<MatrixBuffer>
{
  EntityQuery positions;
  EntityQuery scales;
  EntityQuery rotations;

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
    
    var buffer = EntityManager.GetBuffer<MatrixBuffer>(bufferEntity);
    if (buffer.Length != positionsCount) {
      buffer.ResizeUninitialized(positionsCount);
      for (int i = 0; i < buffer.Length; ++i)
        buffer[i] = new MatrixBuffer 
        { value = new float4x2 () };
    }

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
