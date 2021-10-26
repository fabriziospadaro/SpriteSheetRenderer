using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SpriteSheetCleanerSystem: SystemBase {
  EntityQuery query;
  SpriteSheetCommand spriteCommand;

  protected override void OnCreate(){
    base.OnCreate();
    query = GetEntityQuery(ComponentType.ReadOnly<EntityIDComponent>());
    spriteCommand = new SpriteSheetCommand { entityManager = EntityManager };

  }
  protected override void OnUpdate(){
    var bufferIds = query.ToComponentDataArray<EntityIDComponent>(Unity.Collections.Allocator.TempJob);
    var bufferEntities = query.ToEntityArray(Unity.Collections.Allocator.TempJob);
    Entity bufferEntity = default;

    Entities.WithName("LifeTimeSystem").WithAll<RequireDestroyComponent>().WithReadOnly(bufferEntities).WithReadOnly(bufferIds).ForEach((Entity entity, in BufferHook hook) => {
      for(int i = 0; i < bufferIds.Length; i++)
        if(bufferIds[i].id == hook.bufferEntityID) {
          bufferEntity = bufferEntities[i];
          break;
        }
      spriteCommand.DestroySpriteSheet(bufferEntity, entity, hook.bufferID);
    }).WithDisposeOnCompletion(bufferIds).WithDisposeOnCompletion(bufferEntities).WithStructuralChanges().Run();
  }
}
