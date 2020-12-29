using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class MatrixBufferSystem : JobComponentSystem
{
    [BurstCompile]
    struct UpdateJob : IJobForEach<SpriteMatrix, BufferHook>
    {
        [NativeDisableParallelForRestriction]
        public DynamicBuffer<MatrixBuffer> indexBuffer;
        [ReadOnly]
        public int bufferEntityID;
        public void Execute([ReadOnly, ChangedFilter] ref SpriteMatrix data, [ReadOnly] ref BufferHook hook)
        {
            if (bufferEntityID == hook.bufferEntityID)
                indexBuffer[hook.bufferID] = data.matrix;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var buffers = DynamicBufferManager.GetMatrixBuffers();
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