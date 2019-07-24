# SpriteSheetRenderer
A powerful Unity ECS system to render massive numbers of animated sprites using DynamicBuffers and ComputeBuffer:
##### 800k sprites were rendered at 75fps on a Mid-2015 MacBook Pro.
![N|Solid](https://forum.unity.com/proxy.php?image=https%3A%2F%2Fimgur.com%2F9hKKxst.png&hash=0be52246a477619ac78271704a0597f6)
### How to use:
* 1- Create the Archetype:

```sh
EntityManager archetype = eManager.CreateArchetype(
  typeof(Position2D),
  typeof(Rotation2D),
  typeof(Scale),
  typeof(SpriteIndex),
  typeof(SpriteSheetAnimation),
  typeof(SpriteSheetMaterial),
  typeof(SpriteSheetColor),
  typeof(SpriteMatrix),
  typeof(BufferHook)
);
```

* 2- Spawn a HUUUUGE number of entities:

```sh
NativeArray<Entity> entities = new NativeArray<Entity>(800000, Allocator.Temp);
entityManager.CreateEntity(archetype, entities);
```
* 3- Bake and create dynamic buffers

```sh
//only needed for first time material baking
KeyValuePair<Material, float4[]> atlasData = SpriteSheetCache.BakeSprites(sprites, "emoji");
SpriteSheetMaterial material = new SpriteSheetMaterial { material = atlasData.Key };

DynamicBufferManager.manager = eManager;
DynamicBufferManager.GenerateBuffers(material, entities.Length);
DynamicBufferManager.BakeUvBuffer(material, atlasData);
```

* 4- Fill the values of each entity:

```sh for(int i = 0; i < entities.Length; i++) {
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
