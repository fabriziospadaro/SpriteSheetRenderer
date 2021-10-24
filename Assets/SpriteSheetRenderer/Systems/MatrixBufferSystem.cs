using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class MatrixBufferSystem : SystemBase {
  EntityQuery query;
  protected override void OnCreate(){
    query = GetEntityQuery(ComponentType.ReadOnly<SpriteMatrix>(), ComponentType.ReadOnly<BufferHook>());
    query.SetChangedVersionFilter(typeof(SpriteMatrix));
    query.AddDependency(Dependency);
  }
  protected override void OnUpdate(){
    NativeArray<SpriteMatrix> spriteMatrices = query.ToComponentDataArrayAsync<SpriteMatrix>(Allocator.TempJob, out JobHandle matricesHandle);
    
    if(spriteMatrices.Length > 0) {
      NativeArray<BufferHook> bufferHooks = query.ToComponentDataArrayAsync<BufferHook>(Allocator.TempJob, out JobHandle hooksHandle);
      query.AddDependency(Dependency);

      var jDep = Entities.WithName("MatrixBufferSystem").ForEach(
        (ref DynamicBuffer<MatrixBuffer> matrixBuffers) => {
          for(int i = 0; i < bufferHooks.Length; i++)
            matrixBuffers[bufferHooks[i].bufferID] = spriteMatrices[i].matrix;
        }
      )
      .WithDisposeOnCompletion(bufferHooks)
      .WithDisposeOnCompletion(spriteMatrices)
      .WithNativeDisableParallelForRestriction(spriteMatrices)
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