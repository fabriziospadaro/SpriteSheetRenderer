using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

namespace ECSSpriteSheetAnimation
{
    public class SpriteSheetRotationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Dependency = Entities
                .WithBurst()
                .ForEach((ref SpriteMatrix renderData, in Rotation2D rotation) =>
                {
                    renderData.matrix.z = rotation.angle;
                })
                .ScheduleParallel(Dependency);
        }
    } 
}
