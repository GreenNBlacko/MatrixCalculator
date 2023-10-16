using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public static class UtilManager {
	public static void UpdateInputFieldSilent(TMP_InputField input, string value) {
		var temp = input.onValueChanged;
		input.onValueChanged = new TMP_InputField.OnChangeEvent();

		input.text = value;

		input.onValueChanged = temp;
	}

	public static List<List<string>> convertToStrMatrix(List<List<float>> reference) {
		var list = new List<List<string>>();

		foreach (var item in reference) {
			var subList = new List<string>();

			foreach (var Float in item) {
				subList.Add(Float.ToString());
			}

			list.Add(subList);
		}

		return list;
	}

	public static string formatNumber (float number) {
		return number < 0 ? string.Format("({0})", number) : number.ToString();
	}
}
