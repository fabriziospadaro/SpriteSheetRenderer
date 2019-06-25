using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

public class SpriteSheetRenderer : ComponentSystem {
  private Mesh mesh;

  RenderInformation[] renderInfos;
  EntityQuery processable;
  EntityQuery uvBufferQuery;
  EntityQuery indexBufferQuery;
  EntityQuery colorBufferQuery;
  EntityQuery matrixBufferQuery;

  int shaderPropertyId;
  bool initialize = false;
  ComputeBuffer uvBuffer;
  ComputeBuffer indexBuffer;
  protected override void OnCreate() {
    shaderPropertyId = Shader.PropertyToID("_MainText_UV");
    processable = GetEntityQuery(ComponentType.ReadOnly<RenderData>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
    uvBufferQuery = GetEntityQuery(ComponentType.ReadOnly<UvBuffer>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
    colorBufferQuery = GetEntityQuery(ComponentType.ReadOnly<SpriteColorBuffer>());
    matrixBufferQuery = GetEntityQuery(ComponentType.ReadOnly<MatrixBuffer>());
    indexBufferQuery = GetEntityQuery(ComponentType.ReadOnly<SpriteIndexBuffer>());
  }

  protected override void OnDestroy() {
    for(int i = 0; i < renderInfos.Length; i++)
      renderInfos[i].DestroyBuffers();
  }

  protected override void OnUpdate() {
    if(!initialize) {
      List<SpriteSheetMaterial> materials = new List<SpriteSheetMaterial>();
      EntityManager.GetAllUniqueSharedComponentData(materials);
      for(int i = 0; i < materials.Count; i++)
        if(!materials[i].material)
          materials.Remove(materials[i]);

      renderInfos = new RenderInformation[materials.Count];
      for(int i = 0; i < renderInfos.Length; i++)
        renderInfos[i] = new RenderInformation(materials[i].material);

      mesh = MeshExtension.Quad();
      initialize = true;
    }

    for(int i = 0; i < renderInfos.Length; i++)
      if(UpdateBuffers(i) > 0)
        Graphics.DrawMeshInstancedIndirect(mesh, 0, renderInfos[i].material, new Bounds(Vector2.zero, Vector3.one), renderInfos[i].argsBuffer);

  }
  void ClearDataBuffers(int bufferID) {
    if(renderInfos[bufferID].matrixBuffer != null)
      renderInfos[bufferID].matrixBuffer.Release();
    if(renderInfos[bufferID].colorsBuffer != null)
      renderInfos[bufferID].colorsBuffer.Release();
  }

  int UpdateBuffers(int renderIndex) {
    ClearDataBuffers(renderIndex);
    SpriteSheetMaterial material = new SpriteSheetMaterial {
      material = renderInfos[renderIndex].material
    };
    uvBufferQuery.SetFilter(material);

    int instanceCount = EntityManager.GetBuffer<MatrixBuffer>(matrixBufferQuery.GetSingletonEntity()).Length;
    if(instanceCount > 0) {
      if(uvBuffer != null)
        uvBuffer.Release();

      uvBuffer = new ComputeBuffer(instanceCount, 16);
      uvBuffer.SetData(EntityManager.GetBuffer<UvBuffer>(uvBufferQuery.GetSingletonEntity()).Reinterpret<float4>().AsNativeArray());
      renderInfos[renderIndex].material.SetBuffer("uvBuffer", uvBuffer);

      if(indexBuffer != null)
        indexBuffer.Release();

      indexBuffer = new ComputeBuffer(instanceCount, sizeof(int));
      indexBuffer.SetData(EntityManager.GetBuffer<SpriteIndexBuffer>(indexBufferQuery.GetSingletonEntity()).Reinterpret<int>().AsNativeArray());
      renderInfos[renderIndex].material.SetBuffer("indexBuffer", indexBuffer);

      renderInfos[renderIndex].matrixBuffer = new ComputeBuffer(instanceCount, 16);
      renderInfos[renderIndex].matrixBuffer.SetData(EntityManager.GetBuffer<MatrixBuffer>(matrixBufferQuery.GetSingletonEntity()).Reinterpret<float4>().AsNativeArray());
      renderInfos[renderIndex].material.SetBuffer("matrixBuffer", renderInfos[renderIndex].matrixBuffer);

      renderInfos[renderIndex].args[1] = (uint)instanceCount;
      renderInfos[renderIndex].argsBuffer.SetData(renderInfos[renderIndex].args);

      renderInfos[renderIndex].colorsBuffer = new ComputeBuffer(instanceCount, 16);
      renderInfos[renderIndex].colorsBuffer.SetData(EntityManager.GetBuffer<SpriteColorBuffer>(colorBufferQuery.GetSingletonEntity()).Reinterpret<float4>().AsNativeArray());
      renderInfos[renderIndex].material.SetBuffer("colorsBuffer", renderInfos[renderIndex].colorsBuffer);
    }
    return instanceCount;
  }

}