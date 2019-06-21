using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class CopyColorDataSystem : BufferDataSystem<ColorBuffer>
{
  EntityQuery sourceColors;

  [BurstCompile]
  struct CopyColorData : IJob {
    [ReadOnly]
    public Entity bufferEntity;

    public BufferFromEntity<ColorBuffer> bufferFromEntity;

    [ReadOnly, DeallocateOnJobCompletion]
    public NativeArray<SpriteSheetColor> colors;

    public void Execute() {
      var buffer = bufferFromEntity[bufferEntity];
      for (int i = 0; i < colors.Length; i++)
        buffer[i] = colors[i].value;
    }
  }

  protected override void OnCreate() {
    base.OnCreate();
    sourceColors = GetEntityQuery(ComponentType.ReadOnly<SpriteSheetColor>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
  }

  protected override JobHandle PopulateBuffer(Entity bufferEntity, SpriteSheetMaterial filterMat, JobHandle inputDeps) {

    sourceColors.SetFilter(filterMat);
    int colorsCount = sourceColors.CalculateLength();

    var buffer = EntityManager.GetBuffer<ColorBuffer>(bufferEntity);
    if (buffer.Length != colorsCount)
      buffer.ResizeUninitialized(colorsCount);

    var colors = sourceColors.ToComponentDataArray<SpriteSheetColor>(Allocator.TempJob);

    var copyJob = new CopyColorData {
      bufferEntity = bufferEntity,
      bufferFromEntity = GetBufferFromEntity<ColorBuffer>(false),
      colors = colors
    }.Schedule();
    
    return JobHandle.CombineDependencies(inputDeps, copyJob);
  }

}
