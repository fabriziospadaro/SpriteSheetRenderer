using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

[UpdateAfter(typeof(SpriteSheetAnimationSystem))]
public class RenderDataSystem : JobComponentSystem {
  [BurstCompile]
  struct ShaderDataJob : IJobForEach<Position2D, Scale, Rotation2D, RenderData, SpriteSheet> {
    public void Execute([ReadOnly]ref Position2D translation, [ReadOnly] ref Scale scale, [ReadOnly] ref Rotation2D rotation, ref RenderData renderData, [ReadOnly]ref SpriteSheet spriteSheet) {
      renderData.transform = new float4(translation.Value, rotation.angle, scale.Value);
      renderData.uv.x = 1f / spriteSheet.cell.x;
      renderData.uv.y = 1f / spriteSheet.cell.y;
      renderData.uv.z = spriteSheet.spriteIndex % spriteSheet.cell.x * renderData.uv.x;
      renderData.uv.w = 1 - renderData.uv.y - (spriteSheet.spriteIndex / spriteSheet.cell.x * renderData.uv.y);
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    var job = new ShaderDataJob() { };
    return job.Schedule(this, inputDeps);
  }
}
