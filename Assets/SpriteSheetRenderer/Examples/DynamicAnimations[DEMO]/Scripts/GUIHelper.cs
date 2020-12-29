using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIHelper : MonoBehaviour
{
    public void ChangeAnimation(string animationName)
    {
        SpriteSheetAnimator.Play(DynamicAnimationsDemo.character, animationName);
    }
}
