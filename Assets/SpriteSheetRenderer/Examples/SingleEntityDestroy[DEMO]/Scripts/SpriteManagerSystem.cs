using Unity.Jobs;
using Unity.Entities;
public class SpriteManagerSystem : SystemBase {
  SpriteSheetCommand spriteCommand;

  protected override void OnCreate(){
    base.OnCreate();
    spriteCommand = new SpriteSheetCommand { entityManager = EntityManager };
  }

  protected override void OnUpdate() {
    float t = Time.DeltaTime;
    
    Entities.WithName("LifeTimeSystem").WithAll<LifeTime>().WithNone<RequireDestroyComponent>().ForEach((Entity entity, ref LifeTime lifetime) => {
      if(lifetime.Value < 0)
        spriteCommand.RequestDestruction(entity);
      else 
        lifetime.Value -= t;
    }).WithStructuralChanges().Run(); 
  }
}
