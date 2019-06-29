using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class MatrixBufferSystem : JobComponentSystem {
  [BurstCompile]
  struct UpdateJob : IJobForEach<RenderData, BufferHook> {
    [NativeDisableParallelForRestriction]
    public DynamicBuffer<MatrixBuffer> indexBuffer;
    [ReadOnly]
    public int bufferEnityID;
    public void Execute([ReadOnly, ChangedFilter] ref RenderData data, [ReadOnly] ref BufferHook hook) {
      if(bufferEnityID == hook.bufferEnityID)
        indexBuffer[hook.bufferID] = data.matrix;
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    var buffers = DynamicBufferManager.GetMatrixBuffers();
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