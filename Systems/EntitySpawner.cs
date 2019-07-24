using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using ECSSpriteSheetAnimation.Examples;
using Unity.Collections;
public class EntitySpawner : ComponentSystem {
  bool blocked = false;
  protected override void OnUpdate() {
    if(Input.GetKey(KeyCode.Space)) {
      MakeSpriteEntities.SpawnEntity(PostUpdateCommands, "emoji");
    }
    if(Input.GetKey(KeyCode.U)) {
      blocked = false;
    }
    Entities.WithAll<SpriteSheetAnimation>().ForEach((Entity e, ref SpriteSheetAnimation anim) => {
      if(Input.GetKeyUp(KeyCode.R)) {
        blocked = true;
        MakeSpriteEntities.DestroyEntity(PostUpdateCommands, e, "emoji");
        return;
      }
    });
  }
}
