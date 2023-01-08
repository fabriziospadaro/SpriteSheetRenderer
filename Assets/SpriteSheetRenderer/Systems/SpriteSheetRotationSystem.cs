using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

public partial class SpriteSheetRotationSystem : SystemBase {
  protected override void OnUpdate(){
    Entities.WithName("SpriteSheetRotationSystem").WithChangeFilter<Rotation2D>().ForEach(
      (ref SpriteMatrix renderData, in Rotation2D rotation) => {
        renderData.matrix.z = rotation.angle;
      }
    ).ScheduleParallel();
  }
}
