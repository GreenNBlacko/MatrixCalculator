#if UNITY_EDITOR
using B83.LogicExpressionParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CustomInspectorUtility {
	public static void ShowVariables (Rect position, SerializedProperty property, CustomInspectorVariables inspectorVariables) {
		Rect contentPosition = new Rect(position);

		int i = 0;

		foreach (CustomInspectorVariable variable in inspectorVariables.inspectorVariables) {
			if (CheckDependencies(property, variable)) {
				contentPosition = new Rect(contentPosition.x, contentPosition.y + (i > 0 ? inspectorVariables.inspectorVariables[i - 1].height : 16f) + variable.offset, (variable.witdh <= -1f) ? contentPosition.width : variable.witdh, variable.height);

				if (variable.displayType == VariableDisplayType.CustomCode && (variable.GetPropertyHeight == null || variable.OnGUI == null) && variable.CustomDrawer == null) {
					var tempX = contentPosition.x;
					var tempW = contentPosition.width;

					contentPosition = new Rect(EditorGUI.indentLevel * 15 + contentPosition.x, contentPosition.y, contentPosition.width - EditorGUI.indentLevel * 15, variable.height * 2);

					EditorGUI.HelpBox(contentPosition, string.Format("Variable '{0}' has 'Custom Code' renderer selected, but does not have either 'OnGUI' and 'GetPropertyHeight' functions nor 'CustomDrawer' assigned", variable.variableName), MessageType.Error);

					contentPosition = new Rect(tempX, contentPosition.y + variable.height, tempW, contentPosition.height);
					i++;
					continue;
				}

				if (variable.variableType == VariableType.Array) {
					var height =
						GetPropertyHeight(property.FindPropertyRelative(variable.variableName), variable);

					contentPosition = new Rect(contentPosition.x, contentPosition.y, contentPosition.width, height);

					DrawArray(variable, contentPosition, property.FindPropertyRelative(variable.variableName));

					//contentPosition = new Rect(contentPosition.x, contentPosition.y + height, contentPosition.width, contentPosition.height);
					i++;
					continue;
				}

				if (variable.variableType == VariableType.Class && variable.displayType != VariableDisplayType.CustomCode) {
					var height = 18f + 2f +
						GetPropertyHeight(property.FindPropertyRelative(variable.variableName), variable.ChildClassVariables);

					contentPosition = new Rect(contentPosition.x, contentPosition.y, contentPosition.width, height);

					DrawClass(variable, contentPosition, property.FindPropertyRelative(variable.variableName));

					//contentPosition = new Rect(contentPosition.x, contentPosition.y + height, contentPosition.width, contentPosition.height);
					i++;
					continue;
				}

				if (variable.displayType == VariableDisplayType.CustomCode) {
					if ((variable.OnGUI == null || variable.GetPropertyHeight == null) && variable.CustomDrawer != null) {
						variable.OnGUI = delegate (Rect r, SerializedProperty sp) {
							GUIContent label = new GUIContent(sp.displayName, sp.tooltip);

							variable.CustomDrawer.OnGUI(r, sp, label);
						};

						variable.GetPropertyHeight = delegate (SerializedProperty sp) {
							GUIContent label = new GUIContent(sp.displayName, sp.tooltip);

							return variable.CustomDrawer.GetPropertyHeight(sp, label);
						};
					}

					if (variable.OnGUI == null || variable.GetPropertyHeight == null) {
						i++;
						continue;
					}

					variable.OnGUI.Invoke(contentPosition, property.FindPropertyRelative(variable.variableName));

					contentPosition = new Rect(contentPosition.x, contentPosition.y + variable.GetPropertyHeight.Invoke(property.FindPropertyRelative(variable.variableName)) - 18, contentPosition.width, contentPosition.height);
				} else if (variable.displayType == VariableDisplayType.Default) {
					EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative(variable.variableName), true);
				} else {
					switch (variable.variableType) {
						case VariableType.Int: {
								switch (variable.customVariableDisplay) {
									case CustomVariableDisplay.IntField: {
											property.FindPropertyRelative(variable.variableName).intValue = EditorGUI.IntField(contentPosition, property.FindPropertyRelative(variable.variableName).displayName, property.FindPropertyRelative(variable.variableName).intValue);
											break;
										}
									case CustomVariableDisplay.IntRange: {
											if (variable.ModifierOptions == RangeModifierOptions.MinMaxField) {
												property.FindPropertyRelative(variable.variableName).intValue = Mathf.Clamp(EditorGUI.IntField(contentPosition, property.FindPropertyRelative(variable.variableName).displayName, property.FindPropertyRelative(variable.variableName).intValue), Mathf.RoundToInt(variable.Bounds.x), Mathf.RoundToInt(variable.Bounds.y));
											} else {
												property.FindPropertyRelative(variable.variableName).intValue = EditorGUI.IntSlider(contentPosition, property.FindPropertyRelative(variable.variableName).displayName, property.FindPropertyRelative(variable.variableName).intValue, Mathf.RoundToInt(variable.Bounds.x), Mathf.RoundToInt(variable.Bounds.y));
											}
											break;
										}
									case CustomVariableDisplay.IntPopup: {
											List<int> items = new List<int>();

											foreach (string s in variable.PopupOptions.Split(',')) {
												if (!int.TryParse(s, out int index))
													index = new List<string>(variable.PopupOptions.Split(',')).IndexOf(s);
												items.Add(index);
											}

											property.FindPropertyRelative(variable.variableName).intValue = EditorGUI.IntPopup(contentPosition, property.FindPropertyRelative(variable.variableName).displayName, property.FindPropertyRelative(variable.variableName).intValue, variable.PopupOptions.Split(','), items.ToArray());
											break;
										}
								}
								break;
							}
					}
				}
			}
			i++;
		}
	}

	public static void DrawArray (CustomInspectorVariable variable, Rect position, SerializedProperty property) {
		if (variable.displayType == VariableDisplayType.Default) {
			EditorGUI.PropertyField(position, property, true);

			return;
		}

		var label = new GUIContent(property.displayName, property.tooltip);

		EditorGUI.BeginProperty(position, label, property);

		Rect labelPosition = new Rect { x = position.x, y = position.y, width = position.width, height = 16 };

		property.isExpanded = EditorGUI.Foldout(labelPosition, property.isExpanded, label);

		if (property.isExpanded) {
			EditorGUI.indentLevel++;

			var sizePosition = new Rect(labelPosition.x, labelPosition.y + 18, labelPosition.width, labelPosition.height);

			var size = property.Copy();

			size.Next(true);
			size.Next(true);

			EditorGUI.PropertyField(sizePosition, size);

			var contentPosition = new Rect(position.x, position.y + 18 * 2, position.width, position.height);

			for (int i = 0; i < property.arraySize; i++) {
				var child = property.GetArrayElementAtIndex(i);

				if (variable.displayType == VariableDisplayType.CustomCode) {
					if ((variable.OnGUI == null || variable.GetPropertyHeight == null) && variable.CustomDrawer != null) {
						variable.OnGUI = delegate (Rect r, SerializedProperty sp) {
							GUIContent _label = new GUIContent(sp.displayName, sp.tooltip);

							variable.CustomDrawer.OnGUI(r, sp, _label);
						};

						variable.GetPropertyHeight = delegate (SerializedProperty sp) {
							GUIContent _label = new GUIContent(sp.displayName, sp.tooltip);

							return variable.CustomDrawer.GetPropertyHeight(sp, _label);
						};
					}

					if (variable.OnGUI == null || variable.GetPropertyHeight == null) {
						return;
					}

					contentPosition = new Rect(contentPosition.x, contentPosition.y + (i > 0 ? variable.GetPropertyHeight.Invoke(property.GetArrayElementAtIndex(i - 1)) : 0), contentPosition.width, GetPropertyHeight(child, variable.ChildClassVariables));

					variable.OnGUI.Invoke(contentPosition, child);
				} else {
					contentPosition = new Rect(contentPosition.x, contentPosition.y + (i > 0 ? 18 + GetPropertyHeight(property.GetArrayElementAtIndex(i - 1), variable.ChildClassVariables) : 0), contentPosition.width, GetPropertyHeight(child, variable.ChildClassVariables));

					DrawClass(variable, contentPosition, child);
				}
			}
			EditorGUI.indentLevel--;
		}

		EditorGUI.EndProperty();
	}

	public static void DrawClass (CustomInspectorVariable variable, Rect position, SerializedProperty property) {
		if (variable.displayType == VariableDisplayType.Default) {
			EditorGUI.PropertyField(position, property, true);

			return;
		}

		var label = new GUIContent(property.displayName, property.tooltip);

		EditorGUI.BeginProperty(position, label, property);

		Rect labelPosition = new Rect { x = position.x, y = position.y, width = position.width, height = 16 };

		property.isExpanded = EditorGUI.Foldout(labelPosition, property.isExpanded, label);

		if (property.isExpanded) {
			EditorGUI.indentLevel++;

			ShowVariables(position, property, variable.ChildClassVariables);
			EditorGUI.indentLevel--;
		}

		EditorGUI.EndProperty();
	}

	public static string GetInspectorVariablesPath (string fileName) {
		return "Assets/Custom Inspector Builder/CustomInspectors/" + fileName + "_CustomInspectorVariables.asset";
	}

	public static bool CheckDependencies (SerializedProperty property, CustomInspectorVariable variable) {
		if (variable.variableDependencies.Count > 0) {
			string dependencyCheck = "";



			for (int i = 0; i < variable.variableDependencies.Count; i++) {
				CustomInspectorVariableDependency variableDependency = variable.variableDependencies[i];
				var dependency = property.FindPropertyRelative(variableDependency.dependencyName);

				if (i > 0) {
					if (variable.variableDependencies[i - 1].verificationModifier == DependencyVerificationModifier.NAND || variable.variableDependencies[i - 1].verificationModifier == DependencyVerificationModifier.NOR || variable.variableDependencies[i - 1].verificationModifier == DependencyVerificationModifier.XNOR) {
						dependencyCheck += "!";
					}
				}

				switch (variableDependency.dependencyVerification) {
					case DependencyVerification.Equals: {
							switch (variableDependency.dependencyType) {
								case DependencyType.Int: {
										dependencyCheck += dependency.intValue + " == " + int.Parse(variableDependency.comparedValue);
										break;
									}
								case DependencyType.Bool: {
										dependencyCheck += (dependency.boolValue ? 1 : 0) + " == " + (bool.Parse(variableDependency.comparedValue) ? 1 : 0);
										break;
									}
								case DependencyType.Enum: {
										dependencyCheck += dependency.enumValueIndex + " == " + int.Parse(variableDependency.comparedValue);
										break;
									}
								case DependencyType.String: {
										dependencyCheck += dependency.stringValue + " == " + variableDependency.comparedValue;
										break;
									}
							}
							break;
						}
					case DependencyVerification.EqualsOpposite: {
							switch (variableDependency.dependencyType) {
								case DependencyType.Int: {
										dependencyCheck += dependency.intValue + " != " + int.Parse(variableDependency.comparedValue);
										break;
									}
								case DependencyType.Bool: {
										dependencyCheck += (dependency.boolValue ? 1 : 0) + " != " + (bool.Parse(variableDependency.comparedValue) ? 1 : 0);
										break;
									}
								case DependencyType.Enum: {
										dependencyCheck += dependency.enumValueIndex + " != " + int.Parse(variableDependency.comparedValue);
										break;
									}
								case DependencyType.String: {
										dependencyCheck += dependency.stringValue + " != " + variableDependency.comparedValue;
										break;
									}
							}
							break;
						}
					case DependencyVerification.Contains: {
							switch (variableDependency.dependencyType) {
								case DependencyType.Int: {
										dependencyCheck += (dependency.intValue.ToString().Contains(variableDependency.comparedValue) ? 1 : 0) + " == 1 ";
										break;
									}
								case DependencyType.Bool: {
										dependencyCheck += (dependency.boolValue ? 1 : 0) + " == " + (bool.Parse(variableDependency.comparedValue) ? 1 : 0);
										break;
									}
								case DependencyType.Enum: {
										dependencyCheck += (dependency.enumValueIndex.ToString().Contains(variableDependency.comparedValue) ? 1 : 0) + " == 1 ";
										break;
									}
								case DependencyType.String: {
										dependencyCheck += (dependency.stringValue.Contains(variableDependency.comparedValue) ? 1 : 0) + " == 1 ";
										break;
									}
							}
							break;
						}
					case DependencyVerification.ContainsOpposite: {
							switch (variableDependency.dependencyType) {
								case DependencyType.Int: {
										dependencyCheck += "" + (!dependency.intValue.ToString().Contains(variableDependency.comparedValue) ? 1 : 0) + " == 1 ";
										break;
									}
								case DependencyType.Bool: {
										dependencyCheck += "" + (!dependency.boolValue ? 1 : 0) + " == " + (!bool.Parse(variableDependency.comparedValue) ? 1 : 0);
										break;
									}
								case DependencyType.Enum: {
										dependencyCheck += (!dependency.enumValueIndex.ToString().Contains(variableDependency.comparedValue) ? 1 : 0) + " == 1 ";
										break;
									}
								case DependencyType.String: {
										dependencyCheck += (!dependency.stringValue.Contains(variableDependency.comparedValue) ? 1 : 0) + " == 1 ";
										break;
									}
							}
							break;
						}
					case DependencyVerification.MoreThan: {
							switch (variableDependency.dependencyType) {
								case DependencyType.Int: {
										dependencyCheck += dependency.intValue + " > " + int.Parse(variableDependency.comparedValue);
										break;
									}
								case DependencyType.Bool: {
										dependencyCheck += dependency.boolValue + " > " + (bool.Parse(variableDependency.comparedValue) ? 1 : 0);
										break;
									}
								case DependencyType.Enum: {
										dependencyCheck += dependency.enumValueIndex + " > " + int.Parse(variableDependency.comparedValue);
										break;
									}
								case DependencyType.String: {
										dependencyCheck += (dependency.stringValue.Contains(variableDependency.comparedValue) ? 1 : 0);
										break;
									}
							}
							break;
						}
					case DependencyVerification.LessThan: {
							switch (variableDependency.dependencyType) {
								case DependencyType.Int: {
										dependencyCheck += dependency.intValue + " < " + int.Parse(variableDependency.comparedValue);
										break;
									}
								case DependencyType.Bool: {
										dependencyCheck += dependency.boolValue + " < " + (bool.Parse(variableDependency.comparedValue) ? 1 : 0);
										break;
									}
								case DependencyType.Enum: {
										dependencyCheck += dependency.enumValueIndex + " < " + int.Parse(variableDependency.comparedValue);
										break;
									}
								case DependencyType.String: {
										dependencyCheck += (!dependency.stringValue.Contains(variableDependency.comparedValue) ? 1 : 0);
										break;
									}
							}
							break;
						}
				}

				if (i >= variable.variableDependencies.Count - 1)
					break;

				switch (variableDependency.verificationModifier) {
					case DependencyVerificationModifier.AND: {
							dependencyCheck += " && ";
							break;
						}
					case DependencyVerificationModifier.NAND: {
							dependencyCheck += " && !";
							break;
						}
					case DependencyVerificationModifier.OR: {
							dependencyCheck += " || ";
							break;
						}
					case DependencyVerificationModifier.NOR: {
							dependencyCheck += " || !";
							break;
						}
					case DependencyVerificationModifier.XOR: {
							dependencyCheck += " xor ";
							break;
						}
					case DependencyVerificationModifier.XNOR: {
							dependencyCheck += " xor !";
							break;
						}
				}
			}

			//if (dependencyCheck.Contains("0 == 0")) Debug.Log(dependencyCheck);

			Parser parser = new Parser();

			LogicExpression exp = parser.Parse(dependencyCheck);

			return exp.GetResult();
		} else {
			return true;
		}
	}

	[MenuItem("Utility/Create variables for script")]
	public static CustomInspectorVariables CreateVariables () {
		var path = EditorUtility.OpenFilePanel("Select the InspectorVariables script you wish to load the variables of", "Assets/Custom Inspector Builder/CustomInspectors", "asset");

		var filename = path.Split('/')[path.Split('/').Length - 1].Split('_')[0];

		return CreateVariables(GetInspectorVariablesPath(filename), filename, false);
	}

	public static CustomInspectorVariables CreateVariables (string Path, string fileName, bool createAsset = true) {
		string AssemblyToken = GetAssemblyName(fileName);

		if (createAsset) {
			var p_inspectorVariables = ScriptableObject.CreateInstance<CustomInspectorVariables>();

			AssetDatabase.CreateAsset(p_inspectorVariables, Path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		var inspectorVariables = AssetDatabase.LoadAssetAtPath<CustomInspectorVariables>(Path);

		List<FieldInfo> variables = new List<FieldInfo>();
		Debug.Log("[0] " + AssemblyToken);
		variables.AddRange(Type.GetType(AssemblyToken).GetFields());

		foreach (FieldInfo variable in variables) {
			CustomInspectorVariable inspectorVariable = new CustomInspectorVariable();

			inspectorVariable.variableName = variable.Name;

			inspectorVariable.variableType = VariableType.Int;

			if (variable.FieldType.BaseType.Name.Contains("Enum")) {
				inspectorVariable.variableType = VariableType.Enum;
			}

			if (variable.FieldType.IsClass) {
				if (!GenericVariableType(variable.FieldType.Name) && !variable.FieldType.Name.Contains("List`1"))
					inspectorVariable.variableType = VariableType.Class;
			}

			if (variable.FieldType.Name.Contains("[]") || variable.FieldType.Name.Contains("List")) {
				if (!GenericVariableType(GetClassName(variable.FieldType.FullName)))
					inspectorVariable.variableType = VariableType.Array;
			}

			switch (variable.FieldType.Name) {
				case "Int32": {
						inspectorVariable.variableType = VariableType.Int;
						break;
					}
				case "String": {
						inspectorVariable.variableType = VariableType.String;
						break;
					}
				case "Boolean": {
						inspectorVariable.variableType = VariableType.Bool;
						break;
					}
				case "Vector2": {
						inspectorVariable.variableType = VariableType.Vector;
						break;
					}
				case "Vector3": {
						inspectorVariable.variableType = VariableType.Vector;
						break;
					}
				case "Single": {
						inspectorVariable.variableType = VariableType.Float;
						break;
					}
				default:
					if (inspectorVariable.variableType == VariableType.Int)
						inspectorVariable.variableType = VariableType.Generic;
					break;
			}

			if (inspectorVariable.variableType == VariableType.Class) {
				Debug.Log("[1] " + variable.FieldType.FullName);
				Debug.Log("[2] " + variable.FieldType.Name);

				inspectorVariable.ChildClassVariables = GetVariables(GetInspectorVariablesPath(variable.FieldType.Name));
			}

			if (inspectorVariable.variableType == VariableType.Array) {
				Debug.Log("[1] " + variable.FieldType.FullName);

				var varName = GetClassName(variable.FieldType.FullName);

				Debug.Log("[2] " + varName);

				inspectorVariable.ChildClassVariables = GetVariables(GetInspectorVariablesPath(varName));
			}

			if (inspectorVariable.variableType == VariableType.Enum) {
				inspectorVariable.enumValues = variable.FieldType;
			}

			inspectorVariable.offset = 2;

			inspectorVariable.witdh = -1;
			inspectorVariable.height = 16;

			inspectorVariables.inspectorVariables.Add(inspectorVariable);

		}

		return inspectorVariables;
	}

	public static bool GenericVariableType (string TypeName) {
		if (TypeName == "Sprite" ||
			TypeName == "RectTransform" ||
			TypeName == "AudioClip" ||
			TypeName == "AudioSource" ||
			TypeName == "Enum" ||
			TypeName.Contains("+") ||
			TypeName.Contains("`"))
			return true;

		return false;
	}

	public static string GetClassName (string arrayClass) {
		return arrayClass.Replace("System.Collections.Generic.List`1[[", "").Split(',')[0];
	}

	public static CustomInspectorVariables GetVariables (string Path) {
		if (!File.Exists(Path)) {
			string fileName = Path.Replace("Assets/Editor/InspectorVariables/", "").Replace("_InspectorVariables.asset", "");

			return CreateVariables(Path, fileName);
		}

		CustomInspectorVariables inspectorVariables;

		inspectorVariables = AssetDatabase.LoadAssetAtPath<CustomInspectorVariables>(Path);

		return inspectorVariables;
	}

	public static string GetAssemblyName (string fileName, bool editor = false) {
		if (editor)
			return fileName + ", Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
		return fileName + ", Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
	}

	public static float GetPropertyHeight (SerializedProperty property, CustomInspectorVariables inspectorVariables) {
		float propertyHeight = 0;

		if (!property.isExpanded) { return 0; }

		foreach (CustomInspectorVariable inspectorVariable in inspectorVariables.inspectorVariables) {
			if ((inspectorVariable.OnGUI == null || inspectorVariable.GetPropertyHeight == null) && inspectorVariable.CustomDrawer != null) {
				inspectorVariable.OnGUI = delegate (Rect r, SerializedProperty sp) {
					GUIContent label = new GUIContent(sp.displayName, sp.tooltip);

					inspectorVariable.CustomDrawer.OnGUI(r, sp, label);
				};

				inspectorVariable.GetPropertyHeight = delegate (SerializedProperty sp) {
					GUIContent label = new GUIContent(sp.displayName, sp.tooltip);

					return inspectorVariable.CustomDrawer.GetPropertyHeight(sp, label);
				};
			}

			if (!CheckDependencies(property, inspectorVariable))
				continue;

			if (inspectorVariable.displayType == VariableDisplayType.CustomCode) {
				if (inspectorVariable.OnGUI == null || inspectorVariable.GetPropertyHeight == null) {
					if (CheckDependencies(property, inspectorVariable))
						propertyHeight += 32 + inspectorVariable.offset;
					continue;
				}


				if (inspectorVariable.variableType == VariableType.Array) {
					var prop = property.FindPropertyRelative(inspectorVariable.variableName);

					propertyHeight += 18;

					if (prop.isExpanded) {
						propertyHeight += 18;

						for (int i = 0; i < prop.arraySize; i++) {
							var prop2 = prop.GetArrayElementAtIndex(i);

							propertyHeight += inspectorVariable.GetPropertyHeight.Invoke(prop2);
						}
					}
				} else if (inspectorVariable.variableType == VariableType.Class) {
					propertyHeight += inspectorVariable.GetPropertyHeight.Invoke(property.FindPropertyRelative(inspectorVariable.variableName));
				} else {
					propertyHeight += inspectorVariable.GetPropertyHeight.Invoke(property.FindPropertyRelative(inspectorVariable.variableName));
				}
				continue;
			}

			switch (inspectorVariable.variableType) {
				case VariableType.Int: {
						propertyHeight += inspectorVariable.height + inspectorVariable.offset;
						break;
					}

				case VariableType.Array:
					var prop = property.FindPropertyRelative(inspectorVariable.variableName);

					propertyHeight += 18;

					if (prop.isExpanded) {
						propertyHeight += 18 * prop.arraySize + GetPropertyHeight(prop, inspectorVariable) + inspectorVariable.offset;
					}
					break;

				case VariableType.Class:
					propertyHeight += 18f;

					if (property.FindPropertyRelative(inspectorVariable.variableName).isExpanded) {
						foreach (var vari in inspectorVariable.ChildClassVariables.inspectorVariables) {
							if (CheckDependencies(property.FindPropertyRelative(inspectorVariable.variableName), vari)) {
								if (vari.variableType == VariableType.Class) {
									propertyHeight += GetPropertyHeight(property.FindPropertyRelative(inspectorVariable.variableName), vari) + vari.offset;
								} else if (vari.variableType == VariableType.Array) {
									propertyHeight += GetPropertyHeight(property.FindPropertyRelative(inspectorVariable.variableName), vari) + vari.offset;
								} else {
									propertyHeight += vari.height + vari.offset;
								}
							}
						}
					}
					break;

				default: {
						propertyHeight += inspectorVariable.height + inspectorVariable.offset;
						break;
					}
			}
		}

		return propertyHeight;
	}

	public static Type GetEnumType (string path, string varName, SerializedProperty property) {
		Type output = null;

		var properPath = path.Substring(0, path.Length - 8);

		var properProperty = property.serializedObject.FindProperty(properPath);

		for (int i = 0; i < properProperty.arraySize; i++) {
			var targetProperty = properProperty.GetArrayElementAtIndex(i);

			if (targetProperty == property)
				continue;

			if (targetProperty.FindPropertyRelative("variableName").stringValue == varName) {
				var targetObject = targetProperty.serializedObject.targetObject;
				var targetObjectClassType = targetObject.GetType();
				var field = targetObjectClassType.GetField(targetProperty.propertyPath);
				if (field != null) {
					var value = field.GetValue(targetObject);
					Debug.Log(value);
				}

				//output = ;
			}
		}

		return output;
	}

	public static float GetPropertyHeight (SerializedProperty property, CustomInspectorVariable inspectorVariable) {
		float propertyHeight = 0;

		if ((inspectorVariable.OnGUI == null || inspectorVariable.GetPropertyHeight == null) && inspectorVariable.CustomDrawer != null) {
			inspectorVariable.OnGUI = delegate (Rect r, SerializedProperty sp) {
				GUIContent label = new GUIContent(sp.displayName, sp.tooltip);

				inspectorVariable.CustomDrawer.OnGUI(r, sp, label);
			};

			inspectorVariable.GetPropertyHeight = delegate (SerializedProperty sp) {
				GUIContent label = new GUIContent(sp.displayName, sp.tooltip);

				return inspectorVariable.CustomDrawer.GetPropertyHeight(sp, label);
			};
		}

		if (!CheckDependencies(property, inspectorVariable))
			return propertyHeight;

		if ((inspectorVariable.OnGUI == null || inspectorVariable.GetPropertyHeight == null) && inspectorVariable.displayType == VariableDisplayType.CustomCode) {
			return inspectorVariable.height * 2 + inspectorVariable.offset;
		}

		if (property.isExpanded) {
			if (inspectorVariable.variableType == VariableType.Array && property.isArray) {
				propertyHeight += 18;

				for (int i = 0; i < property.arraySize; i++) {
					if (inspectorVariable.displayType == VariableDisplayType.CustomCode) {
						propertyHeight += inspectorVariable.GetPropertyHeight.Invoke(property.GetArrayElementAtIndex(i));
					} else
						if (property.GetArrayElementAtIndex(i).isExpanded)
						propertyHeight += GetPropertyHeight(property.GetArrayElementAtIndex(i), inspectorVariable.ChildClassVariables);
				}
			} else if (inspectorVariable.variableType == VariableType.Class) {
				if (inspectorVariable.displayType == VariableDisplayType.CustomCode)
					propertyHeight += inspectorVariable.GetPropertyHeight.Invoke(property);
				else
					propertyHeight += GetPropertyHeight(property, inspectorVariable.ChildClassVariables);
			} else {
				propertyHeight += inspectorVariable.height + inspectorVariable.offset;
			}
		}

		return propertyHeight;
	}
}

public class CIB_PropertyDrawerRenderer : PropertyDrawer {
	/* 
												NAMING EXPLANATION

		Every variable is followed by a letter to determine their type and an underscore to seperate name and type
		The types are as follows:
		 • p - public
		 • pr - private
		 • pt - protected
		 • s - static
		 • f - function
		 • l - local
	 */

	protected Dictionary<string, Rect> pt_ControlRects = new Dictionary<string, Rect>();

	protected CustomInspectorVariables pt_InspectorVariables = ScriptableObject.CreateInstance<CustomInspectorVariables>();

	public virtual void CustomOnGUI (Rect f_position, ref Rect f_controlRect, SerializedProperty f_property, GUIContent f_label) {
		CustomInspectorUtility.ShowVariables(f_position, f_property, pt_InspectorVariables);
	}

	protected virtual string pt_VariablesPath => "";
	protected virtual bool pt_Ignorebase => false;

	protected float indent => 16 * EditorGUI.indentLevel;

	private float pr_offset = 3;

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {	
		if (!pt_ControlRects.ContainsKey(property.propertyPath))
			pt_ControlRects.Add(property.propertyPath, new Rect());

		if (pt_VariablesPath != "")
			pt_InspectorVariables = CustomInspectorUtility.GetVariables(pt_VariablesPath);

		pt_ControlRects[property.propertyPath] = new Rect(position.position, new Vector2(position.width, 16));

		var l_controlRect = pt_ControlRects[property.propertyPath];

		EditorGUI.BeginProperty(position, label, property);

		if (pt_Ignorebase) {
			 CustomOnGUI(position, ref l_controlRect, property, label);

			EditorGUI.EndProperty();

			pt_ControlRects[property.propertyPath] = l_controlRect;

			return;
		}

		Rect labelPosition = new Rect { x = position.x, y = position.y, width = position.width, height = 16 };

		property.isExpanded = EditorGUI.Foldout(labelPosition, property.isExpanded, label);

		l_controlRect.y += 16 + pr_offset;

		if (property.isExpanded) {
			EditorGUI.indentLevel++;

			CustomOnGUI(position, ref l_controlRect, property, label);

			EditorGUI.indentLevel--;
		}

		EditorGUI.EndProperty();

		pt_ControlRects[property.propertyPath] = l_controlRect;
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		if (!pt_ControlRects.ContainsKey(property.propertyPath))
			pt_ControlRects.Add(property.propertyPath, new Rect());

		return pt_ControlRects[property.propertyPath].y;
	}

	public GUIContent getLabel (SerializedProperty f_property) {
		return getLabel(f_property.displayName, f_property.tooltip);
	}

	public GUIContent getLabel (string f_name, string f_tooltip = "") {
		return new GUIContent(f_name, f_tooltip);
	}

	public void Foldout (ref Rect f_rect, ref SerializedProperty f_foldout, GUIContent f_label, Func<Rect, Rect> f_content) {
		if (f_foldout.isExpanded = EditorGUI.Foldout(f_rect, f_foldout.isExpanded, f_label)) {
			f_rect.y += f_rect.height + pr_offset;

			f_rect = f_content.Invoke(f_rect);
		}
	}

	public void ButtonFoldout (ref Rect f_rect, ref SerializedProperty f_foldout, GUIContent f_label, Func<Rect, Rect> f_content, GUIContent f_buttonLabel, Action f_button, float f_buttonWidth = 150f) {
		var l_rect = new Rect(f_rect);
		l_rect.width -= indent;
		l_rect.x += indent;

		var l_foldoutRect = new Rect(l_rect.position, new Vector2(l_rect.width - f_buttonWidth, l_rect.height));
		var l_buttonRect = new Rect(l_rect.width - f_buttonWidth, l_rect.y, f_buttonWidth, l_rect.height);

		f_rect.y += l_foldoutRect.height + pr_offset;

		if (f_foldout.isExpanded = EditorGUI.Foldout(l_foldoutRect, f_foldout.isExpanded, f_label)) {
			f_rect = f_content.Invoke(f_rect);
		}

		if (GUI.Button(l_buttonRect, f_buttonLabel))
			f_button.Invoke();
	}

	public void InfoMessage (ref Rect f_rect, string f_message, MessageType f_type) {
		var l_rect = new Rect(f_rect);
		l_rect.width -= indent;
		l_rect.x += indent;

		EditorGUI.HelpBox(new Rect(l_rect.x, l_rect.y, l_rect.width, 32), f_message, f_type);

		f_rect.y += 32 + pr_offset;
	}

	public void Button (ref Rect f_rect, GUIContent f_label, Action f_onPressed) {
		var l_rect = new Rect(f_rect);
		l_rect.width -= indent;
		l_rect.x	 += indent;

		if (GUI.Button(l_rect, f_label))
			f_onPressed.Invoke();

		f_rect.y += 16 + pr_offset;
	}

	public void LabelField (ref Rect f_rect, GUIContent f_label) {
		EditorGUI.LabelField(f_rect, f_label);

		f_rect.y += 16 + pr_offset;
	}

	public void TextField (ref Rect f_rect, GUIContent f_label, ref SerializedProperty f_property) {
		f_property.stringValue = EditorGUI.TextField(f_rect, f_label, f_property.stringValue);

		f_rect.y += 16 + pr_offset;
	}

	public void TextArea (ref Rect f_rect, GUIContent f_label, ref SerializedProperty f_property, int f_minLines = 3, int f_maxLines = 15) {
		var l_lines = new List<string>(f_property.stringValue.Split('\n'));

		LabelField(ref f_rect, f_label);

		var l_rect = new Rect(f_rect);
		l_rect.height = 15.2f * Mathf.Clamp(l_lines.Count, f_minLines, f_maxLines);

		f_property.stringValue = EditorGUI.TextArea(l_rect, f_property.stringValue);

		f_rect.y += 16 + pr_offset + 15 * Mathf.Clamp(l_lines.Count, f_minLines, f_maxLines) + pr_offset;
	}

	public void Toggle (ref Rect f_rect, GUIContent f_label, ref SerializedProperty f_property, Func<Rect, bool, Rect> f_onToggle = null) {
		var l_value = f_property.boolValue;

		f_property.boolValue = EditorGUI.Toggle(f_rect, f_label, f_property.boolValue);

		if (f_onToggle != null && l_value != f_property.boolValue)
			f_rect = f_onToggle.Invoke(f_rect, f_property.boolValue);

		f_rect.y += 16 + pr_offset;
	}
}

/*										TESTING SLUT
[CustomPropertyDrawer(typeof(Menu))]
public class MenuDrawer : CIB_PropertyDrawerRenderer {
	protected override bool pt_Ignorebase => true;

	public override void CustomOnGUI (Rect f_position, ref Rect f_controlRect, SerializedProperty f_property, GUIContent f_label) {
		Toggle(ref f_controlRect, getLabel("Property"), ref f_property);

		ButtonFoldout(ref f_controlRect, ref f_property,
			getLabel("Test"), delegate (Rect rect) {
				EditorGUI.indentLevel++;

				InfoMessage(ref rect, "Perfectly innocent test", MessageType.Info);

				Button(ref rect, getLabel("Button"), delegate { Debug.Log("Button pressed"); });

				var l_nameProperty = f_property.FindPropertyRelative("name");

				TextField(ref rect, getLabel("Name"), ref l_nameProperty);

				EditorGUI.indentLevel--;
				return rect;
			},
			getLabel("Press for log"), delegate {
				Debug.Log("log");
			}
		);

		base.CustomOnGUI(f_position, ref f_controlRect, f_property, f_label);
	}
}
*/

#endif