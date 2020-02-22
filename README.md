# SpriteSheetRenderer
A powerful Unity ECS system to render massive numbers of animated sprites using DynamicBuffers and ComputeBuffer:
##### 1 million animated sprites were rendered at 60fps on a Mid-2015 MacBook Pro.
![N|Solid](https://forum.unity.com/proxy.php?image=https%3A%2F%2Fi.imgur.com%2FzRSWhy0.png&hash=754bc4b4187e2d72ce0eb2c578b996dc)
## C# 4 required

### How to use (SINGLE INSTANTIATE):
* 1- Create the Archetype:

```sh
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
```

* 2- Record and bake this spritesheet(only once)

```sh
SpriteSheetManager.RecordSpriteSheet(sprites, "emoji");
```
* 3- Populate components

```sh
List<IComponentData> components = new List<IComponentData> {
    new Position2D { Value = float2.zero },
    new Scale { Value = 15 },
    new SpriteIndex { Value = UnityEngine.Random.Range(0, maxSprites) },
    new SpriteSheetAnimation { maxSprites = maxSprites, play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, samples = 10 },
    new SpriteSheetColor { color = new float4(color.r, color.g, color.b, color.a) }
};
```

* 4- Instantiate the entity

```sh 
Entity e = SpriteSheetManager.Instantiate(archetype, components, "emoji");
```
 
* Update the entity

```sh 
Entity e = SpriteSheetManager.UpdateEntity(e, new Position2D { Value = float2.zero});
``` 

* Destroy the entity

```sh 
Entity e = SpriteSheetManager.DestroyEntity(e, "emoji");
``` 

### How to use (BULK INSTANTIATE):

* 1- Create the Archetype:

```sh
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
```

* 2- Bulk instantiate entities

```sh
NativeArray<Entity> entities = new NativeArray<Entity>(spriteCount, Allocator.Temp);
eManager.CreateEntity(archetype, entities);
```

* 2- Record and bake this spritesheet(only once)

```sh
SpriteSheetManager.RecordSpriteSheet(sprites, "emoji");
```

* 3- Populate components

```sh
for(int i = 0; i < entities.Length; i++) {
  Entity e = entities[i];
  eManager.SetComponentData(e, new SpriteIndex { Value = 0});
  eManager.SetComponentData(e, new Scale { Value = 10 });
  eManager.SetComponentData(e, new Position2D { Value = RANDOM_VECTOR });
  eManager.SetComponentData(e, new SpriteSheetAnimation { maxSprites = MAX_SPRITES, play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, samples = 10 });
  SpriteSheetColor col = new SpriteSheetColor { color = A_COLOR };
  eManager.SetComponentData(e, col);
  eManager.SetComponentData(e, new BufferHook { bufferID = i, bufferEnityID = DynamicBufferManager.GetEntityBufferID(material) });
  eManager.SetSharedComponentData(e, material);
}
```

#Support
###NaughtyAttributes is an open-source project that I am developing in my free time. If you like it you can support me by donating.

Buy Me A Coffee: https://www.buymeacoffee.com/LRTk8rn
