# SpriteSheetRenderer
A powerful Unity ECS system to render massive numbers of animated sprites:
##### 200k entities were rendered at 80 fps on a Mid-2015 MacBook Pro.
![N|Solid](https://i.imgur.com/Jqh9FQs.png)
### How to use:
* 1- Create the Archetype:

```sh
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
```

* 2- Spawn a HUUUUGE number of entities:

```sh
NativeArray<Entity> entities = new NativeArray<Entity>(500000, Allocator.Temp);
entityManager.CreateEntity(spriteSheetArchetype, entities);
```

* 3- Fill the values of each entity:

```sh
for(int i = 0; i < entities.Length; i++) {
  float2 position = A_RANDOM_POSITION;
  entityManager.SetComponentData(entities[i], new Position2D { Value = position });
  entityManager.SetComponentData(entities[i], new Scale { Value = UnityEngine.Random.Range(0.1f, 1f) });
  entityManager.SetComponentData(entities[i], new SpriteSheet { spriteIndex = UnityEngine.Random.Range(0, 16), cell = new int2(4, 4) });
  entityManager.SetComponentData(entities[i], new SpriteSheetAnimation { play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, samples = 10 });
  entityManager.SetSharedComponentData(entities[i], new SpriteSheetMaterial { material = material });
}
entities.Dispose();
```
