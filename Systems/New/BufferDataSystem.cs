using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

public abstract class BufferDataSystem<BufferDataT> : JobComponentSystem
  where BufferDataT : struct, IBufferElementData {

  static List<SpriteSheetMaterial> sharedMaterials = new List<SpriteSheetMaterial>();

  EntityQuery uninitializedBuffers;
  EntityQuery initializedBuffers;

  /// <summary>
  /// Derived classes should schedule a job to populate the buffer.
  /// The buffer is only guaranteed to be initialized - early outs or resizing can be handled at the call site or
  /// from the job.
  /// </summary>
  protected abstract JobHandle PopulateBuffer(Entity bufferEntity, SpriteSheetMaterial filterMat, JobHandle inputDeps);

  protected override void OnCreate() {
    uninitializedBuffers = GetEntityQuery(
      ComponentType.ReadOnly<RenderBufferTag>(),
      ComponentType.ReadOnly<SpriteSheetMaterial>(),
      ComponentType.Exclude<BufferDataT>());
    initializedBuffers = GetEntityQuery(
      ComponentType.ReadOnly<RenderBufferTag>(),
      ComponentType.ReadOnly<SpriteSheetMaterial>(),
      ComponentType.ReadWrite<BufferDataT>());
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    // Ensure our buffer entities have their dynamic buffers attached
    using (var entities = uninitializedBuffers.ToEntityArray(Allocator.TempJob)) {
      for (int i = 0; i < entities.Length; ++i)
        EntityManager.AddBuffer<BufferDataT>(entities[i]);
    }
    
    if (initializedBuffers.CalculateLength() == 0)
      return inputDeps;

    sharedMaterials.Clear();
    EntityManager.GetAllUniqueSharedComponentData(sharedMaterials);
    // Ignore default (null material)
    sharedMaterials.RemoveAt(0);

    foreach (var mat in sharedMaterials) {
      initializedBuffers.SetFilter(mat);
      var bufferEntity = initializedBuffers.GetSingletonEntity();

      inputDeps = PopulateBuffer(bufferEntity, mat, inputDeps);
    }

    return inputDeps;
  }
}
