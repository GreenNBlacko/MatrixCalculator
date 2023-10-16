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
public class EquationBlockEditor : PropertyDrawer {
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
		label.text = Enum.GetName(typeof(EquationBlock.Types), property.FindPropertyRelative("type").intValue);

		EditorGUI.BeginProperty(position, label, property);

		int activeItems = 0;

		var itemRect = new Rect(position.x, position.y + 19 * activeItems, position.width, 16);
		EditorGUI.PropertyField(itemRect, property, label);
		activeItems++;

		if(property.isExpanded) {
			EditorGUI.indentLevel++;

			var type = property.FindPropertyRelative("type");

			itemRect = new Rect(position.x, position.y + 19 * activeItems, position.width, 16);
			EditorGUI.PropertyField(itemRect, type);
			activeItems++;

			switch ((EquationBlock.Types)type.intValue) {
				case EquationBlock.Types.Matrix: 
					var matrix = property.FindPropertyRelative("_matrix");
					var height = new MatrixEditor().GetPropertyHeight(matrix, label);

					itemRect = new Rect(position.x, position.y + 19 * activeItems, position.width, height);

					EditorGUI.PropertyField(itemRect, matrix);

					activeItems += (int)(height / 19);
					break;

				case EquationBlock.Types.Number:
					var number = property.FindPropertyRelative("_number");

					itemRect = new Rect(position.x, position.y + 19 * activeItems, position.width, 16);
					EditorGUI.PropertyField(itemRect, number);
					activeItems++;
					break;

				case EquationBlock.Types.Expression:
					var expr = property.FindPropertyRelative("_expression");

					itemRect = new Rect(position.x, position.y + 19 * activeItems, position.width, 16);
					EditorGUI.PropertyField(itemRect, expr);
					activeItems++;
					break;

				case EquationBlock.Types.Operator:
					var op = property.FindPropertyRelative("_operator");

					itemRect = new Rect(position.x, position.y + 19 * activeItems, position.width, 16);
					EditorGUI.PropertyField(itemRect, op);
					activeItems++;
					break;
			}

			EditorGUI.indentLevel--;
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		float propertyHeight = 19f;

		if (property.isExpanded) {
			var type = property.FindPropertyRelative("type");

			propertyHeight += 19f;

			switch ((EquationBlock.Types)type.intValue) {
				case EquationBlock.Types.Matrix:
					propertyHeight += new MatrixEditor().GetPropertyHeight(property.FindPropertyRelative("_matrix"), label);
					break;

				default:
					propertyHeight += 19f;
					break;
			}

		}

		return propertyHeight;	
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
	
		if(property.isExpanded) {
			var array = property.FindPropertyRelative("columns");

			propertyHeight += 20f + 19 * array.arraySize;
		}

		return propertyHeight;
	}
}
