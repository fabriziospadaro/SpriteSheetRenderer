using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class DynamicAnimationsDemo: MonoBehaviour, IConvertGameObjectToEntity {
  public SpriteSheetAnimator animator;
  public static Entity character;

  public void Convert(Entity entity, EntityManager eManager, GameObjectConversionSystem conversionSystem){
    // 1) Create Archetype
    EntityArchetype archetype = eManager.CreateArchetype(
             typeof(Position2D),
             typeof(Rotation2D),
             typeof(Scale),
             //required params
             typeof(SpriteIndex),
             typeof(SpriteSheetAnimation),
             typeof(SpriteSheetMaterial),
             typeof(SpriteSheetColor),
             typeof(SpriteMatrix),
             typeof(BufferHook)
    );

    SpriteSheetManager.RecordAnimator(animator);

    var color = Color.white;

    // 3) Populate components
    List<IComponentData> components = new List<IComponentData> {
        new Position2D { Value = float2.zero },
        new Scale { Value = 5 },
        new SpriteSheetColor { color = new float4(color.r, color.g, color.b, color.a) }
    };
    // 4) Instantiate the entity
    character = SpriteSheetManager.Instantiate(archetype, components, animator);
  }
}
