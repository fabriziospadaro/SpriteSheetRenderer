using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECSSpriteSheetAnimation.Examples
{
    public class SingleEntityDestroy : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        private string spriteSheetName = "emoji";

        public Sprite[] sprites;
        private EntityArchetype archetype;

        public void Convert(Entity entity, EntityManager eManager, GameObjectConversionSystem conversionSystem)
        {
            //Record and bake this spritesheets(only once)
            archetype = eManager.CreateArchetype(
                typeof(Position2D),
                typeof(Rotation2D),
                typeof(Scale),
                typeof(LifeTime),
                //required params
                typeof(SpriteIndex),
                typeof(SpriteSheetAnimation),
                typeof(SpriteSheetMaterial),
                typeof(SpriteSheetColor),
                typeof(SpriteMatrix),
                typeof(BufferHook)
            );
            SpriteSheetManager.RecordSpriteSheet(sprites, spriteSheetName);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                int maxSprites = SpriteSheetCache.GetLength(spriteSheetName);
                var color = UnityEngine.Random.ColorHSV(.35f, .85f);

                // 3) Populate components
                List<IComponentData> components = new List<IComponentData> {
                new Position2D { Value = UnityEngine.Random.insideUnitCircle * 7 },
                new Scale { Value = UnityEngine.Random.Range(0,3f) },
                new SpriteIndex { Value = UnityEngine.Random.Range(0, maxSprites) },
                new SpriteSheetAnimation { maxSprites = maxSprites, play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, samples = 10 },
                new SpriteSheetColor { color = new float4(color.r, color.g, color.b, color.a) },
                new LifeTime{ Value = UnityEngine.Random.Range(5,15)}
            };

                SpriteSheetManager.Instantiate(archetype, components, spriteSheetName);
            }
        }
    } 
}
