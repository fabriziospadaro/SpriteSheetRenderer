using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;

public class SpriteSheetScaleSystem : JobComponentSystem {
  [BurstCompile]
  struct SpriteSheetScaleJob : IJobForEach<Scale, RenderData> {
    public void Execute([ReadOnly] ref Scale scale, ref RenderData renderData) {
      renderData.transform.w = scale.Value;
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    var job = new SpriteSheetScaleJob() { };
    return job.Schedule(this, inputDeps);
  }
}
