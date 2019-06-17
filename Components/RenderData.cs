using Unity.Entities;
using Unity.Mathematics;

public struct RenderData : IComponentData {
  public float4 transform;
  public float4 uv;
}