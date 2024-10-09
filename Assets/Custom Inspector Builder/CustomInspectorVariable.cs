using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomInspectorVariable {
	public string variableName;

	public VariableType variableType;
	public VariableDisplayType displayType;

#if UNITY_EDITOR
	public System.Action<Rect, UnityEditor.SerializedProperty> OnGUI;
	public System.Func<UnityEditor.SerializedProperty, float> GetPropertyHeight;
	public UnityEditor.PropertyDrawer CustomDrawer;
#endif

	public CustomVariableDisplay customVariableDisplay;

	public RangeModifierOptions ModifierOptions;
	public Vector2 Bounds;

	public string PopupOptions;

	public CustomInspectorVariables ChildClassVariables;

	public float offset;

	public float witdh;
	public float height;

	public System.Type enumValues;

	public List<CustomInspectorVariableDependency> variableDependencies = new List<CustomInspectorVariableDependency>();

	public CustomInspectorVariable FetchVariable(string variableName) {
		var that = this;

		if(ChildClassVariables == null) {
			Debug.LogError("Variable does not have any child classes!");
			return that;
		}

		List<string> vars = new List<string>(variableName.Split('/'));

		if (that.ChildClassVariables == null) {
			Debug.LogError("Variable does not contain child variable named '" + vars[vars.Count - 1] + "'!");
			return that;
		}

		that = that.ChildClassVariables.FetchVariable(variableName.Substring(variableName.IndexOf('/') + 1));

		if (that == this || that == null) 
			Debug.LogError("Variable does not contain child variable named '" + vars[vars.Count - 1] + "'!");

		return that;
	}
}

[System.Serializable]
public class CustomInspectorVariableDependency {
	public string dependencyName;
	public DependencyType dependencyType;

	public string comparedValue;

	public DependencyVerification dependencyVerification;
	public DependencyVerificationModifier verificationModifier;
}

public enum VariableType { Int, String, Bool, Enum, Vector, Float, Class, Array, Generic };
public enum VariableDisplayType { Default, Custom, CustomCode };
public enum CustomVariableDisplay { IntField, IntRange, IntPopup };
public enum RangeModifierOptions { MinMaxField, Slider }
public enum DependencyType { Int, Bool, Enum, String };
public enum DependencyVerification { Equals, EqualsOpposite, Contains, ContainsOpposite, MoreThan, LessThan };
public enum DependencyVerificationModifier { AND, NAND, OR, NOR, XOR, XNOR };