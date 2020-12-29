using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[System.Serializable]
public abstract class SpriteSheetAnimator : ScriptableObject
{
    public SpriteSheetAnimationData[] animations;
    public int defaultAnimationIndex;
    [HideInInspector]
    public int currentAnimationIndex = 0;
    public float speed = 1;
    [HideInInspector]
    public Entity managedEntity;
    public void Play(string animationName = "")
    {
        if (animationName == "")
            animationName = animations[currentAnimationIndex].name;
        int i = 0;
        foreach (SpriteSheetAnimationData animation in animations)
        {
            if (animation.animationName == animationName)
            {
                SpriteSheetManager.SetAnimation(managedEntity, animation);
                currentAnimationIndex = i;
                return;
            }
            i++;
        }
    }
    public static void Play(Entity e, string animationName = "")
    {
        SpriteSheetAnimator animator = SpriteSheetCache.GetAnimator(e);
        if (animationName == "")
            animationName = animator.animations[animator.currentAnimationIndex].name;
        int i = 0;
        foreach (SpriteSheetAnimationData animation in animator.animations)
        {
            if (animation.animationName == animationName)
            {
                SpriteSheetManager.SetAnimation(e, animation);
                animator.currentAnimationIndex = i;
                return;
            }
            i++;
        }
    }
    public static void Play(EntityCommandBuffer buffer, Entity e, BufferHook hook, string animationName = "")
    {
        SpriteSheetAnimator animator = SpriteSheetCache.GetAnimator(e);
        if (animationName == "")
            animationName = animator.animations[animator.currentAnimationIndex].name;
        int i = 0;
        foreach (SpriteSheetAnimationData animation in animator.animations)
        {
            if (animation.animationName == animationName)
            {
                SpriteSheetManager.SetAnimation(buffer, e, animation, hook);
                animator.currentAnimationIndex = i;
                return;
            }
            i++;
        }
    }
}