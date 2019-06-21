using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Maintains a runtime cache of the uvs for a given material.
/// The sprites need to be set up with unity's sprite asset editor.
/// The textures attached to any passed in materials need to be 
/// in the root of a Resources folder.
/// TODO: Different way to access the texture that doesn't use the 
/// dumb Resources folder. Addressables maybe?
/// </summary>
public class CachedUVData
{
  Texture targetTexture = null;
  List<float2> uvs = new List<float2>();

  CachedUVData() { }
  CachedUVData(Texture tex) {
    BuildFromTexture(tex);
  }

  public Texture TargetTexture_ => targetTexture;

  void BuildFromTexture(Texture tex) {
    if (tex == null || tex.width == 0 || tex.height == 0) {
      Debug.LogWarning("You're passing an invalid texture to CachedUVData!");
    }
    targetTexture = tex;
    // Note - Texture MUST be in a "Resources" folder - path is relative to that
    var sprites = Resources.LoadAll<Sprite>(tex.name);
    uvs.Clear();
    for (int i = 0; i < sprites.Length; i++) {
      foreach (var uv in sprites[i].uv)
        uvs.Add(uv);
    }
  }

  static Dictionary<Material, CachedUVData> uvData = new Dictionary<Material, CachedUVData>();
  
  static CachedUVData GetCachedUVData(Material mat) {
    CachedUVData data;
    if (!uvData.TryGetValue(mat, out data)) 
      uvData[mat] = data = new CachedUVData(mat.mainTexture);

    if (mat.mainTexture != data.TargetTexture_)
      data.BuildFromTexture(mat.mainTexture);

    return data;
  }

  /// <summary>
  /// Constructs a Compute Buffer populated with the cached uv data for the given material.
  /// </summary>
  public static ComputeBuffer GetUVBuffer(Material mat) {
    var uvs = GetCachedUVData(mat).uvs;

    var buffer = new ComputeBuffer(uvs.Count, sizeof(float) * 2);
    buffer.SetData(uvs);
    return buffer;
  }

  public static int GetCellCount(Material mat) {
    var data = GetCachedUVData(mat);
    return data.uvs.Count == 0 ? 0 : data.uvs.Count / 4;
  }

}
