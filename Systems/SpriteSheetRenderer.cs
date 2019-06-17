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
  void ClearMatrixBuffer(int bufferID) {
    if(renderInfos[bufferID].matrixBuffer != null)
      renderInfos[bufferID].matrixBuffer.Release();
  }

  int UpdateBuffers(int renderIndex) {
    ClearMatrixBuffer(renderIndex);
    processable.SetFilter(new SpriteSheetMaterial { material = renderInfos[renderIndex].material });
    NativeArray<RenderData> data = processable.ToComponentDataArray<RenderData>(Allocator.TempJob);
    int instanceCount = data.Length;
    if(instanceCount > 0) {
      NativeArray<float4x2> matrix = new NativeArray<float4x2>(instanceCount, Allocator.TempJob);
      var job = new CalculateMatrixJob() {
        datas = data,
        matrix = matrix
      };
      JobHandle jobHandle = job.Schedule();
      jobHandle.Complete();
      renderInfos[renderIndex].matrixBuffer = new ComputeBuffer(instanceCount, 32);
      renderInfos[renderIndex].matrixBuffer.SetData(job.matrix);
      renderInfos[renderIndex].material.SetBuffer("matrixBuffer", renderInfos[renderIndex].matrixBuffer);
      renderInfos[renderIndex].args[1] = (uint)instanceCount;
      renderInfos[renderIndex].argsBuffer.SetData(renderInfos[renderIndex].args);
      matrix.Dispose();
    }
    data.Dispose();
    return instanceCount;
  }
}

[BurstCompile]
struct CalculateMatrixJob : IJob {
  public NativeArray<float4x2> matrix;
  [ReadOnly] public NativeArray<RenderData> datas;
  public void Execute() {
    for(int i = 0; i < datas.Length; i++)
      matrix[i] = new float4x2(datas[i].transform, datas[i].uv);
  }
}