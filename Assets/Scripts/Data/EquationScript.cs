using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EquationScript : MonoBehaviour {
	public RectTransform ItemList;

	public GameObject ExpressionPrefab;
	public GameObject OperatorPrefab;
	public GameObject NumberPrefab;
	public GameObject MatrixPrefab;

	public List<EquationBlock> Equations;

	public void Updater() {
		if (!Application.isPlaying)
			while (ItemList.childCount != 0)
				DestroyImmediate(ItemList.GetChild(0).gameObject);
		else
			foreach (RectTransform obj in ItemList)
				Destroy(obj.gameObject);

		foreach (EquationBlock e in Equations) {
			switch (e.type) {
				case EquationBlock.Types.Expression:
					var expr = Instantiate(ExpressionPrefab, ItemList);
					expr.name = "Expression";
					expr.transform.GetChild(0).GetComponent<TMP_Text>().text = e._expression;
					break;

				case EquationBlock.Types.Operator:
					var op = Instantiate(OperatorPrefab, ItemList);
					op.name = "Operator";
					op.transform.GetChild(0).GetComponent<TMP_Text>().text = "" + (char)e._operator;
					break;

				case EquationBlock.Types.Number:
					var num = Instantiate(NumberPrefab, ItemList);
					num.name = "Number";
					num.transform.GetChild(0).GetComponent<TMP_Text>().text = e._number.ToString();
					break;

				case EquationBlock.Types.Matrix:
					var matrix = Instantiate(MatrixPrefab, ItemList);
					matrix.name = "Matrix";
					matrix.GetComponent<MatrixScript>().Matrix = e._matrix.GetMatrix();
					matrix.GetComponent<MatrixScript>().DrawMatrix();
					break;
			}
		}
	}
}

[System.Serializable]
public class EquationBlock {
	public Types type;

	public string _expression;
	public Operators _operator;
	public float _number;
	public Matrix<string> _matrix;

	// Enums

	public enum Types { Expression, Operator, Number, Matrix };
	public enum Operators { Equals = '=', Plus = '+', Minus = '-', Divide = '/', Multiply = '*' };
}

[System.Serializable]
public class Matrix<T> {
	public List<Column<T>> columns = new List<Column<T>>();

	public List<List<T>> GetMatrix () {
		List<List<T>> values = new List<List<T>>();

		foreach (var c in columns) {
			List<T> row = new List<T>();

			foreach (var r in c.rows) {
				row.Add(r.Value);
			}

			values.Add(row);
		}

		return values;
	}

	public Matrix() { }

	public Matrix (List<List<T>> reference) {
		for (int i = 0; i < reference.Count; i++) {
			var col = new Column<T>();

			for (int o = 0; o < reference[0].Count; o++) {
				var row = new();

				row.Value = reference[i][o];

				col.rows.Add(row);
			}

			columns.Add(col);
		}
	}
}

public class FloatMatrix : Matrix<float> {
	public FloatMatrix FlipMatrix () {
		for (int i = 0; i < columns.Count; i++) {
			var list = columns[i];

			for (int o = 0; o < list.rows.Count; o++) {
				list.rows[o].Value *= -1;
			}
		}

		return this;
	}

	public static Matrix<float> operator + (FloatMatrix a, FloatMatrix b) {
		for (int i = 0; i < a.columns.Count; i++) {
			for (int o = 0; o < a.columns[0].rows.Count; o++) {
				a.columns[i].rows[o].Value += b.columns[i].rows[o].Value;
			}
		}

		return new Matrix<float>(a.GetMatrix());
	}

	public static Matrix<float> operator - (FloatMatrix a, FloatMatrix b) {
		b = b.FlipMatrix();

		for (int i = 0; i < a.columns.Count; i++) {
			for (int o = 0; o < a.columns[0].rows.Count; o++) {
				a.columns[i].rows[o].Value += b.columns[i].rows[o].Value;
			}
		}

		return new Matrix<float>(a.GetMatrix());
	}

	public static Matrix<float> operator * (FloatMatrix a, FloatMatrix b) {
		for (int i = 0; i < a.columns.Count; i++) {
			for (int o = 0; o < a.columns[0].rows.Count; o++) {
				a.columns[i].rows[o].Value *= b.columns[i].rows[o].Value;
			}
		}

		return a;
	}
}

public class StringMatrix : Matrix<string> {
	public StringMatrix FlipMatrix () {
		for (int i = 0; i < columns.Count; i++) {
			var list = columns[i];

			for (int o = 0; o < list.rows.Count; o++) {
				list.rows[o].Value = (int.Parse(list.rows[0].Value) * -1).ToString();
			}
		}

		return this;
	}

	public static Matrix<string> operator + (StringMatrix a, StringMatrix b) {
		for (int i = 0; i < a.columns.Count; i++) {
			for (int o = 0; o < a.columns[0].rows.Count; o++) {
				a.columns[i].rows[o].Value += b.columns[i].rows[o].Value;
			}
		}

		return new Matrix<string>(a.GetMatrix());
	}

	public static Matrix<string> operator - (StringMatrix a, StringMatrix b) {
		b = b.FlipMatrix();

		for (int i = 0; i < a.columns.Count; i++) {
			for (int o = 0; o < a.columns[0].rows.Count; o++) {
				a.columns[i].rows[o].Value += b.columns[i].rows[o].Value;
			}
		}

		return new Matrix<string>(a.GetMatrix());
	}

	public static Matrix<string> operator * (StringMatrix a, StringMatrix b) {
		for (int i = 0; i < a.columns.Count; i++) {
			for (int o = 0; o < a.columns[0].rows.Count; o++) {
				a.columns[i].rows[o].Value = (int.Parse(a.columns[i].rows[o].Value) * int.Parse(b.columns[i].rows[o].Value)).ToString();
			}
		}

		return a;
	}
}

[System.Serializable]
public class Column<T> {
	public List<Row<T>> rows = new List<Row<T>>();
}

[System.Serializable]
public class Row<T> {
	public T Value;
}
