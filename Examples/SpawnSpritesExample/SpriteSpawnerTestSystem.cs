using ECSSpriteSheetAnimation.Examples;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class SpriteMakerSystem : ComponentSystem {

  EntityQuery spawnerQuery;

  void DoSpawn(SpriteSpawnerTest.SpawnData spawner, Material mat) {
    ComponentType[] type = new ComponentType[]
    {
        typeof(Position2D),
        typeof(Rotation2D),
        typeof(Scale),
        typeof(SpriteSheetAnimation),
        typeof(SpriteSheetMaterial),
        typeof(SpriteSheetColor),
        typeof(UVCell)
    };

    var em = EntityManager;
    var archetype = em.CreateArchetype(type);

    NativeArray<Entity> entities = new NativeArray<Entity>(spawner.spriteCount, Allocator.Temp);
    em.CreateEntity(archetype, entities);

    int cellCount = CachedUVData.GetCellCount(mat);

    Random rand = new Random((uint)UnityEngine.Random.Range(0, int.MaxValue));
    Rect area = spawner.GetSpawnArea();
    SpriteSheetMaterial material = new SpriteSheetMaterial { material = mat };
    var maxFrames = CachedUVData.GetCellCount(mat);
    for (int i = 0; i < entities.Length; i++) {
      Entity e = entities[i];


      Scale scale = new Scale { Value = rand.NextFloat(spawner.minScale, spawner.maxScale) };
      float2 p = rand.NextFloat2(area.min, area.max);
      

      Position2D pos = spawner.origin.xy + p;
      Rotation2D rot = new Rotation2D { angle = spawner.rotation };

      int numFrames = rand.NextInt(3, maxFrames / 2);
      int minFrame = rand.NextInt(0, maxFrames - numFrames);
      SpriteSheetAnimation anim = new SpriteSheetAnimation {
        play = true,
        repetition = SpriteSheetAnimation.RepetitionType.Loop,
        fps = rand.NextFloat(spawner.minFPS_, spawner.maxFPS_),
        frameMin = minFrame,
        frameMax = minFrame + numFrames
      };
      SpriteSheetColor color = UnityEngine.Random.ColorHSV(.35f, .75f);
      UVCell cell = new UVCell { value = rand.NextInt(0, maxFrames) };

      em.SetComponentData(e, scale);
      em.SetComponentData(e, pos);
      em.SetComponentData(e, anim);
      em.SetComponentData(e, color);
      em.SetComponentData(e, cell);
      em.SetComponentData(e, rot);
      em.SetSharedComponentData(e, material);
    }
  }

  protected override void OnCreate() {
    base.OnCreate();
    spawnerQuery = GetEntityQuery(typeof(SpriteSpawnerTest.SpawnData));
  }

  protected override void OnUpdate() {
    var spawners = spawnerQuery.ToEntityArray(Allocator.TempJob);
    var e = spawners[0];

    var mat = EntityManager.GetSharedComponentData<SpriteSheetMaterial>(e).material;
    var spawnData = EntityManager.GetComponentData<SpriteSpawnerTest.SpawnData>(e);

    var entities = EntityManager.GetAllEntities(Allocator.TempJob);
    EntityManager.DestroyEntity(entities);

    DoSpawn(spawnData, mat);

    entities.Dispose();
    spawners.Dispose();
  }
}
