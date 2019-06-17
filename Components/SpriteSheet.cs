using Unity.Entities;
using Unity.Mathematics;

public struct SpriteSheet : IComponentData {
  public int spriteIndex;
  public int2 cell;
}
