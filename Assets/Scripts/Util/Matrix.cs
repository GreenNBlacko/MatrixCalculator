using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Matrix<T> {
	public Vector2 format => new Vector2(columns.Count, (columns.Count > 0 ? columns[0].rows.Count : 0));
	public bool square => format.x == format.y;

	public List<Column<T>> columns = new List<Column<T>>();

	public List<List<T>> GetMatrix() {
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

	public Matrix<T> CopyMatrix() {
		var matrix = new Matrix<T>();

		for (int i = 0; i < columns.Count; i++) {
			var column = new Column<T>();

			for (int o = 0; o < columns[0].rows.Count; o++) {
				column.rows.Add(new Row<T> { Value = columns[i].rows[o].Value });
			}

			matrix.columns.Add(column);
		}

		return matrix;
	}

	public virtual Matrix<T> RemoveRowAndColumn(int i, int o) {
		var newMatrix = CopyMatrix();

		newMatrix.columns.RemoveAt(i);

		for (i = 0; i < newMatrix.columns.Count; i++) {
			newMatrix.columns[i].rows.RemoveAt(o);
		}

		return newMatrix;
	}

	public T getValue(int i, int o) {
		return columns[i].rows[o].Value;
	}

	public Matrix<T> SwapRows(int i, int o) {
		foreach (var column in columns) {
			var el = column.rows[i];

			column.rows[i] = column.rows[o];
			column.rows[o] = el;
		}

		return this;
	}

	public virtual Matrix<T> Augment(Matrix<T> matrixB) {
		var augment = CopyMatrix();

		augment.columns.AddRange(matrixB.columns);

		return augment;
	}

	public Matrix() { }

	public Matrix(List<List<T>> reference) {
		for (int i = 0; i < reference.Count; i++) {
			var col = new Column<T>();

			for (int o = 0; o < reference[0].Count; o++) {
				var row = new Row<T>();

				row.Value = reference[i][o];

				col.rows.Add(row);
			}

			columns.Add(col);
		}
	}

	public Matrix(int n, int m, T defaultValue = default) {
		for (int i = 0; i < n; i++) {
			var col = new Column<T>();

			for (int o = 0; o < m; o++) {
				var row = new Row<T>();

				row.Value = defaultValue;

				col.rows.Add(row);
			}
			columns.Add(col);
		}
	}

	public Column<T> this[int index] { get { return columns[index]; } set { } }
}

public class FractionMatrix : Matrix<Fraction> {

	public int rank => GetRank();

	public FractionMatrix() {

	}

	public FractionMatrix(List<List<Fraction>> reference) {
		columns = new Matrix<Fraction>(reference).columns;
	}

	public FractionMatrix(int n, int m, Fraction defaultValue = default) : base(n, m, defaultValue) { }

	public FractionMatrix FlipMatrix() {
		for (int i = 0; i < columns.Count; i++) {
			var list = columns[i];

			for (int o = 0; o < list.rows.Count; o++) {
				list.rows[o].Value *= new Fraction(-1, 1);
			}
		}

		return this;
	}

	private int GetRank() {
		if (!square) return -1;

		if (~this != 0) return (int)format.x;

		int r = 0;

		for (int i = 0; i < format.x; i++) {
			for (int o = 0; o < format.y; o++) {
				FractionMatrix minor = (FractionMatrix)RemoveRowAndColumn(i, o);

				if ((r = minor.rank) != 0) return r;

				r = -1;
			}
		}

		return r;
	}

	public Fraction Adjunct(int i, int j) {
		return (int)Math.Pow(-1, i + j + 2) * ~new FractionMatrix(RemoveRowAndColumn(i, j).GetMatrix());
	}

	public FractionMatrix Inverse() {
		// Step 1: Calculate the determinant
		var det = ~this;

		// Step 2: Check for singularity
		if (det == 0) {
			return new FractionMatrix(); // Return an empty matrix if singular
		}

		// Step 3: Create an augmented matrix [A | I]
		int n = this.columns.Count; // Assuming columns is the number of rows
		var augmented = new Matrix<Fraction>(GetMatrix());

		// Append identity matrix to the augmented matrix
		for (int i = 0; i < n; i++) {
			var identityColumn = new Column<Fraction>();
			for (int j = 0; j < n; j++) {
				identityColumn.rows.Add(new Row<Fraction> { Value = (i == j) ? 1 : 0 });
			}
			augmented.columns.Add(identityColumn);
		}

		// Step 4: Perform Gaussian elimination
		for (int i = 0; i < n; i++) {
			// Find the row with the largest element in the current column
			Fraction maxElement = augmented.getValue(i, i).Abs();
			int maxRow = i;
			for (int k = i + 1; k < n; k++) {
				if (augmented.getValue(k, i).Abs() > maxElement) {
					maxElement = augmented.getValue(k, i).Abs();
					maxRow = k;
				}
			}

			// Swap the current row with the row containing the principal element
			if (maxRow != i) {
				augmented.SwapRows(i, maxRow);
			}

			// Make the diagonal element 1
			Fraction diagElement = augmented.getValue(i, i);
			if (diagElement == 0) {
				throw new InvalidOperationException("Matrix is singular and cannot be inverted.");
			}

			// Normalize the current row
			for (int j = 0; j < augmented.columns.Count; j++) {
				augmented.columns[j].rows[i].Value /= diagElement;
			}

			// Eliminate all other elements in the current column
			for (int k = 0; k < n; k++) {
				if (k != i) {
					Fraction factor = augmented.getValue(i, k); // Get the value from column i, row k
					for (int j = 0; j < augmented.columns.Count; j++) {
						// Ensure we access the correct row for the current column j
						if (k < augmented.columns[j].rows.Count) {
							Fraction value = augmented.getValue(j, i); // Get the value from column j, row i
							augmented.columns[j].rows[k].Value -= factor * value;
						}
					}
				}
			}
		}

		// Step 5: Extract the inverse matrix from the augmented matrix
		var result = augmented.GetMatrix();

		result.RemoveRange(0, 3);


		// Create the inverse matrix using the extracted data
		var inverseMatrix = new FractionMatrix(result);

		return inverseMatrix;
	}


	public FractionMatrix GaussianEliminate() {
		int rowCount = (int)format.y;
		int colCount = (int)format.x;

		for (int pivot = 0; pivot < Math.Min(rowCount, colCount); pivot++) {
			int maxRow = pivot;
			for (int row = pivot + 1; row < rowCount; row++) {
				if (columns[pivot].rows[row].Value.Abs() > columns[pivot].rows[maxRow].Value.Abs())
					maxRow = row;
			}

			if (maxRow != pivot) {
				SwapRows(pivot, maxRow);
			}

			for (int row = pivot + 1; row < rowCount; row++) {
				if (columns[pivot].rows[pivot].Value == 0) continue;

				Fraction factor = columns[pivot].rows[row].Value / columns[pivot].rows[pivot].Value;
				for (int col = pivot; col < colCount; col++) {
					columns[col].rows[row].Value -= factor * columns[col].rows[pivot].Value;
				}
			}
		}

		return this;
	}

	public override Matrix<Fraction> Augment(Matrix<Fraction> matrixB) {
		return new FractionMatrix(base.Augment(matrixB).GetMatrix());
	}

	public List<Fraction> BackSubstitute() {
		int rowCount = (int)format.y;
		int colCount = (int)format.x;
		var solution = new List<Fraction>(new Fraction[rowCount]);

		for (int row = rowCount - 1; row >= 0; row--) {
			Fraction sum = columns[colCount - 1].rows[row].Value;

			for (int col = row + 1; col < rowCount; col++) {
				sum -= columns[col].rows[row].Value * solution[col];
			}

			solution[row] = sum / columns[row].rows[row].Value;
		}

		return solution;
	}

	public FractionMatrix PrincipalElementMethod() {
		var augmentedMatrix = CopyMatrix(); // Create a copy to work with
		int n = augmentedMatrix.columns[0].rows.Count; // Total number of rows in the original matrix

		List<List<Fraction>> principalRows = new();

		while (augmentedMatrix.columns[0].rows.Count != principalRows.Count) {
			// Step 1: Identify the principal element (the largest absolute element in the entire matrix)
			Fraction maxElement = 0;
			int principalRow = 0;
			int principalCol = 0;

			for (int i = 0; i < augmentedMatrix.columns.Count - 1; i++) {
				for (int j = 0; j < augmentedMatrix.columns[i].rows.Count; j++) {
					Fraction absValue = augmentedMatrix.getValue(i, j).Abs();
					if (absValue > maxElement) {
						maxElement = absValue;
						principalRow = j;
						principalCol = i;
					}
				}
			}

			if (maxElement == 0) {
				throw new InvalidOperationException("Matrix is singular and cannot be solved using the principal element method.");
			}

			// Step 2: Perform row operations using the principal element
			for (int i = 0; i < augmentedMatrix.columns[0].rows.Count; i++) {
				if (i != principalRow) {
					// Calculate the multiplier for the current row
					Fraction currentValue = augmentedMatrix.getValue(principalCol, i);
					Fraction multiplier = currentValue / augmentedMatrix.getValue(principalCol, principalRow);

					for (int j = 0; j < augmentedMatrix.columns.Count; j++) {
						Fraction principalValue = augmentedMatrix.getValue(j, principalRow);
						// Update the value in the current row based on the multiplier
						augmentedMatrix.columns[j].rows[i].Value = augmentedMatrix.getValue(j, i) - (multiplier * principalValue);
					}
				}
			}

			var _row = new List<Fraction>();

			foreach (var col in augmentedMatrix.columns) {
				_row.Add(col.rows[principalRow].Value);
				col.rows[principalRow].Value = 0;
			}

			principalRows.Add(_row);



			foreach (var row in augmentedMatrix.columns[principalCol].rows) {
				row.Value = 0;
			}
		}

		var principalColumns = new List<List<Fraction>>();

		for (int i = 0; i < principalRows[0].Count; i++) {
			var principalRow = new List<Fraction>();

			for (int o = 0; o < principalRows.Count; o++) {
				principalRow.Add(principalRows[o][i]);
			}

			principalColumns.Add(principalRow);
		}

		return new FractionMatrix(new List<List<Fraction>> { new FractionMatrix(principalColumns).BackSubstitutePrincipalElement() });
	}

	public List<Fraction> BackSubstitutePrincipalElement() {
		int n = columns.Count - 1; // Matrix has n variables and 1 extra column for the constants
		var solutions = new List<Fraction>(new Fraction[n]);

		for (int i = 0; i < solutions.Count; i++)
			solutions[i] = 0;

		// Perform back substitution, starting from the last row and going upwards
		for (int row = columns[0].rows.Count - 1; row >= 0; row--) {
			// Identify the leading variable in this row
			int leadingVariableIndex = -1;
			Fraction constant = getValue(n, row); // The last column contains the constants (right-hand side)
			Fraction sum = constant;

			// Calculate the sum of known variables (those with already computed solutions)
			for (int col = 0; col < n; col++) {
				Fraction coeff = getValue(col, row);

				if (coeff != 0 && solutions[col] != 0) // Only consider already solved variables
				{
					sum -= coeff * solutions[col];
				} else if (coeff != 0 && leadingVariableIndex == -1) // First non-zero element in this row is the leading variable
				  {
					leadingVariableIndex = col;
				}
			}

			// Now solve for the leading variable
			if (leadingVariableIndex != -1) {
				Fraction leadingCoeff = getValue(leadingVariableIndex, row);

				if (leadingCoeff == 0) {
					throw new InvalidOperationException("Matrix has no unique solution");
				}

				solutions[leadingVariableIndex] = sum / leadingCoeff;
			} else {
				throw new InvalidOperationException("Matrix has no unique solution");
			}
		}

		return solutions;
	}

	public FractionMatrix SolveJacobi(int maxIterations = 3, double tolerance = 0.01) {
		int n = columns[0].rows.Count;  // Number of variables
		FractionMatrix X = new FractionMatrix(1, n);  // Initialize solution matrix X with zeros

		// Initial guess for solution (all zeros)
		List<Fraction> x = new List<Fraction>(new Fraction[n]);
		for (int i = 0; i < n; i++) {
			x[i] = 0;  // Starting guess for each variable is zero
		}

		// Temporary storage for the new solution after each iteration
		List<Fraction> xNew = new List<Fraction>(new Fraction[n]);

		// Perform Jacobi iteration
		for (int iteration = 0; iteration < maxIterations; iteration++) {
			for (int i = 0; i < n; i++) {
				// Retrieve the diagonal element A_ii
				Fraction A_ii = getValue(i, i);
				if (A_ii == 0) {
					throw new InvalidOperationException("Zero on the diagonal, Jacobi method cannot proceed.");
				}

				// Retrieve the right-hand side constant b_i (the last column in the augmented matrix)
				Fraction b_i = getValue(n, i);

				// Sum the off-diagonal elements A_ij * x_j (for j != i)
				Fraction sum = 0;
				for (int j = 0; j < n; j++) {
					if (i != j) {
						Fraction A_ij = getValue(j, i);  // A_ij value from the coefficient matrix
						sum += A_ij * x[j];  // Multiply A_ij by the previous solution x_j
					}
				}

				// Calculate the new value for x_i^(k+1)
				xNew[i] = (b_i - sum) / A_ii;
			}

			// Check for convergence by comparing the difference between x and xNew
			bool hasConverged = true;
			for (int i = 0; i < n; i++) {
				if (Math.Abs((xNew[i] - x[i]).ToDouble()) > tolerance) {
					hasConverged = false;
					break;
				}
			}

			// Update x to xNew
			for (int i = 0; i < n; i++) {
				x[i] = xNew[i];
			}

			// Break the loop if the solution has converged
			if (hasConverged) {
				break;
			}
		}

		// Assign the solution vector x to the result matrix X
		for (int i = 0; i < n; i++) {
			X[0][i] = x[i];  // Store each value from x into the result matrix X
		}

		return X;  // Return the solution matrix X
	}

	public FractionMatrix SolveSOR(double relaxationFactor = 1.25, int maxIterations = 3, double tolerance = 0.01) {
		int n = columns[0].rows.Count;  // Number of variables (assuming square matrix)
		FractionMatrix X = new FractionMatrix(1, n);  // Solution matrix X with initial guess of zeros

		// Initial guess for solution (all zeros)
		List<Fraction> x = new List<Fraction>(new Fraction[n]);
		for (int i = 0; i < n; i++) {
			x[i] = 0;  // Starting guess for each variable is zero
		}

		// Perform SOR iteration
		for (int iteration = 0; iteration < maxIterations; iteration++) {
			bool hasConverged = true;

			for (int i = 0; i < n; i++) {
				// Retrieve the diagonal element A_ii
				Fraction A_ii = getValue(i, i);
				if (A_ii == 0) {
					throw new InvalidOperationException("Zero on the diagonal, SOR method cannot proceed.");
				}

				// Retrieve the right-hand side constant b_i (the last column in the augmented matrix)
				Fraction b_i = getValue(n, i);

				// Sum the off-diagonal elements A_ij * x_j (for j != i)
				Fraction sum = 0;
				for (int j = 0; j < n; j++) {
					if (i != j) {
						Fraction A_ij = getValue(j, i);  // A_ij value from the coefficient matrix
						sum += A_ij * x[j];  // Multiply A_ij by the previous solution x_j
					}
				}

				// Update the value of x_i with relaxation
				Fraction newX_i = (b_i - sum) / A_ii;  // Compute the new value for x_i without relaxation
				Fraction relaxedX_i = relaxationFactor * newX_i + (1.0 - relaxationFactor) * x[i];  // Apply the relaxation factor

				// Check if the value has converged within the tolerance
				if (Math.Abs((relaxedX_i - x[i]).ToDouble()) > tolerance) {
					hasConverged = false;
				}

				// Update x_i to the new value
				x[i] = relaxedX_i;
			}

			// If all variables have converged, break the loop
			if (hasConverged) {
				break;
			}
		}

		// Store the final solution in the solution matrix X
		for (int i = 0; i < n; i++) {
			X.columns[0].rows[i].Value = x[i];
		}

		return X;  // Return the solution matrix
	}


	public FractionMatrix Transpose() {
		int rowCount = (int)format.y;
		int colCount = (int)format.x;

		var transposedMatrix = new FractionMatrix(new List<List<Fraction>>());

		for (int i = 0; i < colCount; i++) {
			var newRow = new List<Fraction>();

			for (int j = 0; j < rowCount; j++) {
				newRow.Add(columns[i].rows[j].Value);
			}

			transposedMatrix.columns.Add(new Column<Fraction>(newRow));
		}

		return transposedMatrix;
	}

	public override Matrix<Fraction> RemoveRowAndColumn(int i, int o) {
		return new FractionMatrix(base.RemoveRowAndColumn(i, o).GetMatrix());
	}

	public FractionMatrix RemoveRow(int o) {
		var matrix = CopyMatrix();

		foreach (var column in matrix.columns) {
			column.rows.RemoveAt(o);
		}

		return matrix;
	}

	public new FractionMatrix CopyMatrix() {
		return new FractionMatrix(base.CopyMatrix().GetMatrix());
	}

	public static FractionMatrix operator +(FractionMatrix a, FractionMatrix b) {
		for (int i = 0; i < a.columns.Count; i++) {
			for (int o = 0; o < a.columns[0].rows.Count; o++) {
				a.columns[i].rows[o].Value += b.columns[i].rows[o].Value;
			}
		}

		return a;
	}

	public static FractionMatrix operator -(FractionMatrix a, FractionMatrix b) {
		b = b.FlipMatrix();

		for (int i = 0; i < a.columns.Count; i++) {
			for (int o = 0; o < a.columns[0].rows.Count; o++) {
				a.columns[i].rows[o].Value += b.columns[i].rows[o].Value;
			}
		}

		return a;
	}

	public static FractionMatrix operator *(FractionMatrix a, FractionMatrix b) {
		if (a.columns.Count != b.columns[0].rows.Count) {
			throw new InvalidOperationException("Matrix dimensions are incompatible for multiplication.");
		}

		var result = new FractionMatrix(new List<List<Fraction>>());

		for (int i = 0; i < a.columns[0].rows.Count; i++) {
			var resultRow = new List<Fraction>();

			for (int j = 0; j < b.columns.Count; j++) {
				Fraction sum = 0;

				for (int k = 0; k < a.columns.Count; k++) {
					sum += a.columns[k].rows[i].Value * b.columns[j].rows[k].Value;
				}

				resultRow.Add(sum);
			}

			for (int j = 0; j < resultRow.Count; j++) {
				if (result.columns.Count <= j) {
					result.columns.Add(new Column<Fraction>());
				}
				result.columns[j].rows.Add(new Row<Fraction> { Value = resultRow[j] });
			}
		}

		return result;
	}


	public static FractionMatrix operator *(FractionMatrix a, Fraction b) {
		for (int i = 0; i < a.columns.Count; i++) {
			var list1 = a.columns[i];

			for (int o = 0; o < list1.rows.Count; o++) {
				var element1 = list1.rows[o];
				element1.Value *= b;
			}
		}

		return a;
	}

	public static FractionMatrix operator *(Fraction a, FractionMatrix b) {
		return b * a;
	}

	public static Fraction operator ~(FractionMatrix a) {
		switch (a.columns.Count) {
			case 2:
				var a00 = a.getValue(0, 0);
				var a01 = a.getValue(0, 1);
				var a10 = a.getValue(1, 0);
				var a11 = a.getValue(1, 1);

				return a00 * a11 - a10 * a01;

			case 3:
				a00 = a.getValue(0, 0); a10 = a.getValue(1, 0); var a20 = a.getValue(2, 0);
				a01 = a.getValue(0, 1); a11 = a.getValue(1, 1); var a21 = a.getValue(2, 1);
				var a02 = a.getValue(0, 2); var a12 = a.getValue(1, 2); var a22 = a.getValue(2, 2);

				return (a00 * a11 * a22) + (a01 * a12 * a20) + (a02 * a10 * a21) - (a02 * a11 * a20) - (a01 * a10 * a22) - (a00 * a12 * a21);

			case 4:
				return (a.getValue(0, 0) * a.Adjunct(0, 0)) + (a.getValue(0, 1) * a.Adjunct(0, 1)) + (a.getValue(0, 2) * a.Adjunct(0, 2)) + (a.getValue(0, 3) * a.Adjunct(0, 3));
		}

		return -1;

	}
}

public class StringMatrix : Matrix<string> {
	public List<EquationBlock> rank => GetRank();


	public List<EquationBlock> GetRank() {
		var list = new List<EquationBlock>();

		if (!square)
			return list;



		return list;
	}

	public override Matrix<string> RemoveRowAndColumn(int i, int o) {
		return new StringMatrix(base.RemoveRowAndColumn(i, o).GetMatrix());
	}


	public StringMatrix() {

	}

	public StringMatrix(List<List<string>> reference) {
		columns = new Matrix<string>(reference).columns;
	}

	public StringMatrix FlipMatrix() {
		for (int i = 0; i < columns.Count; i++) {
			var list = columns[i];

			for (int o = 0; o < list.rows.Count; o++) {
				list.rows[o].Value = (Fraction.Parse(list.rows[0].Value) * -1).ToString();
			}
		}

		return this;
	}

	public List<EquationBlock> Adjunct(int i, int j) {
		return EquationScript.BuildEquation()
			.AddExpression("A<sub>" + (i + 1).ToString() + (j + 1).ToString() + "</sub>")
			.AddOperator(EquationBlock.Operators.Equals)
			.AddNumber("(-1)<sup>" + (i + 1).ToString() + " + " + (j + 1).ToString() + "</sup>")
			.AddOperator(EquationBlock.Operators.Multiply)
			.AddDeterminant(new StringMatrix(RemoveRowAndColumn(i, j).GetMatrix()))
			.AddOperator(EquationBlock.Operators.Equals)
			.AddNumber(UtilManager.convertToFractionMatrix(this).Adjunct(i, j))
			.GetEquation();
	}

	public List<EquationBuilder> GaussianEliminateWithSteps() {
		int rowCount = (int)format.y;
		int colCount = (int)format.x;

		List<EquationBuilder> solvingSteps = new List<EquationBuilder>();

		for (int pivot = 0; pivot < Math.Min(rowCount, colCount); pivot++) {
			int maxRow = pivot;
			for (int row = pivot + 1; row < rowCount; row++) {
				if (Fraction.Parse(columns[pivot].rows[row].Value).Abs() > Fraction.Parse(columns[pivot].rows[maxRow].Value).Abs())
					maxRow = row;
			}

			if (maxRow != pivot) {
				SwapRows(pivot, maxRow);
				solvingSteps.Add(CreateStep($"A<sub>{pivot + 1}\u2194{maxRow + 1}</sub>"));
			}

			for (int row = pivot + 1; row < rowCount; row++) {
				Fraction pivotValue = Fraction.Parse(columns[pivot].rows[pivot].Value);
				if (pivotValue == 0) continue;

				Fraction factor = Fraction.Parse(columns[pivot].rows[row].Value) / pivotValue;
				for (int col = pivot; col < colCount; col++) {
					Fraction newValue = Fraction.Parse(columns[col].rows[row].Value) - factor * Fraction.Parse(columns[col].rows[pivot].Value);
					columns[col].rows[row].Value = newValue.ToString();
				}

				solvingSteps.Add(CreateStep($"R<sub>{row + 1}</sub> - ({factor}) * R<sub>{pivot + 1}</sub>"));
			}
		}

		return solvingSteps;
	}

	public List<EquationBuilder> BackSubstituteWithSteps() {
		int rowCount = (int)format.y;
		int colCount = (int)format.x;

		List<EquationBuilder> solvingSteps = new List<EquationBuilder>();
		List<string> solutions = new List<string>(new string[rowCount]);

		for (int row = rowCount - 1; row >= 0; row--) {
			Fraction sum = 0;
			Fraction currentPivot = Fraction.Parse(columns[row].rows[row].Value);
			if (currentPivot == 0) continue;

			for (int col = row + 1; col < colCount - 1; col++) {
				sum += Fraction.Parse(columns[col].rows[row].Value) * Fraction.Parse(solutions[col]);
			}

			Fraction constant = Fraction.Parse(columns[colCount - 1].rows[row].Value);
			Fraction solution = (constant - sum) / currentPivot;

			solutions[row] = solution.ToString();

			solvingSteps.Add(CreateStep($"x<sup>{row + 1}</sup> = ({constant} - {sum}) / {currentPivot}"));
		}

		return solvingSteps;
	}

	public Table PrincipalElementMethod() {
		var regular = new Color32(0, 0, 0, 0);
		var selected = new Color32(255, 134, 0, 11);

		var augmentedMatrix = UtilManager.convertToFractionMatrix(this); // Create a copy to work with
		int n = columns[0].rows.Count; // Total number of columns in the original matrix

		var table = new Table();

		var cols = new List<(string label, Color32 color)> { ("Row", regular), ("Multiplier", regular) };

		for (int i = 0; i < n; i++) {
			cols.Add(($"x<sub>{i + 1}</sub>", regular));
		}

		cols.Add(("L.N.", regular));

		table.AddRow(cols);

		var step = 0;

		while (step != n) {
			for (int i = 0; i < augmentedMatrix.columns[0].rows.Count; i++) {
				table.AddRow(cols.Count);
			}

			// Step 1: Identify the principal element (the largest absolute element in the entire matrix)
			Fraction maxElement = 0;
			int principalRow = 0;
			int principalCol = 0;

			for (int i = 0; i < augmentedMatrix.columns.Count - 1; i++) {
				for (int j = 0; j < augmentedMatrix.columns[i].rows.Count; j++) {
					if (augmentedMatrix.columns[i].rows[j].Value == 0)
						continue;

					Fraction absValue = augmentedMatrix.getValue(i, j).Abs();
					if (absValue > maxElement) {
						maxElement = absValue;
						principalRow = j;
						principalCol = i;
					}
				}
			}

			if (maxElement == 0) {
				throw new InvalidOperationException("Matrix is singular and cannot be solved using the principal element method.");
			}

			for (int i = 0; i < augmentedMatrix.columns[0].rows.Count; i++) {
				var rowMult = 1;

				for (int p = 0; p < step; p++) {
					rowMult += n - p;
				}

				table.SetValue(i + rowMult, 0, ((i + 1).ToString(), regular));
				for (int o = 0; o < augmentedMatrix.columns.Count; o++) {
					table.SetValue(i + rowMult, o + 2, (augmentedMatrix.columns[o].rows[i].Value != 0 ? augmentedMatrix.columns[o].rows[i].Value.ToString() : "", (augmentedMatrix.columns[o].rows[i].Value == 0 || o == principalCol || i == principalRow) ? selected : regular));
				}
			}

			// Step 2: Perform row operations using the principal element
			for (int i = 0; i < augmentedMatrix.columns[0].rows.Count; i++) {
				var rowMult = 1;

				for (int p = 0; p < step; p++) {
					rowMult += n - p;
				}

				table.SetValue(i + rowMult, 1, ("", regular));

				if (augmentedMatrix.getValue(principalCol, i) == 0)
					continue;

				if (i != principalRow) {
					Fraction multiplier = augmentedMatrix.getValue(principalCol, i) / augmentedMatrix.getValue(principalCol, principalRow);

					table.SetValue(i + rowMult, 1, (multiplier.ToString(), regular));

					for (int j = 0; j < augmentedMatrix.columns.Count; j++) {
						if (augmentedMatrix.columns[j].rows[i].Value == 0)
							continue;

						Fraction currentValue = augmentedMatrix.getValue(j, i);
						Fraction principalValue = augmentedMatrix.getValue(j, principalRow);
						augmentedMatrix.columns[j].rows[i].Value = currentValue - (multiplier * principalValue);
					}
				}
			}

			// Step 3: Remove the principal row and column
			augmentedMatrix = augmentedMatrix.RemoveRow(principalRow);
			foreach (var row in augmentedMatrix.columns[principalCol].rows) {
				row.Value = 0;
			}

			step++;
		}

		return table;
	}

	public Table SolveJacobi(int maxIterations = 3, double tolerance = 0.01) {
		int n = columns[0].rows.Count;  // Number of variables
		FractionMatrix X = new(1, n);  // Initialize solution matrix X with zeros

		var regular = new Color32(0, 0, 0, 0);

		var matrix = UtilManager.convertToFractionMatrix(this);

		var table = new Table();

		var cols = new List<(string label, Color32 color)> { ("Iteration", regular) };

		for (int i = 0; i < n; i++) {
			cols.Add(($"x<sub>{i + 1}</sub>", regular));
		}

		table.AddRow(cols);

		// Initial guess for solution (all zeros)
		List<Fraction> x = new List<Fraction>(new Fraction[n]);
		for (int i = 0; i < n; i++) {
			x[i] = 0;  // Starting guess for each variable is zero
		}

		// Temporary storage for the new solution after each iteration
		List<Fraction> xNew = new List<Fraction>(new Fraction[n]);

		// Perform Jacobi iteration
		for (int iteration = 0; iteration < maxIterations; iteration++) {
			for (int i = 0; i < n; i++) {
				// Retrieve the diagonal element A_ii
				Fraction A_ii = matrix.getValue(i, i);
				if (A_ii == 0) {
					throw new InvalidOperationException("Zero on the diagonal, Jacobi method cannot proceed.");
				}

				// Retrieve the right-hand side constant b_i (the last column in the augmented matrix)
				Fraction b_i = matrix.getValue(n, i);

				// Sum the off-diagonal elements A_ij * x_j (for j != i)
				Fraction sum = 0;
				for (int j = 0; j < n; j++) {
					if (i != j) {
						Fraction A_ij = matrix.getValue(j, i);  // A_ij value from the coefficient matrix
						sum += A_ij * x[j];  // Multiply A_ij by the previous solution x_j
					}
				}

				// Calculate the new value for x_i^(k+1)
				xNew[i] = (b_i - sum) / A_ii;
			}

			// Check for convergence by comparing the difference between x and xNew
			bool hasConverged = true;
			for (int i = 0; i < n; i++) {
				if (Math.Abs((xNew[i] - x[i]).ToDouble()) > tolerance) {
					hasConverged = false;
					break;
				}
			}

			// Update x to xNew
			for (int i = 0; i < n; i++) {
				x[i] = xNew[i];
			}

			// Break the loop if the solution has converged
			if (hasConverged) {
				break;
			}

			table.AddRow(cols.Count);

			table.SetValue(1 + iteration, 0, ((iteration + 1).ToString(), regular));
			for (int o = 0; o < x.Count; o++) {
				table.SetValue(1 + iteration, o + 1, (x[o].ToString(), regular));
			}
		}
		return table;
	}

	public Table SolveSOR(int maxIterations = 3, double tolerance = 0.01, double relaxationFactor = 1.25) {
		int n = columns[0].rows.Count;  // Number of variables
		FractionMatrix X = new FractionMatrix(1, n);  // Initialize solution matrix X with zeros

		var regular = new Color32(0, 0, 0, 0);

		var matrix = UtilManager.convertToFractionMatrix(this);

		var table = new Table();

		var cols = new List<(string label, Color32 color)> { ("Iteration", regular) };

		for (int i = 0; i < n; i++) {
			cols.Add(($"x<sub>{i + 1}</sub>", regular));
		}

		table.AddRow(cols);

		// Initial guess for solution (all zeros)
		List<Fraction> x = new List<Fraction>(new Fraction[n]);
		for (int i = 0; i < n; i++) {
			x[i] = 0;  // Starting guess for each variable is zero
		}

		// Perform SOR iteration
		for (int iteration = 0; iteration < maxIterations; iteration++) {
			bool hasConverged = true;

			for (int i = 0; i < n; i++) {
				// Retrieve the diagonal element A_ii
				Fraction A_ii = matrix.getValue(i, i);
				if (A_ii == 0) {
					throw new InvalidOperationException("Zero on the diagonal, SOR method cannot proceed.");
				}

				// Retrieve the right-hand side constant b_i (the last column in the augmented matrix)
				Fraction b_i = matrix.getValue(n, i);

				// Sum the off-diagonal elements A_ij * x_j (for j != i)
				Fraction sum = 0;
				for (int j = 0; j < n; j++) {
					if (i != j) {
						Fraction A_ij = matrix.getValue(j, i);  // A_ij value from the coefficient matrix
						sum += A_ij * x[j];  // Multiply A_ij by the previous solution x_j
					}
				}

				// Update the value of x_i with relaxation
				Fraction newX_i = (b_i - sum) / A_ii;  // Compute the new value for x_i without relaxation
				Fraction relaxedX_i = relaxationFactor * newX_i + (1.0 - relaxationFactor) * x[i];  // Apply the relaxation factor

				// Check if the value has converged within the tolerance
				if (Math.Abs((relaxedX_i - x[i]).ToDouble()) > tolerance) {
					hasConverged = false;
				}

				// Update x_i to the new value
				x[i] = relaxedX_i;
			}

			// Add current iteration values to the table
			table.AddRow(cols.Count);


			table.SetValue(iteration + 1, 0, ((iteration + 1).ToString(), regular));

			for (int i = 0; i < n; i++) {
				table.SetValue(iteration + 1, i + 1, (x[i].ToString(), regular));
			}

			// Break the loop if the solution has converged
			if (hasConverged) {
				break;
			}
		}

		return table;
	}


	private EquationBuilder CreateStep(string description) {
		return EquationScript.BuildEquation()
			.AddExpression(description)
			.AddOperator(EquationBlock.Operators.Equals)
			.AddAugmented(this);
	}

	public override Matrix<string> Augment(Matrix<string> matrixB) {
		return new StringMatrix(base.Augment(matrixB).GetMatrix());
	}

	public static StringMatrix operator +(StringMatrix a, StringMatrix b) {
		for (int i = 0; i < a.columns.Count; i++) {
			for (int o = 0; o < a.columns[0].rows.Count; o++) {
				a.columns[i].rows[o].Value += " + " + UtilManager.formatNumber(Fraction.Parse(b.columns[i].rows[o].Value));
			}
		}

		return a;
	}

	public static StringMatrix operator -(StringMatrix a, StringMatrix b) {
		b = b.FlipMatrix();

		for (int i = 0; i < a.columns.Count; i++) {
			for (int o = 0; o < a.columns[0].rows.Count; o++) {
				a.columns[i].rows[o].Value += " + " + UtilManager.formatNumber(Fraction.Parse(b.columns[i].rows[o].Value));
			}
		}

		return a;
	}

	public static StringMatrix operator *(StringMatrix a, StringMatrix b) {
		if (a.columns[0].rows.Count == b.columns.Count) {
			var res = new StringMatrix(new List<List<string>>());

			for (int i = 0; i < a.columns[0].rows.Count; i++) {
				var results = new Column<string>();

				for (int o = 0; o < b.columns.Count; o++) {
					var result = new Row<string> { Value = "" };

					for (int p = 0; p < a.columns.Count; p++) {
						result.Value += string.Format("{0} * {1}" + (p < a.columns[0].rows.Count - 1 ? " + " : ""), a.columns[i].rows[p].Value, b.columns[p].rows[o].Value);
					}

					results.rows.Add(result);
				}

				res.columns.Add(results);
			}

			return res;
		}

		for (int i = 0; i < a.columns.Count; i++) {
			for (int o = 0; o < a.columns[0].rows.Count; o++) {
				a.columns[i].rows[o].Value = " * " + UtilManager.formatNumber(Fraction.Parse(b.columns[i].rows[o].Value));
			}
		}

		return a;
	}

	public static StringMatrix operator *(StringMatrix a, Fraction b) {
		for (int i = 0; i < a.columns.Count; i++) {
			var list1 = a.columns[i];

			for (int o = 0; o < list1.rows.Count; o++) {
				var element1 = list1.rows[o];
				element1.Value = string.Format(" {0} * {1}", element1.Value, UtilManager.formatNumber(b));
			}
		}

		return a;
	}

	public static List<EquationBlock> operator ~(StringMatrix a) {
		if (a.columns.Count != a.columns[0].rows.Count)
			return EquationScript.BuildEquation()
				.AddExpression("|A|")
				.AddOperator(EquationBlock.Operators.Equals)
				.AddExpression("Not possible")
				.GetEquation();

		switch (a.columns.Count) {
			case 2:
				var a00 = Fraction.Parse(a.getValue(0, 0));
				var a01 = Fraction.Parse(a.getValue(0, 1));
				var a10 = Fraction.Parse(a.getValue(1, 0));
				var a11 = Fraction.Parse(a.getValue(1, 1));

				return EquationScript.BuildEquation()
					.AddExpression("|A|")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddExpression("(")
					.AddNumber(a00)
					.AddOperator(EquationBlock.Operators.Multiply)
					.AddNumber(UtilManager.formatNumber(a11))
					.AddExpression(")")
					.AddOperator(EquationBlock.Operators.Minus)
					.AddExpression("(")
					.AddNumber(a10)
					.AddOperator(EquationBlock.Operators.Multiply)
					.AddNumber(UtilManager.formatNumber(a01))
					.AddExpression(")")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddNumber(a00 * a11)
					.AddOperator(EquationBlock.Operators.Minus)
					.AddNumber(UtilManager.formatNumber(a10 * a01))
					.AddOperator(EquationBlock.Operators.Equals)
					.AddNumber((a00 * a11) - (a10 * a01))
					.GetEquation();

			case 3:
				var matrix = UtilManager.convertToFractionMatrix(a).GetMatrix();

				string s_op_formatting = " ( {0} * {1} * {2} ) ";
				string s_op1 = string.Format(s_op_formatting, UtilManager.formatNumber(matrix[0][0]), UtilManager.formatNumber(matrix[1][1]), UtilManager.formatNumber(matrix[2][2]));
				string s_op2 = string.Format(s_op_formatting, UtilManager.formatNumber(matrix[0][1]), UtilManager.formatNumber(matrix[1][2]), UtilManager.formatNumber(matrix[2][0]));
				string s_op3 = string.Format(s_op_formatting, UtilManager.formatNumber(matrix[0][2]), UtilManager.formatNumber(matrix[1][0]), UtilManager.formatNumber(matrix[2][1]));

				string s_op4 = string.Format(s_op_formatting, UtilManager.formatNumber(matrix[0][2]), UtilManager.formatNumber(matrix[1][1]), UtilManager.formatNumber(matrix[2][0]));
				string s_op5 = string.Format(s_op_formatting, UtilManager.formatNumber(matrix[0][1]), UtilManager.formatNumber(matrix[1][0]), UtilManager.formatNumber(matrix[2][2]));
				string s_op6 = string.Format(s_op_formatting, UtilManager.formatNumber(matrix[0][0]), UtilManager.formatNumber(matrix[1][2]), UtilManager.formatNumber(matrix[2][1]));

				return EquationScript.BuildEquation()
					.AddExpression("|A|")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddNumber(s_op1)
					.AddOperator(EquationBlock.Operators.Plus)
					.AddNumber(s_op2)
					.AddOperator(EquationBlock.Operators.Plus)
					.AddNumber(s_op3)
					.AddOperator(EquationBlock.Operators.Minus)
					.AddNumber(s_op4)
					.AddOperator(EquationBlock.Operators.Minus)
					.AddNumber(s_op5)
					.AddOperator(EquationBlock.Operators.Minus)
					.AddNumber(s_op6)
					.AddOperator(EquationBlock.Operators.Equals)
					.AddNumber(UtilManager.formatNumber(matrix[0][0] * matrix[1][1] * matrix[2][2]))
					.AddOperator(EquationBlock.Operators.Plus)
					.AddNumber(UtilManager.formatNumber(matrix[0][1] * matrix[1][2] * matrix[2][0]))
					.AddOperator(EquationBlock.Operators.Plus)
					.AddNumber(UtilManager.formatNumber(matrix[0][2] * matrix[1][0] * matrix[2][1]))
					.AddOperator(EquationBlock.Operators.Minus)
					.AddNumber(UtilManager.formatNumber(matrix[0][2] * matrix[1][1] * matrix[2][0]))
					.AddOperator(EquationBlock.Operators.Minus)
					.AddNumber(UtilManager.formatNumber(matrix[0][1] * matrix[1][0] * matrix[2][2]))
					.AddOperator(EquationBlock.Operators.Minus)
					.AddNumber(UtilManager.formatNumber(matrix[0][0] * matrix[1][2] * matrix[2][1]))
					.AddOperator(EquationBlock.Operators.Equals)
					.AddNumber(~UtilManager.convertToFractionMatrix(a))
					.GetEquation();

			case 4:
				var Fractiona = UtilManager.convertToFractionMatrix(a);

				return EquationScript.BuildEquation()
					.AddExpression("|A|")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddNumber(UtilManager.formatNumber(a.getValue(0, 0)))
					.AddOperator(EquationBlock.Operators.Multiply)
					.AddExpression("A<sub>11</sub>")
					.AddOperator(EquationBlock.Operators.Plus)
					.AddNumber(UtilManager.formatNumber(a.getValue(0, 1)))
					.AddOperator(EquationBlock.Operators.Multiply)
					.AddExpression("A<sub>12</sub>")
					.AddOperator(EquationBlock.Operators.Plus)
					.AddNumber(UtilManager.formatNumber(a.getValue(0, 2)))
					.AddOperator(EquationBlock.Operators.Multiply)
					.AddExpression("A<sub>13</sub>")
					.AddOperator(EquationBlock.Operators.Plus)
					.AddNumber(UtilManager.formatNumber(a.getValue(0, 3)))
					.AddOperator(EquationBlock.Operators.Multiply)
					.AddExpression("A<sub>14</sub>")
					.AddOperator(EquationBlock.Operators.Plus)
					.AddOperator(EquationBlock.Operators.Equals)
					.AddNumber("( " + a.getValue(0, 0))
					.AddOperator(EquationBlock.Operators.Multiply)
					.AddNumber(Fractiona.Adjunct(0, 0) + " )")
					.AddOperator(EquationBlock.Operators.Plus)
					.AddNumber("( " + a.getValue(0, 1))
					.AddOperator(EquationBlock.Operators.Multiply)
					.AddNumber(Fractiona.Adjunct(0, 1) + " )")
					.AddOperator(EquationBlock.Operators.Plus)
					.AddNumber("( " + a.getValue(0, 2))
					.AddOperator(EquationBlock.Operators.Multiply)
					.AddNumber(Fractiona.Adjunct(0, 2) + " )")
					.AddOperator(EquationBlock.Operators.Plus)
					.AddNumber("( " + a.getValue(0, 3))
					.AddOperator(EquationBlock.Operators.Multiply)
					.AddNumber(Fractiona.Adjunct(0, 3) + " )")
					.AddOperator(EquationBlock.Operators.Plus)
					.AddOperator(EquationBlock.Operators.Equals)
					.AddNumber(~UtilManager.convertToFractionMatrix(a))
					.GetEquation();

			default:
				return EquationScript.BuildEquation()
				.AddExpression("|A|")
				.AddOperator(EquationBlock.Operators.Equals)
				.AddExpression("Unsupported")
				.GetEquation();
		}
	}
}

[System.Serializable]
public class Column<T> {
	public Column() { }
	public Column(List<T> reference) {
		foreach (var item in reference) {
			rows.Add(new Row<T> { Value = item });
		}
	}

	public T this[int index] { get { return rows[index].Value; } set { rows[index].Value = value; } }

	public List<Row<T>> rows = new List<Row<T>>();
}

[System.Serializable]
public class Row<T> {
	public T Value;
}