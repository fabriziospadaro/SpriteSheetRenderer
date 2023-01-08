using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public partial class UVBufferSystem : SystemBase {
  EntityQuery query;

  protected override void OnCreate(){
    query = GetEntityQuery(ComponentType.ReadOnly<SpriteIndex>(), ComponentType.ReadOnly<BufferHook>());
    query.SetChangedVersionFilter(typeof(SpriteIndex));
  }

  protected override void OnUpdate(){
    NativeArray<SpriteIndex> spriteIndices = query.ToComponentDataArrayAsync<SpriteIndex>(Allocator.TempJob, out JobHandle indicesHandle);
    Dependency = JobHandle.CombineDependencies(Dependency, indicesHandle);

    if(spriteIndices.Length > 0) {
      NativeArray<BufferHook> bufferHooks = query.ToComponentDataArrayAsync<BufferHook>(Allocator.TempJob, out JobHandle hooksHandle);
      Dependency = JobHandle.CombineDependencies(Dependency, hooksHandle);

      Dependency = JobHandle.CombineDependencies(Dependency,Entities.WithName("UVBufferSystem").WithReadOnly(spriteIndices).WithReadOnly(bufferHooks).ForEach(
        (ref DynamicBuffer<SpriteIndexBuffer> spriteIndexBuffers, in EntityIDComponent entityID) => {
          for(int i = 0; i < bufferHooks.Length; i++)
            if(bufferHooks[i].bufferEnityID == entityID.id)
              spriteIndexBuffers[bufferHooks[i].bufferID] = spriteIndices[i].Value;
        }
      )
      .WithDisposeOnCompletion(bufferHooks)
      .WithDisposeOnCompletion(spriteIndices)
      .ScheduleParallel(Dependency));
    }
    else
      spriteIndices.Dispose();
  }

}
