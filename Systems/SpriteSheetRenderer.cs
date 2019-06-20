using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
//[UpdateAfter(typeof(OcclusionCullingSystem))]
public class SpriteSheetRenderer : ComponentSystem {
  private Mesh mesh;

  RenderInformation[] renderInfos;
  EntityQuery processable;
  int shaderPropertyId;
  bool initialize = false;

  protected override void OnCreate() {
    shaderPropertyId = Shader.PropertyToID("_MainText_UV");
    processable = GetEntityQuery(ComponentType.ReadOnly<RenderData>(), ComponentType.ReadOnly<SpriteSheetMaterial>());
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
    processable.SetFilter(new SpriteSheetMaterial { material = renderInfos[renderIndex].material });
    NativeArray<RenderData> renderData = processable.ToComponentDataArray<RenderData>(Allocator.TempJob);
    int instanceCount = renderData.Length;
    if(instanceCount > 0) {
      NativeArray<float4x2> matrices = new NativeArray<float4x2>(instanceCount, Allocator.TempJob);
      NativeArray<float4> colors = new NativeArray<float4>(instanceCount, Allocator.TempJob);
      var job = new ProcessRenderDataJob() {
        renderData = renderData,
        matrices = matrices,
        colors = colors,
      };
      JobHandle jobHandle = job.Schedule();
      jobHandle.Complete();

      renderInfos[renderIndex].matrixBuffer = new ComputeBuffer(instanceCount, 32);
      renderInfos[renderIndex].matrixBuffer.SetData(job.matrices);
      renderInfos[renderIndex].material.SetBuffer("matrixBuffer", renderInfos[renderIndex].matrixBuffer);
      matrices.Dispose();

      renderInfos[renderIndex].args[1] = (uint)instanceCount;
      renderInfos[renderIndex].argsBuffer.SetData(renderInfos[renderIndex].args);

      renderInfos[renderIndex].colorsBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 4);
      renderInfos[renderIndex].colorsBuffer.SetData(job.colors);
      renderInfos[renderIndex].material.SetBuffer("colorsBuffer", renderInfos[renderIndex].colorsBuffer);
      colors.Dispose();
    }

    renderData.Dispose();
    return instanceCount;
  }
}

[BurstCompile]
struct ProcessRenderDataJob : IJob {
  public NativeArray<float4x2> matrices;
  public NativeArray<float4> colors;
  [ReadOnly] public NativeArray<RenderData> renderData;
  public void Execute() {
    for(int i = 0; i < renderData.Length; i++) {
      matrices[i] = new float4x2(renderData[i].transform, renderData[i].uv);
      colors[i] = renderData[i].color;
    }
  }
}