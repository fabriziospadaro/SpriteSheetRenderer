using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using UnityEngine.U2D;
namespace ECSSpriteSheetAnimation.Examples {
  public class MakeSpriteEntities : UnityEngine.MonoBehaviour, IConvertGameObjectToEntity {
    [SerializeField]
    int spriteCount = 5000;

    [SerializeField]
    float2 spawnArea = new float2(100, 100);
    [SerializeField]
    float minScale = .25f;
    [SerializeField]
    float maxScale = 2f;

    [SerializeField]
    float minFPS_ = 0.05f;

    [SerializeField]
    float maxFPS_ = 1.5f;

    [SerializeField]
    Material mat_ = null;

    Rect GetSpawnArea() {
      Rect r = new Rect(0, 0, spawnArea.x, spawnArea.y);
      r.center = transform.position;
      return r;
    }

    public void Convert(Entity entity, EntityManager eManager, GameObjectConversionSystem conversionSystem) {
      var archetype = eManager.CreateArchetype(
         typeof(Position2D),
         typeof(Rotation2D),
         typeof(Scale),
         typeof(SpriteSheetAnimation),
         typeof(SpriteSheetMaterial),
         typeof(SpriteSheetColor),
         typeof(UVCell)
      );

      NativeArray<Entity> entities = new NativeArray<Entity>(spriteCount, Allocator.Temp);
      eManager.CreateEntity(archetype, entities);

      int cellCount = CachedUVData.GetCellCount(mat_);

      Random rand = new Random((uint)UnityEngine.Random.Range(0, int.MaxValue));
      Rect area = GetSpawnArea();
      SpriteSheetMaterial material = new SpriteSheetMaterial { material = mat_ };
      var maxFrames = CachedUVData.GetCellCount(mat_);
      for (int i = 0; i < entities.Length; i++) {
        Entity e = entities[i];

        
        Scale scale = new Scale { Value = rand.NextFloat(minScale, maxScale) };
        Position2D pos = new Position2D { Value = rand.NextFloat2(area.min, area.max) };
        int numFrames = rand.NextInt(3, maxFrames / 2);
        int minFrame = rand.NextInt(0, maxFrames - numFrames);
        SpriteSheetAnimation anim = new SpriteSheetAnimation {
          play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, fps = rand.NextFloat(minFPS_,maxFPS_),
          frameMin = minFrame, frameMax = minFrame + numFrames };
        SpriteSheetColor color = UnityEngine.Random.ColorHSV(.35f, .75f);
        UVCell cell = new UVCell { value = rand.NextInt(0, maxFrames) };
        
        eManager.SetComponentData(e, scale);
        eManager.SetComponentData(e, pos);
        eManager.SetComponentData(e, anim);
        eManager.SetComponentData(e, color);
        eManager.SetComponentData(e, cell);
        eManager.SetSharedComponentData(e, material);

      }
    }

    private void OnDrawGizmos() {
      var r = GetSpawnArea();
      Gizmos.color = new Color(0, .35f, .45f, .24f);
      Gizmos.DrawCube(r.center, r.size);
    }

  }
}