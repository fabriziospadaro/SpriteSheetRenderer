using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

public class Bound2DSystem : JobComponentSystem
{
    [BurstCompile]
    struct Bound2DJob : IJobForEach<Position2D, Scale, Bound2D>
    {
        public void Execute([ReadOnly] ref Position2D position, [ReadOnly] ref Scale scale, ref Bound2D bound)
        {
            bound.scale = scale.Value;
            bound.position = position.Value;
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new Bound2DJob() { };
        return job.Schedule(this, inputDeps);
    }
}
