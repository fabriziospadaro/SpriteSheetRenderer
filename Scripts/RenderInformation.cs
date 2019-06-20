using UnityEngine;

class RenderInformation {
  public ComputeBuffer matrixBuffer;
  public ComputeBuffer argsBuffer;
  public ComputeBuffer colorsBuffer;
  public Material material;
  public uint[] args;

  public RenderInformation(Material material) {
    this.material = material;
    args = new uint[5] { 0, 0, 0, 0, 0 };
    argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
    //thoose args are always the same since we always use the same mesh
    args[0] = (uint)6;
    args[2] = args[3] = (uint)0;
  }

  public void DestroyBuffers() {
    if(matrixBuffer != null)
      matrixBuffer.Release();
    matrixBuffer = null;

    if(argsBuffer != null)
      argsBuffer.Release();
    argsBuffer = null;

    if (colorsBuffer != null)
        colorsBuffer.Release();
    colorsBuffer = null;
  }
}
