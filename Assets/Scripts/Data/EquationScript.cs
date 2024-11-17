using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquationScript : MonoBehaviour {
	public RectTransform ItemList;

	public GameObject ExpressionPrefab;
	public GameObject OperatorPrefab;
	public GameObject NumberPrefab;
	public GameObject MatrixPrefab;
	public GameObject FractionPrefab;
	public GameObject TablePrefab;
	public GameObject TableRowPrefab;
	public GameObject TableColumnPrefab;

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
					num.transform.GetChild(0).GetComponent<TMP_Text>().text = e._number;
					break;

				case EquationBlock.Types.Matrix:
					var matrix = Instantiate(MatrixPrefab, ItemList);
					matrix.name = "Matrix";
					matrix.GetComponent<MatrixScript>().augmentColumn = (int)Mathf.Min(e._matrix.format.x, e._matrix.format.y);
					matrix.GetComponent<MatrixScript>().size = new Vector2(e._matrix.columns.Count, e._matrix.columns[0].rows.Count);
					matrix.GetComponent<MatrixScript>().Matrix = e._matrix.GetMatrix();
					matrix.GetComponent<MatrixScript>().augmented = e.augmented;
					matrix.GetComponent<MatrixScript>().DrawMatrix();

					if (!e.determinant) {
						var spacer1 = Instantiate(matrix.transform.GetChild(0).gameObject, matrix.transform);
						var spacer2 = Instantiate(matrix.transform.GetChild(0).gameObject, matrix.transform);

						spacer1.name = "Spacer";
						spacer2.name = "Spacer";

						spacer1.transform.SetAsFirstSibling();
						spacer2.transform.SetAsLastSibling();
					}
					break;

				case EquationBlock.Types.Fraction:
					var frac = Instantiate(FractionPrefab, ItemList);
					frac.name = "Fraction";
					frac.transform.GetChild(0).GetComponent<TMP_Text>().text = e._fraction._numerator.ToString();
					frac.transform.GetChild(2).GetComponent<TMP_Text>().text = e._fraction._denominator.ToString();
					break;

				case EquationBlock.Types.Table:
					var _table = Instantiate(TablePrefab, ItemList);
					foreach (var row in e._table.GetTable()) {
						var _row = Instantiate(TableRowPrefab, _table.transform);

						foreach (var col in row) {
							var _col = Instantiate(TableColumnPrefab, _row.transform);

							_col.transform.GetComponentInChildren<TMP_Text>().text = col.label;
							_col.transform.GetChild(0).GetComponent<Graphic>().color = col.color;
						}
					}
					break;
			}
		}
	}

	public void SetEquations(List<EquationBlock> equations) {
		Equations = equations;

		Updater();
	}

	public static EquationBuilder BuildEquation() {
		return new EquationBuilder();
	}
}

public class EquationBuilder {
	public EquationBuilder AddExpression(string expression) {
		equations.Add(new EquationBlock { type = EquationBlock.Types.Expression, _expression = expression });

		return this;
	}

	public EquationBuilder AddOperator(EquationBlock.Operators op) {
		equations.Add(new EquationBlock { type = EquationBlock.Types.Operator, _operator = op });

		return this;
	}

	public EquationBuilder AddNumber(double number) {
		equations.Add(new EquationBlock { type = EquationBlock.Types.Number, _number = number.ToString() });

		return this;
	}

	public EquationBuilder AddNumber(Fraction number) {
		if (number._denominator == 1)
			return AddNumber(number.ToDouble());

		return AddFraction(number);
	}

	public EquationBuilder AddNumber(string number) {
		equations.Add(new EquationBlock { type = EquationBlock.Types.Number, _number = number });

		return this;
	}

	public EquationBuilder AddMatrix(FractionMatrix matrix) {
		return AddMatrix(UtilManager.convertToStrMatrix(matrix));
	}

	public EquationBuilder AddMatrix(StringMatrix matrix) {
		equations.Add(new EquationBlock { type = EquationBlock.Types.Matrix, _matrix = matrix });

		return this;
	}

	public EquationBuilder AddDeterminant(FractionMatrix matrix) {
		return AddDeterminant(UtilManager.convertToStrMatrix(matrix));
	}

	public EquationBuilder AddDeterminant(StringMatrix matrix) {
		equations.Add(new EquationBlock { type = EquationBlock.Types.Matrix, _matrix = matrix, determinant = true });

		return this;
	}

	public EquationBuilder AddAugmented(FractionMatrix matrixA, FractionMatrix matrixB) {
		return AddAugmented(UtilManager.convertToStrMatrix(matrixA), UtilManager.convertToStrMatrix(matrixB));
	}

	public EquationBuilder AddAugmented(StringMatrix matrixA, StringMatrix matrixB) {
		var augment = matrixA.Augment(matrixB) as StringMatrix;

		return AddAugmented(augment);
	}

	public EquationBuilder AddAugmented(StringMatrix matrix) {
		equations.Add(new EquationBlock { type = EquationBlock.Types.Matrix, _matrix = matrix, augmented = true });

		return this;
	}

	public EquationBuilder AddFraction(string numerator, string denominator) {
		return AddFraction(new Fraction(numerator, denominator));
	}

	public EquationBuilder AddFraction(Fraction fraction) {
		equations.Add(new EquationBlock { type = EquationBlock.Types.Fraction, _fraction = fraction });

		return this;
	}

	public EquationBuilder AddTable(Table table) {
		equations.Add(new EquationBlock { type = EquationBlock.Types.Table, _table = table });

		return this;
	}

	public EquationBuilder AddBlock(EquationBlock block) {
		equations.Add(block);

		return this;
	}

	public EquationBuilder AddBlocks(List<EquationBlock> blocks) {
		equations.AddRange(blocks);

		return this;
	}

	public List<EquationBlock> GetEquation() {
		return equations;
	}

	private List<EquationBlock> equations = new List<EquationBlock>();
}

[System.Serializable]
public class EquationBlock {
	public Types type;

	public string _expression;
	public Operators _operator;
	public string _number;
	public Matrix<string> _matrix = new StringMatrix();
	public Table _table = new();
	public bool determinant = false;
	public bool augmented = false;
	public Fraction _fraction = new Fraction();

	// Enums

	public enum Types { Expression, Operator, Number, Matrix, Fraction, Table };
	public enum Operators { Equals = '=', Plus = '+', Minus = '-', Divide = '/', Multiply = '*', Equivalent = '~' };
}

[System.Serializable]
public class Fraction {
	public System.Numerics.BigInteger _numerator;
	public System.Numerics.BigInteger _denominator;

	public Fraction() {
		_numerator = 0;
		_denominator = 1;
	}

	public Fraction(string numerator, string denominator) {
		_numerator = System.Numerics.BigInteger.Parse(numerator);
		_denominator = System.Numerics.BigInteger.Parse(denominator);
		Simplify();
	}

	public Fraction(System.Numerics.BigInteger numerator, System.Numerics.BigInteger denominator) {
		if (denominator == 0)
			throw new DivideByZeroException("Denominator cannot be zero.");

		_numerator = numerator;
		_denominator = denominator;
		Simplify();
	}

	// Simplify the fraction using GCD
	private void Simplify() {
		System.Numerics.BigInteger gcd = System.Numerics.BigInteger.GreatestCommonDivisor(_numerator, _denominator);
		_numerator /= gcd;
		_denominator /= gcd;

		// Ensure denominator is positive
		if (_denominator < 0) {
			_numerator = -_numerator;
			_denominator = -_denominator;
		}
	}

	public static Fraction Parse(string fraction) {
		System.Numerics.BigInteger nom, den = 1;

		var sides = fraction.Split('/');

		if (sides.Length == 0)
			throw new NullReferenceException("No fraction found!");

		var _nom = sides[0];
		var _den = sides.Length > 1 ? sides[1] : "1";

		try {
			nom = System.Numerics.BigInteger.Parse(_nom);
			den = System.Numerics.BigInteger.Parse(_den);
		} catch {
			return double.Parse(fraction);
		}

		return new Fraction(nom, den);
	}

	public static bool TryParse(string fraction, out Fraction frac) {
		try {
			frac = Parse(fraction);
			return true;
		} catch {
			frac = null;
			return false;
		}
	}

	public static implicit operator Fraction(int value) {
		return new Fraction(value, 1);
	}

	public static implicit operator Fraction(double value) {
		var split = value.ToString().Split('.');

		if (split.Length < 2)
			if (value.ToString().Split(',').Length < 2)
				return (int)value;
			else
				split = value.ToString().Split(',');

		var floatingPointNums = split[1].Length;

		return new Fraction((int)(value * Math.Pow(10, floatingPointNums)), (int)Math.Pow(10, floatingPointNums));
	}

	// Addition
	public static Fraction operator +(Fraction a, Fraction b) {
		System.Numerics.BigInteger numerator = a._numerator * b._denominator + b._numerator * a._denominator;
		System.Numerics.BigInteger denominator = a._denominator * b._denominator;
		return new Fraction(numerator, denominator);
	}

	// Subtraction
	public static Fraction operator -(Fraction a, Fraction b) {
		System.Numerics.BigInteger numerator = a._numerator * b._denominator - b._numerator * a._denominator;
		System.Numerics.BigInteger denominator = a._denominator * b._denominator;
		return new Fraction(numerator, denominator);
	}

	// Multiplication
	public static Fraction operator *(Fraction a, Fraction b) {
		return new Fraction(a._numerator * b._numerator, a._denominator * b._denominator);
	}

	// Division
	public static Fraction operator /(Fraction a, Fraction b) {
		if (b._numerator == 0)
			throw new DivideByZeroException("Cannot divide by zero.");
		return new Fraction(a._numerator * b._denominator, a._denominator * b._numerator);
	}

	public static bool operator ==(Fraction a, int b) {
		return a == new Fraction(b, 1);
	}

	public static bool operator !=(Fraction a, int b) {
		return !(a == b);
	}

	public static bool operator ==(Fraction a, Fraction b) {
		return a._numerator == b._numerator && a._denominator == b._denominator;
	}

	public static bool operator !=(Fraction a, Fraction b) {
		return !(a == b);
	}

	public static bool operator <(Fraction a, Fraction b) {
		return a.ToDouble() < b.ToDouble();
	}

	public static bool operator >(Fraction a, Fraction b) {
		return a.ToDouble() > b.ToDouble();
	}

	public static bool operator <=(Fraction a, Fraction b) {
		return a.ToDouble() <= b.ToDouble();
	}
	public static bool operator >=(Fraction a, Fraction b) {
		return a.ToDouble() >= b.ToDouble();
	}

	public Fraction Pow(int fac) {
		var frac = this;

		for (int i = 0; i < fac; i++)
			frac *= this;

		return frac;
	}

	public Fraction Abs() {
		return new Fraction(_numerator >= 0 ? _numerator : _numerator * -1, _denominator >= 0 ? _denominator : _denominator * -1);
	}

	// ToString for displaying the fraction
	public override string ToString() {
		return _denominator == 1 ? _numerator.ToString() : $"{_numerator} / {_denominator}";
	}

	// Convert fraction to floating point (optional)
	public double ToDouble() {
		return (double)_numerator / (double)_denominator;
	}
}