using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace ECSSpriteSheetAnimation
{
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

        public void Play(string animationName = null)
        {
            if (string.IsNullOrEmpty(animationName))
                animationName = animations[currentAnimationIndex].name;
            for (int i = 0; i < animations.Length; i++)
            {
                var animation = animations[i];
                if (animation.animationName == animationName)
                {
                    SpriteSheetManager.SetAnimation(managedEntity, animation);
                    currentAnimationIndex = i;
                    return;
                }
            }
        }
        public static void Play(Entity e, string animationName = null)
        {
            SpriteSheetAnimator animator = SpriteSheetCache.GetAnimator(e);
            if (string.IsNullOrEmpty(animationName))
                animationName = animator.animations[animator.currentAnimationIndex].name;
            for (int i = 0; i < animator.animations.Length; i++)
            {
                var animation = animator.animations[i];
                if (animation.animationName == animationName)
                {
                    SpriteSheetManager.SetAnimation(e, animation);
                    animator.currentAnimationIndex = i;
                    return;
                }
            }
        }
        public static void Play(EntityCommandBuffer buffer, Entity e, BufferHook hook, string animationName = null)
        {
            SpriteSheetAnimator animator = SpriteSheetCache.GetAnimator(e);
            if (string.IsNullOrEmpty(animationName))
                animationName = animator.animations[animator.currentAnimationIndex].name;
            for (int i = 0; i < animator.animations.Length; i++)
            {
                var animation = animator.animations[i];
                if (animation.animationName == animationName)
                {
                    SpriteSheetManager.SetAnimation(buffer, e, animation, hook);
                    animator.currentAnimationIndex = i;
                    return;
                }
            }
        }
    } 
}
