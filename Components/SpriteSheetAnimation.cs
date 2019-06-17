using Unity.Entities;

public struct SpriteSheetAnimation : IComponentData {
  public enum RepetitionType { Once, Loop, PingPong }
  public RepetitionType repetition;
  public int elapsedFrames;
  //how many frames does this animation takes to move to the next sprite
  public int samples;
  public bool play;
}
