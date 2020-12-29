using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class SpriteIndexBufferSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var buffers = DynamicBufferManager.GetIndexBuffers();

        for (int bufferID = 0; bufferID < buffers.Length; bufferID++)
        {
            DynamicBuffer<SpriteIndexBuffer> buffer = buffers[bufferID];
            Dependency = Entities
                .WithBurst()
                .WithNativeDisableContainerSafetyRestriction(buffer)
                .ForEach((in SpriteIndex spriteIndex, in BufferHook bufferHook) =>
                {
                    if (bufferID == bufferHook.bufferEntityID)
                    {
                        buffer[bufferHook.bufferID] = spriteIndex.Value;
                    }
                })
                .ScheduleParallel(Dependency);
        }
    }
}
