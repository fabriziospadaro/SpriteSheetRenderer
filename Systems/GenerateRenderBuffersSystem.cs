using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

/// <summary>
/// Generates our render buffer entities. We generate one entity
/// for each material (Shared Component Data). Ensures the render
/// buffer entities stays in sync with our materials.
/// </summary>
[AlwaysUpdateSystem]
public class GenerateRenderBuffersSystem : ComponentSystem {
  EntityArchetype bufferArchetype;
  EntityQuery bufferQuery;
  
  static List<SpriteSheetMaterial> sharedMaterials = new List<SpriteSheetMaterial>();

  protected override void OnCreate() {
    bufferQuery = GetEntityQuery(typeof(RenderBufferTag), typeof(SpriteSheetMaterial));
    bufferArchetype = EntityManager.CreateArchetype(typeof(RenderBufferTag), typeof(SpriteSheetMaterial));
  }

  protected override void OnUpdate() {
    sharedMaterials.Clear();
    EntityManager.GetAllUniqueSharedComponentData(sharedMaterials);
    // Ignore default ( null material )
    sharedMaterials.RemoveAt(0);

    int bufferCount = bufferQuery.CalculateLength();

    NativeArray<Entity> buffers;
    if( bufferCount != sharedMaterials.Count ) {
      EntityManager.DestroyEntity(bufferQuery);
      buffers = new NativeArray<Entity>(sharedMaterials.Count, Allocator.TempJob);
      EntityManager.CreateEntity(bufferArchetype, buffers);
    } else {
      buffers = bufferQuery.ToEntityArray(Allocator.TempJob);
    }

    for( int i = 0; i < buffers.Length; ++i ) {
      var bufferMat = EntityManager.GetSharedComponentData<SpriteSheetMaterial>(buffers[i]);
      EntityManager.SetSharedComponentData(buffers[i], sharedMaterials[i]);
    }

    buffers.Dispose();
  }
}
