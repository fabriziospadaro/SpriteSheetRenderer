using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class ColorBufferSystem : SystemBase {
  EntityQuery query;

  protected override void OnCreate(){
    query = GetEntityQuery(ComponentType.ReadOnly<SpriteSheetColor>(), ComponentType.ReadOnly<BufferHook>());
    query.SetChangedVersionFilter(typeof(SpriteSheetColor));
  }

  protected override void OnUpdate(){
    NativeArray<SpriteSheetColor> spriteSheetColors = query.ToComponentDataArrayAsync<SpriteSheetColor>(Allocator.TempJob, out JobHandle colorsHandle);
    Dependency = JobHandle.CombineDependencies(Dependency, colorsHandle);

    if(spriteSheetColors.Length > 0) {
      NativeArray<BufferHook> bufferHooks = query.ToComponentDataArrayAsync<BufferHook>(Allocator.TempJob, out JobHandle hooksHandle);
      Dependency = JobHandle.CombineDependencies(Dependency, hooksHandle);

      Dependency = JobHandle.CombineDependencies(Dependency,Entities.WithName("ColorBufferSystem").WithReadOnly(spriteSheetColors).WithReadOnly(bufferHooks).ForEach(
        (ref DynamicBuffer<SpriteColorBuffer> spriteColorBuffers, in EntityIDComponent entityID) => {
          for(int i = 0; i < bufferHooks.Length; i++)
            if(bufferHooks[i].bufferEnityID == entityID.id)
              spriteColorBuffers[bufferHooks[i].bufferID] = spriteSheetColors[i].color;
        }
      )
      .WithDisposeOnCompletion(bufferHooks)
      .WithDisposeOnCompletion(spriteSheetColors)
      .ScheduleParallel(Dependency));
    }
    else
      spriteSheetColors.Dispose();
  }

}
