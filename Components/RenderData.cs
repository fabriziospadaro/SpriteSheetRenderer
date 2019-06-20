using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct RenderData : IComponentData {
  public float4 transform;
  public float4 uv;
  public float4 color;
}