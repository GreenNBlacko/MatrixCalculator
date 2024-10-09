#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CustomInspectorVariable))]
public class CustomInspectorVariablePropertyDrawer : PropertyDrawer {
	float ActiveElements = 0;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);

		ActiveElements = 0;

		Rect labelPosition = new Rect { x = position.x, y = position.y, width = position.width, height = 16 };

		property.isExpanded = EditorGUI.Foldout(labelPosition, property.isExpanded, label);
		ActiveElements++;

		if (property.isExpanded) {
			EditorGUI.indentLevel++;

			var variableNameRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
			var variableName = property.FindPropertyRelative("variableName");

			EditorGUI.PropertyField(variableNameRect, variableName);
			ActiveElements++;

			var variableTypeRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
			var variableType = property.FindPropertyRelative("variableType");

			EditorGUI.PropertyField(variableTypeRect, variableType);
			ActiveElements++;

			var displayTypeRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
			var displayType = property.FindPropertyRelative("displayType");

			EditorGUI.PropertyField(displayTypeRect, displayType);
			ActiveElements++;

			if(displayType.intValue == (int)VariableDisplayType.Custom) {
				var customVariableDisplayRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
				var customVariableDisplay = property.FindPropertyRelative("customVariableDisplay");

				EditorGUI.PropertyField(customVariableDisplayRect, customVariableDisplay);
				ActiveElements++;

				switch ((CustomVariableDisplay) customVariableDisplay.enumValueIndex) {
					case CustomVariableDisplay.IntRange:
						var ModifierOptionsRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
						var ModifierOptions = property.FindPropertyRelative("ModifierOptions");

						EditorGUI.PropertyField(ModifierOptionsRect, ModifierOptions);
						ActiveElements++;

						var BoundsRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
						var Bounds = property.FindPropertyRelative("Bounds");

						EditorGUI.PropertyField(BoundsRect, Bounds);
						ActiveElements++;
						break;

					case CustomVariableDisplay.IntPopup:
						var PopupOptionsRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
						var PopupOptions = property.FindPropertyRelative("PopupOptions");

						EditorGUI.PropertyField(PopupOptionsRect, PopupOptions);
						ActiveElements++;
						break;

					default:
						break;
				}
			}

			var ChildClassVariablesRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
			var ChildClassVariables = property.FindPropertyRelative("ChildClassVariables");

			EditorGUI.PropertyField(ChildClassVariablesRect, ChildClassVariables);
			ActiveElements++;

			var witdhRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
			var witdh = property.FindPropertyRelative("witdh");

			EditorGUI.PropertyField(witdhRect, witdh);
			ActiveElements++;

			var heightRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
			var height = property.FindPropertyRelative("height");

			EditorGUI.PropertyField(heightRect, height);
			ActiveElements++;

			var offsetRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
			var offset = property.FindPropertyRelative("offset");

			EditorGUI.PropertyField(offsetRect, offset);
			ActiveElements++;

			var variableDependenciesRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
			var variableDependencies = property.FindPropertyRelative("variableDependencies");

			variableDependencies.isExpanded = EditorGUI.Foldout(variableDependenciesRect, variableDependencies.isExpanded, variableDependencies.displayName);
			ActiveElements++;

			if(variableDependencies.isExpanded) {
				EditorGUI.indentLevel++;

				var SizeRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);

				variableDependencies.arraySize = EditorGUI.DelayedIntField(SizeRect, new GUIContent { text = "Size" }, variableDependencies.arraySize);
				ActiveElements++;

				if(variableDependencies.arraySize > 0) {
					for (int i = 0; i < variableDependencies.arraySize; i++) {
						var ElementRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
						var Element = variableDependencies.GetArrayElementAtIndex(i);

						EditorGUI.PropertyField(ElementRect, Element);
						ActiveElements++;

						if (Element.isExpanded) {
							var dependencyNameRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
							var dependencyName = Element.FindPropertyRelative("dependencyName");

							EditorGUI.PropertyField(dependencyNameRect, dependencyName);
							ActiveElements++;

							var dependencyTypeRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
							var dependencyType = Element.FindPropertyRelative("dependencyType");

							EditorGUI.PropertyField(dependencyTypeRect, dependencyType);
							ActiveElements++;

							var comparedValueRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
							var comparedValue = Element.FindPropertyRelative("comparedValue");

							switch ((DependencyType)dependencyType.enumValueIndex) {
								case DependencyType.Int:
									if (!int.TryParse(comparedValue.stringValue, out var _))
										comparedValue.stringValue = "0";

									comparedValue.stringValue = EditorGUI.IntField(comparedValueRect, new GUIContent(comparedValue.displayName, comparedValue.tooltip), int.Parse(comparedValue.stringValue)).ToString();
									break;

								case DependencyType.Bool:
									if (!bool.TryParse(comparedValue.stringValue, out var _))
										comparedValue.stringValue = "false";

									comparedValue.stringValue = EditorGUI.Toggle(comparedValueRect, new GUIContent(comparedValue.displayName, comparedValue.tooltip), bool.Parse(comparedValue.stringValue)).ToString();
									break;

								/*case DependencyType.Enum:
									CustomInspectorUtility.GetEnumType(property.propertyPath, dependencyName.stringValue, property);
									break;*/

								default: 
									EditorGUI.PropertyField(comparedValueRect, comparedValue);
									break;
							}

							
							ActiveElements++;

							var dependencyVerificationRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
							var dependencyVerification = Element.FindPropertyRelative("dependencyVerification");

							EditorGUI.PropertyField(dependencyVerificationRect, dependencyVerification);
							ActiveElements++;

							var verificationModifierRect = new Rect(position.x, position.y + 18 * ActiveElements, position.width, 16);
							var verificationModifier = Element.FindPropertyRelative("verificationModifier");

							EditorGUI.PropertyField(verificationModifierRect, verificationModifier);
							ActiveElements++;
						}
					}
				}
				EditorGUI.indentLevel--;
			}

			EditorGUI.indentLevel--;
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		float propertyHeight = EditorGUIUtility.singleLineHeight;

		if (property.isExpanded) {
			propertyHeight += 18 * 8;

			var displayType = property.FindPropertyRelative("displayType");

			if (displayType.intValue == (int)VariableDisplayType.Custom) {
				propertyHeight += 18;

				var customDisplayType = property.FindPropertyRelative("customVariableDisplay");

				switch ((CustomVariableDisplay)customDisplayType.intValue) {
					case CustomVariableDisplay.IntRange:
						propertyHeight += 18 * 2;
						break;

					case CustomVariableDisplay.IntPopup:
						propertyHeight += 18;
						break;
				

					default:
						break;
				}
			}

			var variableDependencies = property.FindPropertyRelative("variableDependencies");

			if (variableDependencies.isExpanded) {
				propertyHeight += 18 + 18 * variableDependencies.arraySize;

				for(int i = 0; i < variableDependencies.arraySize; i++) {
					if(variableDependencies.GetArrayElementAtIndex(i).isExpanded) {
						propertyHeight += 18 * 5;
					}
				}
			}
		}

		return propertyHeight;
	}
}

#endif