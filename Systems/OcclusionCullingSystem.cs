using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public class OcclusionCullingSystem : JobComponentSystem {

  EntityCommandBufferSystem barrier;

  protected override void OnCreate() {
    barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
  }
  //[BurstCompile] one day they will have this ***** working 
  struct OcclusionCullingJob : IJobForEachWithEntity<Bound2D> {
    [ReadOnly] public float2 position;
    [ReadOnly] public float2 scale;
    [ReadOnly] public EntityCommandBuffer.Concurrent buffer;
    public void Execute(Entity entity, int index, ref Bound2D Ebound) {
      if(Bound2DExtension.Intersects(Ebound.position, Ebound.scale, position, scale)) {
        if(!Ebound.visibile) {
          buffer.AddComponent(index, entity, new RenderData { });
          Ebound.visibile = true;
        }
      }
      else if(Ebound.visibile) {
        buffer.RemoveComponent<RenderData>(index, entity);
        Ebound.visibile = false;
      }
    }
  }

  protected override JobHandle OnUpdate(JobHandle inputDeps) {
    float2[] datas = Bound2DExtension.BoundValuesFromCamera(Camera.main);
    var job = new OcclusionCullingJob() {
      position = datas[0], scale = datas[1], buffer = barrier.CreateCommandBuffer().ToConcurrent()
    }.Schedule(this, inputDeps);
    barrier.AddJobHandleForProducer(job);
    return job;
  }
}


