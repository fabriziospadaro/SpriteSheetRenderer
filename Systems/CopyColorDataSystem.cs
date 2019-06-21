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
  struct CopyColors : IJobForEachWithEntity<SpriteSheetColor> {
    public Entity bufferEntity;
    public BufferFromEntity<ColorBuffer> bufferFromEntity;

    public void Execute(Entity e, int index, [ReadOnly] ref SpriteSheetColor c0) {
      var buffer = bufferFromEntity[bufferEntity];
      buffer[index] = c0.value;
    }
  }

  protected override void OnCreate() {
    base.OnCreate();
    sourceColors = GetEntityQuery(ComponentType.ReadOnly<SpriteSheetColor>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
  }

  protected override JobHandle PopulateBuffer(Entity bufferEntity, SpriteSheetMaterial filterMat, JobHandle inputDeps) {

    sourceColors.SetFilter(filterMat);

    EntityManager.GetBuffer<ColorBuffer>(bufferEntity).ResizeUninitialized(sourceColors.CalculateLength());
    inputDeps = new CopyColors {
      bufferEntity = bufferEntity,
      bufferFromEntity = GetBufferFromEntity<ColorBuffer>()
    }.ScheduleSingle(sourceColors, inputDeps);

    return inputDeps;
  }

}
