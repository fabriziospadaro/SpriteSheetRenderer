using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// This describes the number of buffer elements that should be reserved
// in chunk data for each instance of a buffer. In this case, 8 integers
// will be reserved (32 bytes) along with the size of the buffer header
// (currently 16 bytes on 64-bit targets)
[InternalBufferCapacity(8)]
public struct UvBuffer : IBufferElementData
{
    public static implicit operator float4(UvBuffer e) { return e.uv; }
    public static implicit operator UvBuffer(float4 e) { return new UvBuffer { uv = e }; }
    public float4 uv;
}