using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatrixInputScript : MonoBehaviour {
	public Vector2 size = new Vector2(2, 2);

	public List<List<Fraction>> Matrix = new List<List<Fraction>>();

	public RectTransform ItemList;

	public GameObject ColumnPrefab;
	public GameObject RowPrefab;

	public Button SaveButton;
	public TMP_InputField NInput;
	public TMP_InputField MInput;

	public Action<FractionMatrix> CloseAction;

	[HideInInspector] public const string MatrixSizeError = "Improper matrix size";
	[HideInInspector] public const string MatrixElementError = "Invalid input at index ({0};{1})";

	public void Setup(FractionMatrix matrix) {
		if (matrix.columns.Count > 0) {
			size = new Vector2(matrix.columns.Count, matrix.columns[0].rows.Count);
			Matrix = matrix.GetMatrix();
		} else UpdateMatrix();

		DrawMatrix();
	}

	public void UpdateMatrix () {
		SaveButton.enabled = true;
		SaveButton.transform.GetChild(0).gameObject.SetActive(true);
		SaveButton.transform.GetChild(1).gameObject.SetActive(false);

		Matrix.Clear();

		for (int i = 0; i < size.x; i++) {
			var list = new List<Fraction>();

			for (int o = 0; o < size.y; o++) {
				if (ItemList.childCount <= i || ItemList.GetChild(i).childCount <= o) {
					list.Add(0);
					continue;
				}

				if (!Fraction.TryParse(ItemList.GetChild(i).GetChild(o).GetComponent<TMP_InputField>().text, out Fraction item)) {
					item = 0;

					SaveButton.enabled = false;
					SaveButton.transform.GetChild(0).gameObject.SetActive(false);
					SaveButton.transform.GetChild(1).gameObject.SetActive(true);

					SaveButton.transform.GetChild(1).GetComponent<TMP_Text>().text = string.Format(MatrixElementError, i, o);
				}

				list.Add(item);
			}

			Matrix.Add(list);
		}
	}

	public void SetX (string input) {
		if (!int.TryParse(input, out int X) || X < 1) {
			SaveButton.enabled = false;
			SaveButton.transform.GetChild(0).gameObject.SetActive(false);
			SaveButton.transform.GetChild(1).gameObject.SetActive(true);

			SaveButton.transform.GetChild(1).GetComponent<TMP_Text>().text = MatrixSizeError;

			return;
		}

		size = new Vector2(X, size.y);

		UpdateMatrix();

		DrawMatrix();
	}

	public void SetY (string input) {
		if (!int.TryParse(input, out int Y) || Y < 1) {
			SaveButton.enabled = false;
			SaveButton.transform.GetChild(0).gameObject.SetActive(false);
			SaveButton.transform.GetChild(1).gameObject.SetActive(true);

			SaveButton.transform.GetChild(1).GetComponent<TMP_Text>().text = MatrixSizeError;

			return;
		}

		size = new Vector2(size.x, Y);

		UpdateMatrix();

		DrawMatrix();
	}

	public void DrawMatrix () {
		if (!Application.isPlaying)
			while (ItemList.childCount != 0)
				DestroyImmediate(ItemList.GetChild(0).gameObject);
		else
			foreach (RectTransform obj in ItemList)
				Destroy(obj.gameObject);

		for (int i = 0; i < size.x; i++) {
			var column = Instantiate(ColumnPrefab, ItemList).transform;
			column.gameObject.name = "Column_" + i;

			for (int o = 0; o < size.y; o++) {
				var row = Instantiate(RowPrefab, column).transform;
				row.gameObject.name = "Row_" + o;

				UtilManager.UpdateInputFieldSilent(row.GetComponent<TMP_InputField>(), Matrix[i][o].ToString());

				row.GetComponent<TMP_InputField>().onEndEdit.AddListener(delegate {
					UpdateMatrix();
				});
			}
		}
	}

	public void Close () {
		UpdateMatrix();

		if (!SaveButton.enabled || !SaveButton.interactable) { return; }

		CloseAction.Invoke(new FractionMatrix(Matrix));

		Destroy(gameObject);
	}
}
