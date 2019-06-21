using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

public class SpriteSheetPositionSystem : JobComponentSystem {
  [BurstCompile]
  struct SpriteSheetPositionJob : IJobForEach<Position2D, RenderData> {
    public void Execute([ReadOnly][ChangedFilter]ref Position2D translation, ref RenderData renderData) {
      renderData.matrix.c0.x = translation.Value.x;
      renderData.matrix.c0.y = translation.Value.y;
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    var job = new SpriteSheetPositionJob() { };
    return job.Schedule(this, inputDeps);
  }
}
