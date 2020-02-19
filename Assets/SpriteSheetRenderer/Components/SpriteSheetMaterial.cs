using UnityEngine;
using Unity.Entities;
using System;

public struct SpriteSheetMaterial : ISharedComponentData, IEquatable<SpriteSheetMaterial> {
  public Material material;
  public bool Equals(SpriteSheetMaterial other) {
    return material == other.material;
  }
  public override int GetHashCode() {
    return material ? material.GetHashCode() : 1371622046;
  }
}
