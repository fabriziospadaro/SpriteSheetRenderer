using Unity.Entities;
using Unity.Mathematics;

public struct Position2D : IComponentData {
  public float2 Value;
  public static implicit operator float2(Position2D p) => p.Value;
  public static implicit operator Position2D(float2 v) => new Position2D { Value = v };
}
