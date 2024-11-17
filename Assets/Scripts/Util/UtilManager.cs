using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;

public static class UtilManager {
	public static void UpdateInputFieldSilent (TMP_InputField input, string value) {
		var temp = input.onValueChanged;
		input.onValueChanged = new TMP_InputField.OnChangeEvent();

		input.text = value;

		input.onValueChanged = temp;
	}

	public static StringMatrix convertToStrMatrix(FractionMatrix reference) {
		return new StringMatrix(convertToStrMatrix(reference.GetMatrix()));
	}

	public static List<List<string>> convertToStrMatrix (List<List<Fraction>> reference) {
		var list = new List<List<string>>();

		foreach (var item in reference) {
			var subList = new List<string>();

			foreach (var Fraction in item) {
				subList.Add(Fraction.ToString());
			}

			list.Add(subList);
		}

		return list;
	}

	public static FractionMatrix convertToFractionMatrix (StringMatrix reference) {
		return new FractionMatrix(convertToFractionMatrix(reference.GetMatrix()));
	}

	public static List<List<Fraction>> convertToFractionMatrix (List<List<string>> reference) {
		var list = new List<List<Fraction>>();

		foreach (var item in reference) {
			var subList = new List<Fraction>();

			foreach (var String in item) {
				subList.Add(Fraction.Parse(String));
			}

			list.Add(subList);
		}

		return list;
	}

	public static string formatNumber (string number) {
		return formatNumber(Fraction.Parse(number));
	}

	public static string formatNumber (Fraction number) {
		return number < 0 ? string.Format("({0})", number) : number.ToString();
	}

	public static List<string> SeparateChars (string[] strings) {
		var list = new List<string>();

		foreach (string s in strings) {
			list.Add(SeparateChars(s));
		}

		return list;
	}

	public static string SeparateChars (string s) {
		var r = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

		return r.Replace(s, " ");
	}
}

public static class ListExtensions {
	public static void Assign<T>(this IList<T> list, int count, T value = default) {
		for(int i = 0; i < count; i++)
			list.Add(value);
	}
}
