using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct ColorBuffer : IBufferElementData {
  public Color value;
  public static implicit operator Color(ColorBuffer c) => c.value;
  public static implicit operator ColorBuffer(Color c) => new ColorBuffer { value = c };
}

[System.Serializable]
public struct MatrixBuffer : IBufferElementData {
  public float4x2 value;
  public static implicit operator float4x2(MatrixBuffer v) => v.value;
  public static implicit operator MatrixBuffer(float4x2 v) => new MatrixBuffer { value = v };
}

[System.Serializable]
public struct PosBuffer : IBufferElementData {
  public float2 pos;
  public static implicit operator float2(PosBuffer p) => p.pos;
  public static implicit operator PosBuffer(float2 p) => new PosBuffer { pos = p };
}

public struct UVCellBuffer : IBufferElementData {
  public int value;
  public static implicit operator int(UVCellBuffer v) => v.value;
  public static implicit operator UVCellBuffer(int v) => new UVCellBuffer { value = v };
}

public struct AnimationBuffer : IBufferElementData {

}