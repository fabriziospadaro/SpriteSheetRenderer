using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public class SpriteRenderSystem : ComponentSystem {

  EntityQuery colorsBufferQ;
  EntityQuery matricesBufferQ;
  EntityQuery positionsBufferQ;
  EntityQuery uvCellsBufferQ;
  
  List<SpriteSheetMaterial> sharedMaterials = new List<SpriteSheetMaterial>();

  Mesh mesh;

  ComputeBuffer argsBuffer;
  uint[] args;

  // Buffers can't be disposed in the same frame you call Graphics.DrawMesh or their data will be erased
  // rendering. So keep them around for one frame and dispose them on the next render.
  Queue<ComputeBuffer> oldBuffers = new Queue<ComputeBuffer>();


  protected override void OnCreate() {
    colorsBufferQ = GetEntityQuery(ComponentType.ReadOnly<ColorBuffer>(), typeof(SpriteSheetMaterial));
    matricesBufferQ = GetEntityQuery(ComponentType.ReadOnly<MatrixBuffer>(), typeof(SpriteSheetMaterial));
    positionsBufferQ = GetEntityQuery(ComponentType.ReadOnly<PosBuffer>(), typeof(SpriteSheetMaterial));
    uvCellsBufferQ = GetEntityQuery(ComponentType.ReadOnly<UVCellBuffer>(), typeof(SpriteSheetMaterial));
    args = new uint[5] { 6, 0, 0, 0, 0 };
    argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
    mesh = MeshExtension.Quad();
  }
  
  protected override void OnDestroy() {
    base.OnDestroy();
    argsBuffer.Dispose();
    CleanOldBuffers();
  }

  ComputeBuffer GetBuffer(int instanceCount, int stride) {
    var buffer = new ComputeBuffer(instanceCount, stride);
    oldBuffers.Enqueue(buffer);
    return buffer;
  }

  void CleanOldBuffers() {
    while (oldBuffers.Count > 0)
      oldBuffers.Dequeue().Dispose();
  }

  protected override void OnUpdate() {
    sharedMaterials.Clear();
    EntityManager.GetAllUniqueSharedComponentData<SpriteSheetMaterial>(sharedMaterials);
    sharedMaterials.RemoveAt(0);

    CleanOldBuffers();

    foreach ( var sharedMat in sharedMaterials) {

      matricesBufferQ.SetFilter(sharedMat);
      colorsBufferQ.SetFilter(sharedMat);
      positionsBufferQ.SetFilter(sharedMat);
      
      var matrixDataBuffer = EntityManager.GetBuffer<MatrixBuffer>(matricesBufferQ.GetSingletonEntity());
      var colorDataBuffer = EntityManager.GetBuffer<ColorBuffer>(colorsBufferQ.GetSingletonEntity());
      var uvCellsDataBuffer = EntityManager.GetBuffer<UVCellBuffer>(uvCellsBufferQ.GetSingletonEntity());
      //var positionsDataBuffer = EntityManager.GetBuffer<PosBuffer>(positionsBufferQ.GetSingletonEntity());

      int instanceCount = matrixDataBuffer.Length;

      var colorData = colorDataBuffer.Reinterpret<float4>().ToNativeArray(Allocator.TempJob);
      var colorsBuffer = GetBuffer(instanceCount, sizeof(float) * 4);
      colorsBuffer.SetData(colorData);
      colorData.Dispose();
      sharedMat.material.SetBuffer("colorsBuffer", colorsBuffer);

      var matrixData = matrixDataBuffer.Reinterpret<float4x2>().ToNativeArray(Allocator.TempJob);
      var matrixBuffer = GetBuffer(instanceCount, sizeof(float) * 8);
      matrixBuffer.SetData(matrixData);
      matrixData.Dispose();
      sharedMat.material.SetBuffer("matrixBuffer", matrixBuffer);

      var uvCellsData = uvCellsDataBuffer.Reinterpret<int>().ToNativeArray(Allocator.TempJob);
      var uvCellsBuffer = GetBuffer(instanceCount, sizeof(int));
      uvCellsBuffer.SetData(uvCellsData);
      uvCellsData.Dispose();
      sharedMat.material.SetBuffer("uvCellsBuffer", uvCellsBuffer);

      var uvBuffer = CachedUVData.GetUVBuffer(sharedMat.material);
      oldBuffers.Enqueue(uvBuffer);
      sharedMat.material.SetBuffer("uvBuffer", uvBuffer);
      
      //var posBuffer = GetBuffer(instanceCount, sizeof(float) * 2);
      //var posData = positionsDataBuffer.Reinterpret<float2>().ToNativeArray(Allocator.TempJob);
      //posBuffer.SetData(posData);
      //sharedMat.material.SetBuffer("posBuffer", posBuffer);
      //posData.Dispose();

      args[1] = (uint)instanceCount;
      argsBuffer.SetData(args);

      Graphics.DrawMeshInstancedIndirect(mesh, 0, sharedMat.material, new Bounds(Vector2.zero, new Vector3(500, 500, 5)), argsBuffer);
    }

  }
}
