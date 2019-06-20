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
      typeof(SpriteSheetMaterial),
      typeof(UvBuffer)
    );

    NativeArray<Entity> entities = new NativeArray<Entity>(200000, Allocator.Temp);
    entityManager.CreateEntity(spriteSheetArchetype, entities);
    float2[] cameraBound = Bound2DExtension.BoundValuesFromCamera(Camera.main);
    //bake the uvs
    float4[] uvs = SpriteSheetCache.BakeUv(material);
    for(int i = 0; i < entities.Length; i++) {
      float2 position = cameraBound[0] + new float2(UnityEngine.Random.Range(-cameraBound[1].x / 2, cameraBound[1].x / 2), UnityEngine.Random.Range(-cameraBound[1].y / 2, cameraBound[1].y / 2));
      entityManager.SetComponentData(entities[i], new Position2D { Value = position });
      entityManager.SetComponentData(entities[i], new Scale { Value = 1 });
      entityManager.SetComponentData(entities[i], new SpriteSheet { spriteIndex = UnityEngine.Random.Range(0, uvs.Length), maxSprites = uvs.Length });
      entityManager.SetComponentData(entities[i], new SpriteSheetAnimation { play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, samples = 10 });
      entityManager.SetSharedComponentData(entities[i], new SpriteSheetMaterial { material = material });
      //store the uvs into a dynamic buffer
      var lookup = entityManager.GetBuffer<UvBuffer>(entities[i]);
      for(int j = 0; j < uvs.Length; j++)
        lookup.Add(new UvBuffer { uv = uvs[j] });
    }
    entities.Dispose();
  }
}
