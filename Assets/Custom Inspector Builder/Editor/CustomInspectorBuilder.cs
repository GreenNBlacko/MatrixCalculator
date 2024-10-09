#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;


public class CustomInspectorBuilder : EditorWindow {
	public int MenuNumber = 0;
	public int SelectedOption;

	public bool CreateMode;

	public object reference;

	public string EdittedFile;

	public object output;

	private Vector2 scrollPos;

	[MenuItem("Utility/Custom Inspector Builder")]
	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(CustomInspectorBuilder), false, "Custom Inspector Builder");
	}

	public void OnGUI() {
		ShowMenu(MenuNumber);
	}

	public void ShowMenu(int ID) {
		switch (ID) {
			case 0: {
				EditorGUILayout.LabelField("<color=#FFFFFF>Create or edit custom inspectors</color>", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, richText = true });
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Create new custom inspector   ")) { MenuNumber = 1; }
				if (GUILayout.Button("Edit existing custom inspector")) { MenuNumber = 2; }
				EditorGUILayout.EndHorizontal();
				break;
			}
			case 1: {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Reference script");
				reference = (MonoScript)EditorGUILayout.ObjectField((Object)reference, typeof(MonoScript), true);
				EditorGUILayout.EndHorizontal();

				if (reference != null) {
					List<string> stringsTemp = new List<string>();
					List<string> strings = new List<string>();

					stringsTemp.AddRange(reference.ToString().Replace("public class ", "¤").Split('¤'));
					stringsTemp.RemoveAt(0);

					foreach (string element in stringsTemp) {
						string temp = element.Substring(0, element.Trim('¤').Replace(" : MonoBehaviour", "\n").Replace(" {", "\n").IndexOf('\n'));

						strings.Add(temp);
					}

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Selected class");
					SelectedOption = EditorGUILayout.Popup(SelectedOption, strings.ToArray());
					EditorGUILayout.EndHorizontal();

					if (GUILayout.Button("Create custom inspector")) { CreateMode = true; MenuNumber = 3; EdittedFile = strings[SelectedOption]; }
				} else {
					EditorGUILayout.HelpBox("Please select a referenced script and class before proceeding", MessageType.Info);
				}

				if (GUILayout.Button("Go back")) { MenuNumber = 0; reference = null; }
				break;
			}
			case 2: {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Script to edit");
				reference = (MonoScript)EditorGUILayout.ObjectField((Object)reference, typeof(MonoScript), true);
				EditorGUILayout.EndHorizontal();

				if (reference != null) {
					if (reference.ToString().Contains("PropertyDrawer") || reference.ToString().Contains("CustomEditor")) {
						string str;

						str = reference.ToString().Replace("public class ", "¤").Split('¤')[1];

						str = str.Substring(0, str.Trim('¤').Replace(" : PropertyDrawer {", "\n").Replace(" : Editor {", "\n").IndexOf('\n'));

						if (GUILayout.Button("Edit custom inspector")) { CreateMode = false; MenuNumber = 3; EdittedFile = str.Replace("PropertyDrawer", "").Replace("CustomEditor", ""); }
					} else {
						EditorGUILayout.HelpBox("The specified script is not a custom editor.", MessageType.Error);
					}
				} else {
					EditorGUILayout.HelpBox("Please select a referenced script before proceeding", MessageType.Info);
				}

				if (GUILayout.Button("Go back")) { MenuNumber = 0; reference = null; }
				break;
			}
			case 3: {
				if (reference == null) { Debug.LogError("Failed to get reference data. Ensure that you haven't moved/renamed the referenced script while attmenpting to edit the file."); MenuNumber = 0; OnGUI(); return; }

				MonoScript ReferenceScript = (MonoScript)reference;

				System.Type type;

				Debug.Log(ReferenceScript.GetClass().AssemblyQualifiedName);

				try {
					type = System.Type.GetType(ReferenceScript.GetClass().AssemblyQualifiedName.Replace("-Editor", "").Replace(ReferenceScript.GetClass().Name, EdittedFile));
				} catch {
					type = System.Type.GetType(ReferenceScript.GetClass().AssemblyQualifiedName.Replace(ReferenceScript.GetClass().Name, EdittedFile));
				}

				List<FieldInfo> info = new List<FieldInfo>();
				info.AddRange(type.GetFields());


				List<FieldInfo> variables = new List<FieldInfo>();

				foreach (FieldInfo field in info) {
					object[] attributes = field.GetCustomAttributes(typeof(System.Attribute), true);

					if (!field.IsPublic || field.IsPrivate)
						continue;

					variables.Add(field);

					foreach (object attribute in attributes) {
						if (attribute.ToString() == "UnityEngine.HideInInspector") { variables.Remove(field); break; }
					}
				}

				if (type.BaseType == typeof(MonoBehaviour)) {
					string path = Application.dataPath + "/Editor/" + EdittedFile + "CustomEditor.cs";

					if (CreateMode) {

					}
				} else {
					if (!Directory.Exists(Application.dataPath + "/CustomEditors/Editor/"))
						Directory.CreateDirectory(Application.dataPath + "/CustomEditors/Editor/");

					string path = Application.dataPath + "/CustomEditors/Editor/" + EdittedFile + "PropertyDrawer.cs";

					if (CreateMode) {
						string file =
							"#if UNITY_EDITOR\n" +
							"using UnityEditor;\n" +
							"using UnityEngine;\n" +
							"\n" +
							"[CustomPropertyDrawer(typeof(" + EdittedFile + "))]\n" + 
							"public class " + EdittedFile + "PropertyDrawer" + " : CIB_PropertyDrawerRenderer {\n" +
							"	const string InspectorVariablesPath = " + '"' + CustomInspectorUtility.GetInspectorVariablesPath(EdittedFile) + '"' + ";\n" +
							"	\n" +
							"	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {\n" +
							"		pt_InspectorVariables = CustomInspectorUtility.GetVariables(InspectorVariablesPath);\n" +
							"		\n" +
							"		EditorGUI.BeginProperty(position, label, property);\n" +
							"		\n" +
							"		Rect labelPosition = new Rect { x = position.x, y = position.y, width = position.width, height = 16 };\n" +
							"		\n" +
							"		property.isExpanded = EditorGUI.Foldout(labelPosition, property.isExpanded, label);\n" +
							"		\n" +
							"		if(property.isExpanded) {\n" +
							"			EditorGUI.indentLevel++;\n" +
							"			\n" +
							"			CustomInspectorUtility.ShowVariables(position, property, pt_InspectorVariables);\n" +
							"			EditorGUI.indentLevel--;\n" +
							"		}\n" +
							"		\n" +
							"		EditorGUI.EndProperty();\n" +
							"	}\n" +
							"	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {\n" +
							"		return 18 + CustomInspectorUtility.GetPropertyHeight(property, pt_InspectorVariables);\n" +
							"	}\n" +
							"}\n" +
							"#endif";

						File.WriteAllText(path, file);

						CreateMode = false;
					}

					scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

					EditorGUILayout.LabelField("Variables:");

					EditorGUI.indentLevel++;

					foreach (FieldInfo variable in variables) {
						EditorGUILayout.LabelField(variable.Name);
					}

					EditorGUI.indentLevel--;

					EditorGUILayout.EndScrollView();
				}

				if (GUILayout.Button("Cancel")) { MenuNumber = 0; reference = null; EdittedFile = null; }

				break;
			}
		}
	}
}

#region RenameAttribute
[CustomPropertyDrawer(typeof(RenameAttribute))]
public class RenameEditor : PropertyDrawer {
	RenameAttribute drawIf;

	SerializedProperty comparedField;

	private bool ShowMe(SerializedProperty property) {
		drawIf = attribute as RenameAttribute;

		List<bool> verificationResults = new List<bool>();

		int checkType = drawIf.checkType;

		for (int i = 0; i < drawIf.comparedPropertyName.Length; i++) {
			if (drawIf.comparedPropertyName[i] == "") { return true; }

			string path = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, drawIf.comparedPropertyName[i]) : drawIf.comparedPropertyName[i];

			comparedField = property.serializedObject.FindProperty(path);

			if (comparedField == null) {
				Debug.LogError("Cannot find property with name: " + path);
				verificationResults.Add(true);
			}

			// get the value & compare based on types
			switch (comparedField.type) { // Possible extend cases to support your own type
				case "bool": {
					verificationResults.Add(comparedField.boolValue.Equals(drawIf.comparedValue[i]));
					break;
				}
				case "Enum": {
					verificationResults.Add(comparedField.enumValueIndex.Equals((int)drawIf.comparedValue[i]));
					break;
				}

				default: {
					Debug.LogError("Error: " + comparedField.type + " is not supported of " + path);
					verificationResults.Add(true);
					break;
				}
			}
		}

		foreach (bool result in verificationResults) {
			if (checkType == 0) {
				if (!result)
					return false;
			} else {
				if (result)
					return true;
			}
		}

		if (verificationResults[verificationResults.Count - 1] && checkType == 0)
			return true;
		else
			return false;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		drawIf = attribute as RenameAttribute;

		if (ShowMe(property)) {
			EditorGUI.PropertyField(position, property, new GUIContent((attribute as RenameAttribute).NewName));
		} else {
			if (!drawIf.hideVariable)
				EditorGUI.PropertyField(position, property);
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		drawIf = attribute as RenameAttribute;

		if (!ShowMe(property) && drawIf.hideVariable)
			return 0f;
		return base.GetPropertyHeight(property, label);
	}
}
#endregion

#endif