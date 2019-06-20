using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

public class SpriteSheetUvSystem : JobComponentSystem {
  [BurstCompile]
  struct SpriteSheetUvJob : IJobForEachWithEntity<RenderData, SpriteSheet> {
    [ReadOnly] public BufferFromEntity<UvBuffer> lookup;
    public void Execute(Entity entity, int index, ref RenderData renderData, [ReadOnly]ref SpriteSheet spriteSheet) {
      renderData.matrix.c1 = lookup[entity][spriteSheet.spriteIndex].uv;
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    var lookup = GetBufferFromEntity<UvBuffer>();
    var job = new SpriteSheetUvJob() { lookup = lookup };
    return job.Schedule(this, inputDeps);
  }
}
