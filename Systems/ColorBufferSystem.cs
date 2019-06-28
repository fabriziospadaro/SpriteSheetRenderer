using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class ColorBufferSystem : JobComponentSystem {
  //todo menage multiple materials
  //todo menage destruction of buffers
  [BurstCompile]
  struct UpdateJob : IJobForEach<SpriteSheetColor, BufferHook> {
    [NativeDisableParallelForRestriction]
    public DynamicBuffer<SpriteColorBuffer> indexBuffer;
    [ReadOnly]
    public int bufferEnityID;
    public void Execute([ReadOnly, ChangedFilter] ref SpriteSheetColor data, [ReadOnly, ChangedFilter] ref BufferHook hook) {
      if(bufferEnityID == hook.bufferEnityID)
        indexBuffer[hook.bufferID] = data.color;
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    var buffers = DynamicBufferManager.GetColorBuffer();
    NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(buffers.Length, Allocator.TempJob);
    for(int i = 0; i < buffers.Length; i++) {
      inputDeps = new UpdateJob() {
        indexBuffer = buffers[i],
        bufferEnityID = i
      }.Schedule(this, inputDeps);
      jobs[i] = inputDeps;
    }
    JobHandle.CompleteAll(jobs);
    jobs.Dispose();
    return inputDeps;
  }
}
