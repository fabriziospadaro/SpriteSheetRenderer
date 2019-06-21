using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[UpdateBefore(typeof(CopyUVCellDataSystem))]
public class SpriteAnimationSystem : JobComponentSystem {

  struct AnimationJob : IJobForEach<SpriteSheetAnimation, UVCell> {
    public float dt;
    public void Execute(ref SpriteSheetAnimation anim, ref UVCell cell) {
      if (!anim.play)
        return;

      anim.elapsed += dt;

      if( anim.elapsed >= anim.fps ) {
        int min = anim.frameMin;
        int max = anim.frameMax;
        int curr = cell.value;
        int count = max - min;

        int i = curr - min;
        i = (i + 1) % count;
        cell.value = min + i;
        anim.elapsed = 0;
      }
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    inputDeps = new AnimationJob {
      dt = Time.deltaTime,
    }.Schedule(this, inputDeps);
    return inputDeps;
  }
}
