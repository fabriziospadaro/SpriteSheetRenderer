using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

/// <summary>
/// Copy color data to the render buffer.
/// </summary>
public class CopyColorDataSystem : RenderBufferSystem<ColorBuffer>
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
      buffer.ResizeUninitialized(colors.Length);
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
    var colors = sourceColors.ToComponentDataArray < SpriteSheetColor>(Allocator.TempJob);

    var copy = new CopyColorData {
      bufferEntity = bufferEntity,
      bufferFromEntity = GetBufferFromEntity<ColorBuffer>(false),
      colors = colors
    }.Schedule();

    return JobHandle.CombineDependencies(inputDeps, copy);
  }

}
