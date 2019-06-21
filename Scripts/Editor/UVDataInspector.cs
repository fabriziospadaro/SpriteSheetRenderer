using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UVData))]
public class UVDataInspector : Editor
{
  public override void OnInspectorGUI() {
    base.OnInspectorGUI();

    if( GUILayout.Button("Cache UV data") ) {

    }
  }
}
