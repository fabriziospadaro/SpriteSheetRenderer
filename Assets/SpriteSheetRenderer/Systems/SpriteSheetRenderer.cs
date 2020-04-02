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

  int shaderPropertyId;
  protected override void OnCreate() {
    mesh = MeshExtension.Quad();
    shaderPropertyId = Shader.PropertyToID("_MainText_UV");
  }

  protected override void OnDestroy() {
    SpriteSheetManager.CleanBuffers();
  }
  protected override void OnUpdate() {

    for(int i = 0; i < SpriteSheetManager.renderInformation.Count; i++) {
      if(UpdateBuffers(i) > 0)
        Graphics.DrawMeshInstancedIndirect(mesh, 0, SpriteSheetManager.renderInformation[i].material, new Bounds(Vector2.zero, Vector3.one), SpriteSheetManager.renderInformation[i].argsBuffer);

      //this is just a wip to clean the old buffers
      DynamicBuffer<SpriteIndexBuffer> indexBuffer = EntityManager.GetBuffer<SpriteIndexBuffer>(SpriteSheetManager.renderInformation[i].bufferEntity);
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
        EntityManager.GetBuffer<SpriteIndexBuffer>(SpriteSheetManager.renderInformation[i].bufferEntity).RemoveRange(size + 1 - toRemove, toRemove);
        EntityManager.GetBuffer<MatrixBuffer>(SpriteSheetManager.renderInformation[i].bufferEntity).RemoveRange(size + 1 - toRemove, toRemove);
        EntityManager.GetBuffer<SpriteColorBuffer>(SpriteSheetManager.renderInformation[i].bufferEntity).RemoveRange(size + 1 - toRemove, toRemove);
      }
    }
  }

  int UpdateBuffers(int renderIndex) {
    SpriteSheetManager.ReleaseBuffer(renderIndex);

    RenderInformation renderInformation = SpriteSheetManager.renderInformation[renderIndex];
    int instanceCount = EntityManager.GetBuffer<SpriteIndexBuffer>(renderInformation.bufferEntity).Length;
    if(instanceCount > 0) {
      //TODO: deve moltiplicare il numero di sprites per questa animazione
      int stride = 16 * SpriteSheetCache.GetLenght(renderInformation.material);
      renderInformation.uvBuffer = new ComputeBuffer(instanceCount, stride);
      renderInformation.uvBuffer.SetData(EntityManager.GetBuffer<UvBuffer>(renderInformation.bufferEntity).Reinterpret<float4>().AsNativeArray());
      renderInformation.material.SetBuffer("uvBuffer", renderInformation.uvBuffer);


      renderInformation.indexBuffer = new ComputeBuffer(instanceCount, sizeof(int));
      renderInformation.indexBuffer.SetData(EntityManager.GetBuffer<SpriteIndexBuffer>(renderInformation.bufferEntity).Reinterpret<int>().AsNativeArray());
      renderInformation.material.SetBuffer("indexBuffer", renderInformation.indexBuffer);

      renderInformation.matrixBuffer = new ComputeBuffer(instanceCount, 16);
      renderInformation.matrixBuffer.SetData(EntityManager.GetBuffer<MatrixBuffer>(renderInformation.bufferEntity).Reinterpret<float4>().AsNativeArray());
      renderInformation.material.SetBuffer("matrixBuffer", renderInformation.matrixBuffer);

      renderInformation.args[1] = (uint)instanceCount;
      renderInformation.argsBuffer.SetData(renderInformation.args);

      renderInformation.colorsBuffer = new ComputeBuffer(instanceCount, 16);
      renderInformation.colorsBuffer.SetData(EntityManager.GetBuffer<SpriteColorBuffer>(renderInformation.bufferEntity).Reinterpret<float4>().AsNativeArray());
      renderInformation.material.SetBuffer("colorsBuffer", renderInformation.colorsBuffer);
    }
    return instanceCount;
  }

}