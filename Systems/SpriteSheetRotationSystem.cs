using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

[DisableAutoCreation]
public class SpriteSheetRotationSystem : JobComponentSystem {
  [BurstCompile]
  struct SpriteSheetRotationJob : IJobForEach<Rotation2D, RenderData> {
    public void Execute([ReadOnly] ref Rotation2D rotation, ref RenderData renderData) {
      renderData.matrix.c0.z = rotation.angle;
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    var job = new SpriteSheetRotationJob() { };
    return job.Schedule(this, inputDeps);
  }
}
