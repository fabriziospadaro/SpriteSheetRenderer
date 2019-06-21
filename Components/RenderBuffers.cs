using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct RenderData : IComponentData {
  public float4x2 matrix;
  public float4 color;
}