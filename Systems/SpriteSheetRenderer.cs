using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using System.Linq;
public class SpriteSheetRenderer : ComponentSystem {
  private Mesh mesh;

  RenderInformation[] renderInfos;
  /*
  EntityQuery uvBufferQuery;
  EntityQuery indexBufferQuery;
  EntityQuery colorBufferQuery;
  EntityQuery matrixBufferQuery;
  */
  int shaderPropertyId;
  bool initialize = false;
  protected override void OnCreate() {
    shaderPropertyId = Shader.PropertyToID("_MainText_UV");
    /*
    uvBufferQuery = GetEntityQuery(ComponentType.ReadOnly<UvBuffer>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
    colorBufferQuery = GetEntityQuery(ComponentType.ReadOnly<SpriteColorBuffer>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
    matrixBufferQuery = GetEntityQuery(ComponentType.ReadOnly<MatrixBuffer>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
    indexBufferQuery = GetEntityQuery(ComponentType.ReadOnly<SpriteIndexBuffer>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
    */
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
      for(int i = 0; i < renderInfos.Length; i++) {
        renderInfos[i] = new RenderInformation(materials[i].material, DynamicBufferManager.GetEntityBuffer(materials[i].material));
      }

      mesh = MeshExtension.Quad();
      initialize = true;
    }


    for(int i = 0; i < renderInfos.Length; i++) {
      if(UpdateBuffers(i) > 0)
        Graphics.DrawMeshInstancedIndirect(mesh, 0, renderInfos[i].material, new Bounds(Vector2.zero, Vector3.one), renderInfos[i].argsBuffer);

      //this is just a wip to clean the old buffers
      DynamicBuffer<SpriteIndexBuffer> indexBuffer = EntityManager.GetBuffer<SpriteIndexBuffer>(renderInfos[i].bufferEntity);
      int size = indexBuffer.Length - 1;
      int toRemove = 0;
      for(int j = size; j >= 0; j--) {
        if(indexBuffer[j].index == -1) {
          toRemove++;
        }
        else {
          break;
        }
      }
      if(toRemove > 0) {
        EntityManager.GetBuffer<SpriteIndexBuffer>(renderInfos[i].bufferEntity).RemoveRange(size + 1 - toRemove, toRemove);
        EntityManager.GetBuffer<MatrixBuffer>(renderInfos[i].bufferEntity).RemoveRange(size + 1 - toRemove, toRemove);
        EntityManager.GetBuffer<SpriteColorBuffer>(renderInfos[i].bufferEntity).RemoveRange(size + 1 - toRemove, toRemove);
      }
    }
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

    /*
    uvBufferQuery.SetFilter(material);
    matrixBufferQuery.SetFilter(material);
    indexBufferQuery.SetFilter(material);
    colorBufferQuery.SetFilter(material);
    */

    int instanceCount = EntityManager.GetBuffer<SpriteIndexBuffer>(renderInfos[renderIndex].bufferEntity).Length;
    if(instanceCount > 0) {
      int stride = instanceCount >= 16 * renderInfos[renderIndex].spriteCount ? 1 : 16 * renderInfos[renderIndex].spriteCount;
      renderInfos[renderIndex].uvBuffer = new ComputeBuffer(instanceCount, stride);
      renderInfos[renderIndex].uvBuffer.SetData(EntityManager.GetBuffer<UvBuffer>(renderInfos[renderIndex].bufferEntity).Reinterpret<float4>().AsNativeArray());
      renderInfos[renderIndex].material.SetBuffer("uvBuffer", renderInfos[renderIndex].uvBuffer);


      renderInfos[renderIndex].indexBuffer = new ComputeBuffer(instanceCount, sizeof(int));
      renderInfos[renderIndex].indexBuffer.SetData(EntityManager.GetBuffer<SpriteIndexBuffer>(renderInfos[renderIndex].bufferEntity).Reinterpret<int>().AsNativeArray());
      renderInfos[renderIndex].material.SetBuffer("indexBuffer", renderInfos[renderIndex].indexBuffer);

      renderInfos[renderIndex].matrixBuffer = new ComputeBuffer(instanceCount, 16);
      renderInfos[renderIndex].matrixBuffer.SetData(EntityManager.GetBuffer<MatrixBuffer>(renderInfos[renderIndex].bufferEntity).Reinterpret<float4>().AsNativeArray());
      renderInfos[renderIndex].material.SetBuffer("matrixBuffer", renderInfos[renderIndex].matrixBuffer);

      renderInfos[renderIndex].args[1] = (uint)instanceCount;
      renderInfos[renderIndex].argsBuffer.SetData(renderInfos[renderIndex].args);

      renderInfos[renderIndex].colorsBuffer = new ComputeBuffer(instanceCount, 16);
      renderInfos[renderIndex].colorsBuffer.SetData(EntityManager.GetBuffer<SpriteColorBuffer>(renderInfos[renderIndex].bufferEntity).Reinterpret<float4>().AsNativeArray());
      renderInfos[renderIndex].material.SetBuffer("colorsBuffer", renderInfos[renderIndex].colorsBuffer);
    }
    return instanceCount;
  }

}