using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class ColorBufferSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var buffers = DynamicBufferManager.GetColorBuffers();

        for (int bufferID = 0; bufferID < buffers.Length; bufferID++)
        {
            DynamicBuffer<SpriteColorBuffer> buffer = buffers[bufferID];
            Dependency = Entities
                .WithBurst()
                .WithNativeDisableContainerSafetyRestriction(buffer)
                .ForEach((in SpriteSheetColor spriteSheetColor, in BufferHook bufferHook) =>
                {
                    if (bufferID == bufferHook.bufferEntityID)
                    {
                        buffer[bufferHook.bufferID] = spriteSheetColor.color;
                    }
                })
                .ScheduleParallel(Dependency);
        }
    }
}
