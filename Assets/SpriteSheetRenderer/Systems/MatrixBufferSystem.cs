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
    private struct UpdateJobChunk : IJobChunk
    {
        [NativeDisableParallelForRestriction]
        public DynamicBuffer<MatrixBuffer> indexBuffer;
        [ReadOnly]
        public int bufferEntityID;

        [ReadOnly]
        public ComponentTypeHandle<SpriteMatrix> data;
        [ReadOnly]
        public ComponentTypeHandle<BufferHook> hook;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkSpriteMatrix = chunk.GetNativeArray(data);
            var chunkBufferHooks = chunk.GetNativeArray(hook);

            for (int i = 0; i < chunk.Count; i++)
            {
                if (bufferEntityID == chunkBufferHooks[i].bufferEntityID)
                {
                    indexBuffer[chunkBufferHooks[i].bufferID] = chunkSpriteMatrix[i].matrix;
                }
            }
        }
    }

    private EntityQuery m_EntityQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_EntityQuery = GetEntityQuery(
            ComponentType.ReadOnly<SpriteMatrix>(),
            ComponentType.ReadOnly<BufferHook>());
        m_EntityQuery.SetChangedVersionFilter(ComponentType.ReadOnly<SpriteMatrix>());

    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var buffers = DynamicBufferManager.GetMatrixBuffers();
        NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(buffers.Length, Allocator.Temp);
        for (int i = 0; i < buffers.Length; i++)
        {
            inputDeps = new UpdateJobChunk()
            {
                indexBuffer = buffers[i],
                bufferEntityID = i,
                data = GetComponentTypeHandle<SpriteMatrix>(isReadOnly: true),
                hook = GetComponentTypeHandle<BufferHook>(isReadOnly: true)
            }.Schedule(m_EntityQuery, inputDeps);
            jobs[i] = inputDeps;
        }
        JobHandle.CompleteAll(jobs);
        jobs.Dispose();
        return inputDeps;
    }
}
