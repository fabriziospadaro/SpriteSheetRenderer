using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECSSpriteSheetAnimation.Examples
{
    public class FractalInitializer : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        private string spriteSheetName = "emoji";

        public Sprite[] sprites;
        public static EntityArchetype archetype;
        public static QuadTree qt;

        public void Convert(Entity entity, EntityManager eManager, GameObjectConversionSystem conversionSystem)
        {
            archetype = eManager.CreateArchetype(
                typeof(Position2D),
                typeof(Rotation2D),
                typeof(Scale),
                typeof(SpriteSheetColor),
            #region required for spritesheet renderer
            typeof(SpriteIndex),
                typeof(SpriteSheetAnimation),
                typeof(SpriteSheetMaterial),
                typeof(SpriteMatrix),
                typeof(BufferHook)
            #endregion
        );
            SpriteSheetManager.RecordSpriteSheet(sprites, spriteSheetName);
            qt = new QuadTree(new float3(0, 0, 20), Entity.Null);
        }
    } 
}
