using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
public static class SpriteSheetCache {
  private static Dictionary<string, KeyValuePair<Material, int>> materialNameMaterial = new Dictionary<string, KeyValuePair<Material, int>>();
  private static Dictionary<Material, string> materialToName = new Dictionary<Material, string>();
  public static KeyValuePair<Material, float4[]> BakeSprites(Sprite[] sprites, string materialName) {
    Material material = new Material(Shader.Find("Instanced/SpriteSheet"));
    Texture texture = sprites[0].texture;
    material.mainTexture = texture;
    if(sprites.Length == 1) {
      return new KeyValuePair<Material, float4[]>(material, new float4[1] { new float4(1, 1, 0, 0) });
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
      materialNameMaterial.Add(materialName, new KeyValuePair<Material, int>(material, sprites.Length));
      materialToName.Add(material, materialName);
      return new KeyValuePair<Material, float4[]>(material, uvs);
    }
  }
  public static int GetLength(string spriteSheetName) => materialNameMaterial[spriteSheetName].Value;
  public static Material GetMaterial(string spriteSheetName) => materialNameMaterial[spriteSheetName].Key;
  public static string GetMaterialName(Material material) => materialToName[material];
  public static int GetLenght(Material material) => GetLength(GetMaterialName(material));

}
