using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CustomEditor(typeof(MatrixInputScript))]
public class MatrixInputScriptEditor : Editor {
#if UNITY_EDITOR
	public override void OnInspectorGUI() {
		var matrixScript = (MatrixInputScript)target;

		var tempSize = matrixScript.size;

		serializedObject.Update();

		if (matrixScript.ItemList == null || matrixScript.ColumnPrefab == null || matrixScript.RowPrefab == null || matrixScript.SaveButton == null || matrixScript.NInput == null || matrixScript.MInput == null)
			EditorGUILayout.HelpBox("Please set all the variables before proceeding", MessageType.Error);
		else {
			EditorGUILayout.PropertyField(serializedObject.FindProperty("size"), true);

			serializedObject.ApplyModifiedProperties();

			if (tempSize != matrixScript.size) {
				UtilManager.UpdateInputFieldSilent(matrixScript.NInput, matrixScript.size.x.ToString());
				UtilManager.UpdateInputFieldSilent(matrixScript.MInput, matrixScript.size.y.ToString());

				if (tempSize.x != matrixScript.size.x)
					matrixScript.SetX(matrixScript.size.x.ToString());
				else
					matrixScript.SetY(matrixScript.size.y.ToString());

				return;
			}

			if (matrixScript.size.x > 1 && matrixScript.size.y > 1) {
				if (matrixScript.Matrix.Count < matrixScript.size.x || matrixScript.Matrix[0].Count < matrixScript.size.y)
					matrixScript.UpdateMatrix();

				bool redraw = false;

				for (int i = 0; i < matrixScript.size.x; i++) {
					EditorGUILayout.BeginHorizontal();

					for (int o = 0; o < matrixScript.size.y; o++) {
						var temp = matrixScript.Matrix[i][o];

						matrixScript.Matrix[i][o] = EditorGUILayout.FloatField(matrixScript.Matrix[i][o]);

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
		EditorGUILayout.PropertyField(serializedObject.FindProperty("SaveButton"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("NInput"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("MInput"));

		serializedObject.ApplyModifiedProperties();
	}
#endif
}
