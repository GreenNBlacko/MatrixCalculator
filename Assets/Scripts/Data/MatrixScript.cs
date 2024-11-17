using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatrixScript : MonoBehaviour {
	public Vector2 size = new Vector2(2, 2);

	public List<List<string>> Matrix = new List<List<string>>();

	public RectTransform ItemList;

	public GameObject ColumnPrefab;
	public GameObject RowPrefab;

	public bool augmented = false;

	public int augmentColumn = -1;

	public enum type { Matrix, Determinant, Augmented };

	public void ResizeMatrix() {
		var newMatrix = new List<List<string>>();

		for (int i = 0; i < size.x; i++) {
			var list = new List<string>();

			if (Matrix.Count > i)
				list = Matrix[i];

			for (int o = 0; o < size.y; o++) {
				if (list.Count <= o)
					list.Add("0");
			}

			newMatrix.Add(list);
		}

		Matrix = newMatrix;
	}

	public void DrawMatrix () {
		if (!Application.isPlaying)
			while (ItemList.childCount != 0)
				DestroyImmediate(ItemList.GetChild(0).gameObject);
		else
			foreach (RectTransform obj in ItemList)
				Destroy(obj.gameObject);

		for (int i = 0; i < size.x; i++) {
			if (i == augmentColumn && augmented) {
				var separator = Instantiate(transform.GetChild(0).gameObject, ItemList);

				separator.name = "Line";

				separator.transform.SetAsLastSibling();
			}

			var column = Instantiate(ColumnPrefab, ItemList).transform;
			column.gameObject.name = "Column_" + i;

			for (int o = 0; o < size.y; o++) {
				var row = Instantiate(RowPrefab, column).transform;
				row.gameObject.name = "Row_" + o;

				row.GetComponent<TMP_Text>().text = Matrix[i][o];
			}
		}
	}
}
