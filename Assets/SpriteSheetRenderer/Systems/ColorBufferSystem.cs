using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class ColorBufferSystem : SystemBase {
  EntityQuery query;
  protected override void OnCreate(){
    query = GetEntityQuery(ComponentType.ReadOnly<SpriteSheetColor>(), ComponentType.ReadOnly<BufferHook>());
    query.SetChangedVersionFilter(typeof(SpriteSheetColor));
    query.AddDependency(Dependency);
  }
  protected override void OnUpdate(){
    NativeArray<SpriteSheetColor> spriteSheetColors = query.ToComponentDataArrayAsync<SpriteSheetColor>(Allocator.TempJob, out JobHandle colorsHandle);
    
    if(spriteSheetColors.Length > 0) {
      NativeArray<BufferHook> bufferHooks = query.ToComponentDataArrayAsync<BufferHook>(Allocator.TempJob, out JobHandle hooksHandle);
      Dependency = JobHandle.CombineDependencies(Dependency, colorsHandle, hooksHandle);

      var jDep = Entities.WithName("ColorBufferSystem").ForEach(
        (ref DynamicBuffer<SpriteColorBuffer> spriteColorBuffers) => {
          for(int i = 0; i < bufferHooks.Length; i++)
            spriteColorBuffers[bufferHooks[i].bufferID] = spriteSheetColors[i].color;
        }
      )
      .WithDisposeOnCompletion(bufferHooks)
      .WithDisposeOnCompletion(spriteSheetColors)
      .WithNativeDisableParallelForRestriction(spriteSheetColors)
      .WithNativeDisableParallelForRestriction(bufferHooks)
      .ScheduleParallel(Dependency);
      Dependency = JobHandle.CombineDependencies(Dependency, jDep);
    }
  }

  protected override void OnStopRunning(){
    base.OnStopRunning();
    query.CompleteDependency();
    Dependency.Complete();
  }

}
