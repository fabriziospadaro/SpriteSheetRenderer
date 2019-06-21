using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

//public class SpriteSheetAnimationSystem : JobComponentSystem {
//  EntityQuery animatedSprites;

//  [BurstCompile]
//  struct SpriteSheetAnimationJob : IJobForEach<SpriteSheetAnimation, UVCell> {
//    public int maxFrames;
//    public void Execute(ref SpriteSheetAnimation AnimCmp, ref UVCell cell) {
//      if(AnimCmp.play && AnimCmp.elapsedFrames % AnimCmp.samples == 0 && AnimCmp.elapsedFrames != 0) {
//        switch(AnimCmp.repetition) {
//          case SpriteSheetAnimation.RepetitionType.Once:
//            if(!NextWillReachEnd(spriteSheetCmp)) {
//              spriteSheetCmp.spriteIndex += 1;
//            }
//            else {
//              AnimCmp.play = false;
//              AnimCmp.elapsedFrames = 0;
//            }
//            break;
//          case SpriteSheetAnimation.RepetitionType.Loop:
//            if(NextWillReachEnd(spriteSheetCmp))
//              spriteSheetCmp.spriteIndex = 0;
//            else
//              spriteSheetCmp.spriteIndex += 1;
//            break;
//        }
//        AnimCmp.elapsedFrames = 0;
//      }
//      else if(AnimCmp.play) {
//        AnimCmp.elapsedFrames += 1;
//      }
//    }
//    public bool NextWillReachEnd(SpriteSheet sprite) {
//      return sprite.spriteIndex + 1 >= sprite.maxSprites;
//    }
//  }

//  protected override void OnCreate() {
//    base.OnCreate();
//    animatedSprites = GetEntityQuery(
//      ComponentType.ReadWrite<SpriteSheetAnimation>(),
//      ComponentType.ReadWrite<UVCell>(),
//      ComponentType.ReadOnly<SpriteSheetMaterial>());
//  }

//  protected override JobHandle OnUpdate(JobHandle inputDeps) {
//    var mat = 

//    var job = new SpriteSheetAnimationJob {

//    }.Schedule(animatedSprites, inputDeps);
//    return job;
//  }
//}

