using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[InternalBufferCapacity(16)]
public struct MatrixBuffer : IBufferElementData
{
    public static implicit operator float4(MatrixBuffer e) { return e.matrix; }
    public static implicit operator MatrixBuffer(float4 e) { return new MatrixBuffer { matrix = e }; }
    public float4 matrix;
}