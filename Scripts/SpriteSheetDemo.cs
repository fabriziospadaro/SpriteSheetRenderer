using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SpriteSheetDemo : MonoBehaviour {
  public Material material;
  private EntityArchetype spriteSheetArchetype;
  // Start is called before the first frame update
  void Start() {
    EntityManager entityManager = World.Active.EntityManager;
    spriteSheetArchetype = entityManager.CreateArchetype(
      typeof(Position2D),
      typeof(Rotation2D),
      typeof(Scale),
      typeof(Bound2D),
      typeof(SpriteSheet),
      typeof(SpriteSheetAnimation),
      typeof(SpriteSheetMaterial)
    );
    //4.5 ms vecchio sistema con 5k di unità
    NativeArray<Entity> entities = new NativeArray<Entity>(200000, Allocator.Temp);
    entityManager.CreateEntity(spriteSheetArchetype, entities);
    float2[] cameraBound = Bound2DExtension.BoundValuesFromCamera(Camera.main);
    for(int i = 0; i < entities.Length; i++) {
      float2 position = cameraBound[0] + new float2(UnityEngine.Random.Range(-cameraBound[1].x / 2, cameraBound[1].x / 2), UnityEngine.Random.Range(-cameraBound[1].y / 2, cameraBound[1].y / 2));
      entityManager.SetComponentData(entities[i], new Position2D { Value = position });
      entityManager.SetComponentData(entities[i], new Scale { Value = UnityEngine.Random.Range(0.1f, 1f) });
      entityManager.SetComponentData(entities[i], new SpriteSheet { spriteIndex = UnityEngine.Random.Range(0, 16), cell = new int2(4, 4) });
      entityManager.SetComponentData(entities[i], new SpriteSheetAnimation { play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, samples = 10 });
      entityManager.SetSharedComponentData(entities[i], new SpriteSheetMaterial { material = material });
    }
    entities.Dispose();
  }
}
