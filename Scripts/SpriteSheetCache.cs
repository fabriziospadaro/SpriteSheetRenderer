using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public static class SpriteSheetCache {
  public static float4[] BakeUv(Material material) {
    Texture texture = material.mainTexture;
    string spriteSheet = AssetDatabase.GetAssetPath(material.mainTexture);
    Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheet).OfType<Sprite>().ToArray();
    if(sprites.Length == 0) {
      return new float4[1] { new float4(1, 1, 0, 0) };
    }
    else {
      float w = texture.width;
      float h = texture.height;
      float4[] uvs = new float4[sprites.Length];
      int i = 0;
      foreach(Sprite s in sprites) {
        float tilingX = 1f / (w / s.rect.width);
        float tilingY = 1f / (h / s.rect.height);
        float OffsetX = tilingX * (s.rect.x / s.rect.width);
        float OffsetY = tilingY * (s.rect.y / s.rect.height);
        uvs[i].x = tilingX;
        uvs[i].y = tilingY;
        uvs[i].z = OffsetX;
        uvs[i].w = OffsetY;
        i++;
      }
      return uvs;
    }
  }
}
