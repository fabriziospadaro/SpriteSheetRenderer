using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class MatrixBufferSystem : SystemBase {
  EntityQuery query;

  protected override void OnCreate(){
    query = GetEntityQuery(ComponentType.ReadOnly<SpriteMatrix>(), ComponentType.ReadOnly<BufferHook>());
    query.SetChangedVersionFilter(typeof(SpriteMatrix));
  }

  protected override void OnUpdate(){
    NativeArray<SpriteMatrix> spriteMatrices = query.ToComponentDataArrayAsync<SpriteMatrix>(Allocator.TempJob, out JobHandle matricesHandle);
    Dependency = JobHandle.CombineDependencies(Dependency, matricesHandle);

    if(spriteMatrices.Length > 0) {
      NativeArray<BufferHook> bufferHooks = query.ToComponentDataArrayAsync<BufferHook>(Allocator.TempJob, out JobHandle hooksHandle);
      Dependency = JobHandle.CombineDependencies(Dependency, hooksHandle);

      Dependency = JobHandle.CombineDependencies(Dependency, Entities.WithName("MatrixBufferSystem").WithReadOnly(spriteMatrices).WithReadOnly(bufferHooks).ForEach(
        (ref DynamicBuffer<MatrixBuffer> matrixBuffers, in DynamicBuffer<IdsBuffer> idsBuffer, in EntityIDComponent entityID) => {
          for(int i = 0; i < bufferHooks.Length; i++) {
            if(bufferHooks[i].bufferEntityID == entityID.id) {
              int id = 0;
              for(int j = 0; j < idsBuffer.Length; j++) {
                if(idsBuffer[j] == bufferHooks[i].bufferID) {
                  id = j;
                  break;
                }
              }
              matrixBuffers[id] = spriteMatrices[i].matrix;
            }
          }
        }
      )
      .WithDisposeOnCompletion(bufferHooks)
      .WithDisposeOnCompletion(spriteMatrices)
      .ScheduleParallel(Dependency));
    }
    else
      spriteMatrices.Dispose();
  }
}