using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace ECSSpriteSheetAnimation.Examples
{
    public class MakeSpriteEntities : UnityEngine.MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        int spriteCount_ = 5000;

        [SerializeField]
        Material mat_ = null;

        [SerializeField]
        float2 spawnArea_ = new float2(100, 100);

        [SerializeField]
        float minScale_ = .25f;

        [SerializeField]
        float maxScale_ = 2f;

        Rect GetSpawnArea()
        {
            Rect r = new Rect(0, 0, spawnArea_.x, spawnArea_.y);
            r.center = transform.position;
            return r;
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var arch = dstManager.CreateArchetype(
                  typeof(Position2D),
                  typeof(Rotation2D),
                  typeof(Scale),
                  typeof(Bound2D),
                  typeof(SpriteSheet),
                  typeof(SpriteSheetAnimation),
                  typeof(SpriteSheetMaterial),
                  typeof(UvBuffer),
                  typeof(SpriteColor)
                );

            NativeArray<Entity> entities = new NativeArray<Entity>(spriteCount_, Allocator.Temp);
            dstManager.CreateEntity(arch, entities);

            float4[] uvs = SpriteSheetCache.BakeUv(mat_);
            int cellCount = uvs.Length;

            Random rand = new Random((uint)UnityEngine.Random.Range(0, int.MaxValue));
            Rect area = GetSpawnArea();
            SpriteSheetMaterial mat = new SpriteSheetMaterial { material = mat_ };

            for (int i = 0; i < entities.Length; i++)
            {
                Entity e = entities[i];

                SpriteSheet          sheet = new SpriteSheet { spriteIndex = rand.NextInt(0, cellCount), maxSprites = cellCount };
                Scale                scale = new Scale { Value = rand.NextFloat(minScale_, maxScale_) };
                Position2D           pos   = new Position2D { Value = rand.NextFloat2(area.min, area.max) };
                SpriteSheetAnimation anim = new SpriteSheetAnimation { play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, samples = 10 };
                SpriteColor          col = UnityEngine.Random.ColorHSV(.15f, .75f);

                dstManager.SetComponentData(e, sheet);
                dstManager.SetComponentData(e, scale);
                dstManager.SetComponentData(e, pos);
                dstManager.SetComponentData(e, anim);
                dstManager.SetComponentData(e, col);
                dstManager.SetSharedComponentData(e, mat);

                // Fill uv buffer
                var buffer = dstManager.GetBuffer<UvBuffer>(entities[i]);
                for (int j = 0; j < uvs.Length; j++)
                    buffer.Add(uvs[j]);

            }
        }

        private void OnDrawGizmosSelected()
        {
            var r = GetSpawnArea();
            Gizmos.color = new Color(0, .35f, .45f, .24f);
            Gizmos.DrawCube(r.center, r.size);
        }

    }
}