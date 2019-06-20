using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct SpriteColor : IComponentData
{
    public float4 value;
    public Color color
    {
        get => new Color(value.x, value.y, value.z, value.w);
        set => this.value = new float4(value.r, value.g, value.b, value.a);
    }
    public static implicit operator Color(SpriteColor c) => c.color;
    public static implicit operator SpriteColor(Color c) => new SpriteColor { color = c};
}
