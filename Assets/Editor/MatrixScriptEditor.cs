using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CustomEditor(typeof(MatrixScript))]
public class MatrixScriptEditor : Editor {
#if UNITY_EDITOR
	public override void OnInspectorGUI() {
		var matrixScript = (MatrixScript)target;

		var tempSize = matrixScript.size;

		serializedObject.Update();

		if (matrixScript.ItemList == null || matrixScript.ColumnPrefab == null || matrixScript.RowPrefab == null)
			EditorGUILayout.HelpBox("Please set all the variables before proceeding", MessageType.Error);
		else {
			EditorGUILayout.PropertyField(serializedObject.FindProperty("size"), true);

			serializedObject.ApplyModifiedProperties();

			if (tempSize != matrixScript.size) {
				matrixScript.ResizeMatrix();

				matrixScript.DrawMatrix();
				return;
			}

			if (matrixScript.size.x > 1 && matrixScript.size.y > 1) {
				if (matrixScript.Matrix.Count != matrixScript.size.x || matrixScript.Matrix[0].Count != matrixScript.size.y)
					matrixScript.ResizeMatrix();

				bool redraw = false;

				for (int i = 0; i < matrixScript.size.x; i++) {
					EditorGUILayout.BeginHorizontal();

					for (int o = 0; o < matrixScript.size.y; o++) {
						var temp = matrixScript.Matrix[i][o];

						matrixScript.Matrix[i][o] = EditorGUILayout.TextField(matrixScript.Matrix[i][o]);

						if (matrixScript.Matrix[i][o] != temp)
							redraw = true;
					}

					EditorGUILayout.EndHorizontal();
				}

				if (redraw)
					matrixScript.DrawMatrix();
			} else {
				EditorGUILayout.HelpBox(MatrixInputScript.MatrixSizeError, MessageType.Error);
			}
		}

		EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemList"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("ColumnPrefab"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("RowPrefab"));

		serializedObject.ApplyModifiedProperties();
	}
#endif
}
