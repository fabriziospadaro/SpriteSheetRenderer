using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu( fileName ="UV Data", menuName ="DotsSpriteSheet/UVData", order = 1)]
public class UVData : ScriptableObject
{
  [SerializeField]
  Sprite[] sprites_;

  public IEnumerable<float2> uvs_;
  

}
