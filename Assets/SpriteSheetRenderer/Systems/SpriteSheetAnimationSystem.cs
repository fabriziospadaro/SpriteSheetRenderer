using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

public class SpriteSheetAnimationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Dependency = Entities
            .WithBurst()
            .ForEach((ref SpriteSheetAnimation animation, ref SpriteIndex spriteIndex) =>
            {
                if (animation.play && animation.elapsedFrames % animation.samples == 0 && animation.elapsedFrames != 0)
                {
                    switch (animation.repetition)
                    {
                        case SpriteSheetAnimation.RepetitionType.Once:
                            if (spriteIndex.Value + 1 < animation.maxSprites)
                            {
                                spriteIndex.Value += 1;
                            }
                            else
                            {
                                animation.play = false;
                            }
                            break;
                        case SpriteSheetAnimation.RepetitionType.Loop:
                            spriteIndex.Value = spriteIndex.Value + 1 >= animation.maxSprites ? 0 : spriteIndex.Value + 1;
                            break;
                    }
                    animation.elapsedFrames = 0;
                }
                else if (animation.play)
                {
                    animation.elapsedFrames += 1;
                }
            })
            .Schedule(Dependency);
    }

    public static bool NextWillReachEnd(SpriteSheetAnimation animation, SpriteIndex spriteIndex)
    {
        return spriteIndex.Value + 1 >= animation.maxSprites;
    }
}
