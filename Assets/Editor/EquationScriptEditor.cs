using Codice.Client.BaseCommands;
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EquationScript))]
public class EquationScriptEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();

		EditorGUILayout.Space(5);

		var script = (EquationScript)target;

		if(GUILayout.Button("Draw equation")) {
			script.Updater();
		}
	}
}

[CustomPropertyDrawer(typeof(EquationBlock))]
public class EquationBlockEditor : CIB_PropertyDrawerRenderer {
	protected override string pt_VariablesPath => CustomInspectorUtility.GetInspectorVariablesPath("EquationBlock");

	public override void CustomOnGUI (Rect f_position, ref Rect f_controlRect, SerializedProperty f_property, GUIContent f_label) {
		pt_InspectorVariables.FetchVariable("_matrix").CustomDrawer = new MatrixEditor();

		base.CustomOnGUI(f_position, ref f_controlRect, f_property, f_label);
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return 18 + CustomInspectorUtility.GetPropertyHeight(property, pt_InspectorVariables);
	}
}

[CustomPropertyDrawer(typeof(Matrix<string>))]
public class MatrixEditor : PropertyDrawer {
	Vector2 size = new Vector2(2, 2);

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		var cols = property.FindPropertyRelative("columns");


		size = new Vector2(Mathf.Max(size.x, cols.arraySize), Mathf.Max(size.y, (cols.arraySize < 1 ? 0 : cols.GetArrayElementAtIndex(0).FindPropertyRelative("rows").arraySize)));

		EditorGUI.BeginProperty(position, label, property);

		int activeItems = 0;

		var itemRect = new Rect(position.x, position.y + 19 * activeItems, position.width, 16);
		EditorGUI.PropertyField(itemRect, property, label);
		activeItems++;

		if(property.isExpanded) {
			EditorGUI.indentLevel++;

			itemRect = new Rect(position.x, position.y + 19 * activeItems, position.width, 16);
			size = EditorGUI.Vector2Field(itemRect, "Size", size);
			activeItems++;

			position = new Rect(position.x, position.y + 1, position.width, position.height);

			var columns = property.FindPropertyRelative("columns");

			columns.arraySize = (int)size.x;

			for(int i = 0; i < size.x; i++) {
				var rows = columns.GetArrayElementAtIndex(i).FindPropertyRelative("rows");
				rows.arraySize = (int)size.y;

				var ColumnRect = new Rect(position.x, position.y + 19 * activeItems, position.width / size.y, 16);

				for (int o = 0; o < size.y; o++) {
					var rowRect = new Rect(ColumnRect.x + ColumnRect.width * o, ColumnRect.y, ColumnRect.width - 1, 16);

					var row = rows.GetArrayElementAtIndex(o).FindPropertyRelative("Value");

					row.stringValue = EditorGUI.TextField(rowRect, row.stringValue);
				}

				activeItems++;
			}

			EditorGUI.indentLevel--;
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		float propertyHeight = 19f;
	
		if(property != null && property.isExpanded) {
			var array = property.FindPropertyRelative("columns");

			propertyHeight += 20f + 19 * array.arraySize;
		}

		return propertyHeight;
	}
}