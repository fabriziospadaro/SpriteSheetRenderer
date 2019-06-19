using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

[UpdateAfter(typeof(SpriteSheetAnimationSystem))]
public class RenderDataSystem : JobComponentSystem {
  [BurstCompile]
  struct ShaderDataJob : IJobForEachWithEntity<Position2D, Scale, Rotation2D, RenderData, SpriteSheet> {
    [ReadOnly] public BufferFromEntity<UvBuffer> lookup;
    public void Execute(Entity entity, int index, [ReadOnly]ref Position2D translation, [ReadOnly] ref Scale scale, [ReadOnly] ref Rotation2D rotation, ref RenderData renderData, [ReadOnly]ref SpriteSheet spriteSheet) {
      renderData.transform = new float4(translation.Value, rotation.angle, scale.Value);
      renderData.uv = lookup[entity][spriteSheet.spriteIndex].uv;
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    var lookup = GetBufferFromEntity<UvBuffer>();
    var job = new ShaderDataJob() { lookup = lookup };
    return job.Schedule(this, inputDeps);
  }
}
