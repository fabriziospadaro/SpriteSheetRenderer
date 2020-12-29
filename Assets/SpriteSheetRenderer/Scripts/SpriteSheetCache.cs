using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public static class SpriteSheetCache
{
    private static Dictionary<string, KeyValuePair<Material, int>> materialNameMaterial = new Dictionary<string, KeyValuePair<Material, int>>();
    private static Dictionary<Material, string> materialToName = new Dictionary<Material, string>();
    public static Dictionary<Entity, SpriteSheetAnimator> entityAnimator = new Dictionary<Entity, SpriteSheetAnimator>();

    public static KeyValuePair<Material, float4[]> BakeSprites(Sprite[] sprites, string materialName)
    {
        Material material = new Material(Shader.Find("Instanced/SpriteSheet"));
        Texture texture = sprites[0].texture;
        material.mainTexture = texture;

        float w = texture.width;
        float h = texture.height;
        float4[] uvs = new float4[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            var s = sprites[i];
            float tilingX = 1f * (s.rect.width / w);
            float tilingY = 1f * (s.rect.height / h);
            float OffsetX = (s.rect.x / w);
            float OffsetY = (s.rect.y / h);
            uvs[i].x = tilingX;
            uvs[i].y = tilingY;
            uvs[i].z = OffsetX;
            uvs[i].w = OffsetY;
        }
        materialNameMaterial.Add(materialName, new KeyValuePair<Material, int>(material, sprites.Length));
        materialToName.Add(material, materialName);
        return new KeyValuePair<Material, float4[]>(material, uvs);
    }

    public static int TotalLength() => materialNameMaterial.Count;

    public static int GetLength(string spriteSheetName) => materialNameMaterial[spriteSheetName].Value;
    public static Material GetMaterial(string spriteSheetName) => materialNameMaterial[spriteSheetName].Key;

    public static SpriteSheetAnimator GetAnimator(Entity e) => entityAnimator[e];
    public static string GetMaterialName(Material material) => materialToName[material];
    public static int GetLenght(Material material) => GetLength(GetMaterialName(material));

}
