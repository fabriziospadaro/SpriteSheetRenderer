using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct SpriteSheetColor : IComponentData {
  public Color value;
  public static implicit operator Color(SpriteSheetColor c) => c.value;
  public static implicit operator SpriteSheetColor(Color c) => new SpriteSheetColor { value = c };
}
