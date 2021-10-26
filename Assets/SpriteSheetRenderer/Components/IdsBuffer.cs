using Unity.Entities;

[InternalBufferCapacity(sizeof(int))]
public struct IdsBuffer : IBufferElementData {
  public static implicit operator int(IdsBuffer e) { return e.value; }
  public static implicit operator IdsBuffer(int e) { return new IdsBuffer { value = e }; }
  public int value;
}
