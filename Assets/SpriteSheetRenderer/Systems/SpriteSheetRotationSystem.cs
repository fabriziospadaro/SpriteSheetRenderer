using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

public class SpriteSheetRotationSystem : JobComponentSystem
{
    [BurstCompile]
    struct SpriteSheetRotationJob : IJobForEach<Rotation2D, SpriteMatrix>
    {
        public void Execute([ReadOnly] ref Rotation2D rotation, ref SpriteMatrix renderData)
        {
            renderData.matrix.z = rotation.angle;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new SpriteSheetRotationJob() { };
        return job.Schedule(this, inputDeps);
    }
}
