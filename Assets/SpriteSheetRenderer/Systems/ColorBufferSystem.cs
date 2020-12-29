using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class ColorBufferSystem : JobComponentSystem
{
    [BurstCompile]
    struct UpdateJob : IJobForEach<SpriteSheetColor, BufferHook>
    {
        [NativeDisableParallelForRestriction]
        public DynamicBuffer<SpriteColorBuffer> indexBuffer;
        [ReadOnly]
        public int bufferEntityID;
        public void Execute([ReadOnly, ChangedFilter] ref SpriteSheetColor data, [ReadOnly] ref BufferHook hook)
        {
            if (bufferEntityID == hook.bufferEntityID)
                indexBuffer[hook.bufferID] = data.color;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var buffers = DynamicBufferManager.GetColorBuffers();
        NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(buffers.Length, Allocator.TempJob);
        for (int i = 0; i < buffers.Length; i++)
        {
            inputDeps = new UpdateJob()
            {
                indexBuffer = buffers[i],
                bufferEntityID = i
            }.Schedule(this, inputDeps);
            jobs[i] = inputDeps;
        }
        JobHandle.CompleteAll(jobs);
        jobs.Dispose();
        return inputDeps;
    }
}
