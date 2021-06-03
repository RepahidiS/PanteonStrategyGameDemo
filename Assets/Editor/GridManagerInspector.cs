using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(GridManager))]
public class GridManagerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GridManager gridManager = (GridManager)target;

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Create grid nodes"))
            gridManager.CreatePathNodes();
        EditorGUILayout.EndVertical();
    }
}