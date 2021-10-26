using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct SpriteSheetCommand{
  public EntityManager entityManager;
  public void RequestDestruction(Entity entity) {
    entityManager.AddComponent<RequireDestroyComponent>(entity);
  }

  public void DestroySpriteSheet(Entity bufferEntity, Entity entity, int bufferID) {
    entityManager.DestroyEntity(entity);
    var idsBuffers = entityManager.GetBuffer<IdsBuffer>(bufferEntity);
    int removeAt = 0;
    for(int i = 0; i < idsBuffers.Length; i++) {
      if(idsBuffers[i] == bufferID) {
        removeAt = i;
        break;
      }
    }

    entityManager.GetBuffer<MatrixBuffer>(bufferEntity).RemoveAt(removeAt);
    entityManager.GetBuffer<SpriteColorBuffer>(bufferEntity).RemoveAt(removeAt);
    entityManager.GetBuffer<SpriteIndexBuffer>(bufferEntity).RemoveAt(removeAt);
    idsBuffers.RemoveAt(removeAt);
    //entityManager.GetBuffer<SpriteIndexBuffer>(bufferEntity).Insert(bufferID, new SpriteIndexBuffer { index = -1 });
    //entityManager.GetBuffer<MatrixBuffer>(bufferEntity).Insert(bufferID, new MatrixBuffer());
    //entityManager.GetBuffer<SpriteColorBuffer>(bufferEntity).Insert(bufferID, new SpriteColorBuffer());
  }
}
