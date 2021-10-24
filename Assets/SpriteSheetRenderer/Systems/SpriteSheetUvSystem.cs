using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class SpriteSheetUvJobSystem : SystemBase {
  EntityQuery query;
  protected override void OnCreate(){
    query = GetEntityQuery(ComponentType.ReadOnly<SpriteIndex>(), ComponentType.ReadOnly<BufferHook>());
    query.SetChangedVersionFilter(typeof(SpriteIndex));
    query.AddDependency(Dependency);
  }
  protected override void OnUpdate(){
    NativeArray<SpriteIndex> spriteIndices = query.ToComponentDataArrayAsync<SpriteIndex>(Allocator.TempJob, out JobHandle indicesHandle);

    if(spriteIndices.Length > 0) {
      NativeArray<BufferHook> bufferHooks = query.ToComponentDataArrayAsync<BufferHook>(Allocator.TempJob, out JobHandle hooksHandle);
      Dependency = JobHandle.CombineDependencies(Dependency, indicesHandle,hooksHandle);

      var jDep = Entities.WithName("SpriteSheetUvJobSystem").ForEach(
        (ref DynamicBuffer<SpriteIndexBuffer> spriteIndexBuffers) => {
          for(int i = 0; i < bufferHooks.Length; i++)
            spriteIndexBuffers[bufferHooks[i].bufferID] = spriteIndices[i].Value;
        }
      )
      .WithDisposeOnCompletion(bufferHooks)
      .WithDisposeOnCompletion(spriteIndices)
      .WithNativeDisableParallelForRestriction(spriteIndices)
      .WithNativeDisableContainerSafetyRestriction(bufferHooks)
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
