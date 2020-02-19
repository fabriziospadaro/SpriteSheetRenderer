using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
[InternalBufferCapacity(sizeof(int))]
public struct SpriteIndexBuffer : IBufferElementData {
  public static implicit operator int(SpriteIndexBuffer e) { return e.index; }
  public static implicit operator SpriteIndexBuffer(int e) { return new SpriteIndexBuffer { index = e }; }
  public int index;
}
