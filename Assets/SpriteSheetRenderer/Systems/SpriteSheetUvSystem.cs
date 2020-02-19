using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class SpriteSheetUvJobSystem : JobComponentSystem {
  [BurstCompile]
  struct UpdateJob : IJobForEach<SpriteIndex, BufferHook> {
    [NativeDisableParallelForRestriction]
    public DynamicBuffer<SpriteIndexBuffer> indexBuffer;
    [ReadOnly]
    public int bufferEnityID;
    public void Execute([ReadOnly, ChangedFilter] ref SpriteIndex data, [ReadOnly] ref BufferHook hook) {
      if(bufferEnityID == hook.bufferEnityID)
        indexBuffer[hook.bufferID] = data.Value;
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    var buffers = DynamicBufferManager.GetIndexBuffers();
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
