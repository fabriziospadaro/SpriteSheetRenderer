using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;

public class SpriteSheetScaleSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Dependency = Entities
            .WithBurst()
            .WithChangeFilter<Scale>()
            .ForEach((ref SpriteMatrix renderData, in Scale scale) =>
            {
                renderData.matrix.w = scale.Value;
            })
            .ScheduleParallel(Dependency);
    }
}
