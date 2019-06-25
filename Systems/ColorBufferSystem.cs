using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class ColorBufferSystem : ComponentSystem {
  Entity colorBufferEntity;
  EntityQuery colorQuery;
  private readonly ComponentType cmpTypeA = ComponentType.ReadOnly<SpriteSheetColor>();
  //todo menage multiple materials
  protected override void OnCreate() {
    colorBufferEntity = EntityManager.CreateEntity(typeof(SpriteColorBuffer));
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
      int bufferSize = colorBuffer.Length;
      for(int i = 0; i < colors.Length; i++) {
        if(i >= bufferSize)
          colorBuffer.Add(colors[i].color);
        else
          colorBuffer[i] = colors[i].color;
      }
    }
  }

  protected override void OnUpdate() {
    colorQuery.SetFilterChanged(cmpTypeA);
    DynamicBuffer<SpriteColorBuffer> colorbuffer = EntityManager.GetBuffer<SpriteColorBuffer>(colorBufferEntity);
    NativeArray<SpriteSheetColor> colors = colorQuery.ToComponentDataArray<SpriteSheetColor>(Allocator.TempJob);
    var job = new UpdateJob() {
      colors = colors,
      colorBuffer = colorbuffer,
    };
    job.Schedule().Complete();
  }
}
