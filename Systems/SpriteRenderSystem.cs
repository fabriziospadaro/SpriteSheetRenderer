using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

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
  // before rendering. So keep them around for one frame and dispose them on the next render.
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
    EntityManager.GetAllUniqueSharedComponentData(sharedMaterials);
    sharedMaterials.RemoveAt(0);

    CleanOldBuffers();

    foreach ( var sharedMat in sharedMaterials) {

      matricesBufferQ.SetFilter(sharedMat);
      colorsBufferQ.SetFilter(sharedMat);
      positionsBufferQ.SetFilter(sharedMat);
      
      var matrixDataBuffer = EntityManager.GetBuffer<MatrixBuffer>(matricesBufferQ.GetSingletonEntity());
      var colorDataBuffer = EntityManager.GetBuffer<ColorBuffer>(colorsBufferQ.GetSingletonEntity());
      var uvCellsDataBuffer = EntityManager.GetBuffer<UVCellBuffer>(uvCellsBufferQ.GetSingletonEntity());

      int instanceCount = matrixDataBuffer.Length;
      
      var matrixData = matrixDataBuffer.Reinterpret<float4x2>().AsNativeArray();
      var matrixBuffer = GetBuffer(instanceCount, sizeof(float) * 8);
      matrixBuffer.SetData(matrixData);
      sharedMat.material.SetBuffer("matrixBuffer", matrixBuffer);
      
      var colorData = colorDataBuffer.Reinterpret<float4>().AsNativeArray();
      var colorsBuffer = GetBuffer(instanceCount, sizeof(float) * 4);
      colorsBuffer.SetData(colorData);
      sharedMat.material.SetBuffer("colorsBuffer", colorsBuffer);
      
      var uvCellsData = uvCellsDataBuffer.Reinterpret<int>().AsNativeArray();
      var uvCellsBuffer = GetBuffer(instanceCount, sizeof(int));
      uvCellsBuffer.SetData(uvCellsData);
      sharedMat.material.SetBuffer("uvCellsBuffer", uvCellsBuffer);
      
      var uvBuffer = CachedUVData.GetUVBuffer(sharedMat.material);
      oldBuffers.Enqueue(uvBuffer);
      sharedMat.material.SetBuffer("uvBuffer", uvBuffer);
      
      args[1] = (uint)instanceCount;
      argsBuffer.SetData(args);
      var bounds = World.GetOrCreateSystem<CopyMatrixDataSystem>().Bounds;
      Graphics.DrawMeshInstancedIndirect(mesh, 0, sharedMat.material, bounds, argsBuffer);
    }

  }
}
