using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class ColorBufferSystem : ComponentSystem {
  EntityQuery colorQuery;
  private readonly ComponentType cmpTypeA = ComponentType.ReadOnly<SpriteSheetColor>();
  //todo menage multiple materials
  protected override void OnCreate() {
    colorQuery = GetEntityQuery(cmpTypeA);
  }
  //todo menage destruction of buffers
  [BurstCompile]
  struct UpdateJob : IJob {
    [NativeDisableParallelForRestriction]
    public DynamicBuffer<SpriteColorBuffer> colorBuffer;
    [DeallocateOnJobCompletion]
    [ReadOnly]
    public NativeArray<SpriteSheetColor> colors;
    public void Execute() {
      for(int i = 0; i < colors.Length; i++)
        colorBuffer[colors[i].bufferID] = colors[i].color;
    }
  }

  protected override void OnUpdate() {
    colorQuery.SetFilterChanged(cmpTypeA);
    NativeArray<SpriteSheetColor> colors = colorQuery.ToComponentDataArray<SpriteSheetColor>(Allocator.TempJob);
    var job = new UpdateJob() {
      colors = colors,
      colorBuffer = DynamicBufferManager.GetColorBuffer(),
    };
    job.Schedule().Complete();
  }
}
