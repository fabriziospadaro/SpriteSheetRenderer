using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

public class SpriteSheetPositionSystem : SystemBase {
  protected override void OnUpdate(){
    Entities.WithName("SpriteSheetPositionSystem").WithChangeFilter<Position2D>().ForEach(
      (ref SpriteMatrix renderData, in Position2D translation) => {
        renderData.matrix.xy = translation.Value.xy;
      }
    ).ScheduleParallel();
  }
}
