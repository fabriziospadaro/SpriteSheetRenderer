using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

[DisableAutoCreation]
public class SpriteSheetColorSystem : JobComponentSystem {
  

  [BurstCompile]
  struct SpriteSheetColorJob : IJobForEach<SpriteSheetColor, RenderData> {
    public void Execute([ReadOnly] ref SpriteSheetColor color, ref RenderData renderData) {
      UnityEngine.Color col = color;
      renderData.color = new float4(col.r, col.g, col.b, col.a);
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    var job = new SpriteSheetColorJob() { };
    return job.Schedule(this, inputDeps);
  }
}
