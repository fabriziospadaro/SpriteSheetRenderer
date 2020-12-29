using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(8)]
public struct SpriteColorBuffer : IBufferElementData
{
    public static implicit operator float4(SpriteColorBuffer e) { return e.color; }
    public static implicit operator SpriteColorBuffer(float4 e) { return new SpriteColorBuffer { color = e }; }
    public float4 color;
}
