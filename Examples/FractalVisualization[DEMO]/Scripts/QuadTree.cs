using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class QuadTree {
  float3 bounds;
  private QuadTree[] tree;
  private Entity entity;
  bool splitted = false;
  public QuadTree(float3 bounds, Entity entity) {
    this.bounds = bounds;
    int maxSprites = SpriteSheetCache.GetLength("emoji");
    if(entity == Entity.Null) {
      var color = UnityEngine.Random.ColorHSV(.35f, .85f);
      List<IComponentData> components = new List<IComponentData> {
        new Position2D { Value = bounds.xy },
        new Scale { Value = bounds.z },
        new SpriteIndex { Value = UnityEngine.Random.Range(0, maxSprites) },
        new SpriteSheetAnimation { maxSprites = maxSprites, play = true, repetition = SpriteSheetAnimation.RepetitionType.Loop, samples = 10 },
        new SpriteSheetColor { color = new float4(color.r, color.g, color.b, color.a) }
      };
      this.entity = SpriteSheetManager.Instantiate(Fractalnitializer.archetype, components, "emoji");
    }
    else {
      this.entity = entity;
      UpdateEntityMatrix();
    }
  }

  public bool Intersects(float2 pos) {
    return (
      pos.x >= bounds.x - bounds.z / 2 &&
      pos.x < bounds.x + bounds.z / 2 &&
      pos.y >= bounds.y - bounds.z / 2 &&
      pos.y < bounds.y + bounds.z / 2
    );
  }

  private void UpdateEntityMatrix() {
    SpriteSheetManager.UpdateEntity(entity, new Position2D { Value = bounds.xy });
    SpriteSheetManager.UpdateEntity(entity, new Scale { Value = bounds.z });
  }

  public void Subdivide() {
    tree = new QuadTree[4];
    //Debug.Log(entity);
    tree[0] = new QuadTree(new float3(bounds.x - bounds.z / 4, bounds.y - bounds.z / 4, bounds.z / 2), entity);
    entity = Entity.Null;
    splitted = true;
    tree[1] = new QuadTree(new float3(bounds.x + bounds.z / 4, bounds.y - bounds.z / 4, bounds.z / 2), entity);
    tree[2] = new QuadTree(new float3(bounds.x - bounds.z / 4, bounds.y + bounds.z / 4, bounds.z / 2), entity);
    tree[3] = new QuadTree(new float3(bounds.x + bounds.z / 4, bounds.y + bounds.z / 4, bounds.z / 2), entity);
  }

  public void Insert(float2 pos, bool force = false) {
    if(!splitted && (force || Intersects(pos))) {
      Subdivide();
    }
    else if(splitted) {
      tree[0].Insert(pos, force);
      tree[1].Insert(pos, force);
      tree[2].Insert(pos, force);
      tree[3].Insert(pos, force);
    }
  }

}
