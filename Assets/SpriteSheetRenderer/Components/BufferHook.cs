using Unity.Entities;
public struct BufferHook : IComponentData {
  public int bufferID;
  public int bufferEntityID;
}