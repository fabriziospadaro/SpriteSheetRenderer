using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

public class SpriteSheetAnimationSystem : JobComponentSystem
{
    [BurstCompile]
    struct SpriteSheetAnimationJob : IJobForEach<SpriteSheetAnimation, SpriteIndex>
    {

        public void Execute(ref SpriteSheetAnimation AnimCmp, ref SpriteIndex spriteSheetCmp)
        {
            if (AnimCmp.play && AnimCmp.elapsedFrames % AnimCmp.samples == 0 && AnimCmp.elapsedFrames != 0)
            {
                switch (AnimCmp.repetition)
                {
                    case SpriteSheetAnimation.RepetitionType.Once:
                        if (!NextWillReachEnd(AnimCmp, spriteSheetCmp))
                        {
                            spriteSheetCmp.Value += 1;
                        }
                        else
                        {
                            AnimCmp.play = false;
                            AnimCmp.elapsedFrames = 0;
                        }
                        break;
                    case SpriteSheetAnimation.RepetitionType.Loop:
                        if (NextWillReachEnd(AnimCmp, spriteSheetCmp))
                            spriteSheetCmp.Value = 0;
                        else
                            spriteSheetCmp.Value += 1;
                        break;
                }
                AnimCmp.elapsedFrames = 0;
            }
            else if (AnimCmp.play)
            {
                AnimCmp.elapsedFrames += 1;
            }
        }
        public bool NextWillReachEnd(SpriteSheetAnimation anim, SpriteIndex sprite)
        {
            return sprite.Value + 1 >= anim.maxSprites;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new SpriteSheetAnimationJob();
        return job.Schedule(this, inputDeps);
    }
}

