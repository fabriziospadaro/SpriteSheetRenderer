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
  protected override void OnCreate() {
    shaderPropertyId = Shader.PropertyToID("_MainText_UV");
    processable = GetEntityQuery(ComponentType.ReadOnly<SpriteMatrix>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
    uvBufferQuery = GetEntityQuery(ComponentType.ReadOnly<UvBuffer>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
    colorBufferQuery = GetEntityQuery(ComponentType.ReadOnly<SpriteColorBuffer>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
    matrixBufferQuery = GetEntityQuery(ComponentType.ReadOnly<MatrixBuffer>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
    indexBufferQuery = GetEntityQuery(ComponentType.ReadOnly<SpriteIndexBuffer>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
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
    if(renderInfos[bufferID].uvBuffer != null)
      renderInfos[bufferID].uvBuffer.Release();
    if(renderInfos[bufferID].indexBuffer != null)
      renderInfos[bufferID].indexBuffer.Release();
  }

  int UpdateBuffers(int renderIndex) {
    ClearDataBuffers(renderIndex);
    SpriteSheetMaterial material = new SpriteSheetMaterial {
      material = renderInfos[renderIndex].material
    };

    uvBufferQuery.SetFilter(material);
    matrixBufferQuery.SetFilter(material);
    indexBufferQuery.SetFilter(material);
    colorBufferQuery.SetFilter(material);

    var entities = matrixBufferQuery.ToEntityArray(Allocator.TempJob);
    var bufferEntity = entities[0];
    entities.Dispose();

    int instanceCount = EntityManager.GetBuffer<SpriteIndexBuffer>(bufferEntity).Length;
    if(instanceCount > 0) {

      renderInfos[renderIndex].uvBuffer = new ComputeBuffer(instanceCount, 16);
      renderInfos[renderIndex].uvBuffer.SetData(EntityManager.GetBuffer<UvBuffer>(bufferEntity).Reinterpret<float4>().AsNativeArray());
      renderInfos[renderIndex].material.SetBuffer("uvBuffer", renderInfos[renderIndex].uvBuffer);


      renderInfos[renderIndex].indexBuffer = new ComputeBuffer(instanceCount, sizeof(int));
      renderInfos[renderIndex].indexBuffer.SetData(EntityManager.GetBuffer<SpriteIndexBuffer>(bufferEntity).Reinterpret<int>().AsNativeArray());
      renderInfos[renderIndex].material.SetBuffer("indexBuffer", renderInfos[renderIndex].indexBuffer);

      renderInfos[renderIndex].matrixBuffer = new ComputeBuffer(instanceCount, 16);
      renderInfos[renderIndex].matrixBuffer.SetData(EntityManager.GetBuffer<MatrixBuffer>(bufferEntity).Reinterpret<float4>().AsNativeArray());
      renderInfos[renderIndex].material.SetBuffer("matrixBuffer", renderInfos[renderIndex].matrixBuffer);

      renderInfos[renderIndex].args[1] = (uint)instanceCount;
      renderInfos[renderIndex].argsBuffer.SetData(renderInfos[renderIndex].args);

      renderInfos[renderIndex].colorsBuffer = new ComputeBuffer(instanceCount, 16);
      renderInfos[renderIndex].colorsBuffer.SetData(EntityManager.GetBuffer<SpriteColorBuffer>(bufferEntity).Reinterpret<float4>().AsNativeArray());
      renderInfos[renderIndex].material.SetBuffer("colorsBuffer", renderInfos[renderIndex].colorsBuffer);
    }
    return instanceCount;
  }

}