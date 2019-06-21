using Unity.Entities;

public struct SpriteSheetAnimation : IComponentData {
  public enum RepetitionType { Once, Loop, PingPong }
  public RepetitionType repetition;
  public int frameMin;
  public int frameMax;
  public float fps;
  public bool play;
  public float elapsed;
}
