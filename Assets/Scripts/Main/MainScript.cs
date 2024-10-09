using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainScript : MonoBehaviour {
	[Header("Prefabs")]
	public GameObject MatrixInputDisplay;
	public GameObject EquationDisplay;

	[Header("Lists")]
	public RectTransform InputList;
	public RectTransform MatrixInputList;
	public RectTransform EquationDisplayList;

	[Header("References")]
	public Transform SolutionRegular;
	public Transform SolutionExpanded;
	public Transform SolutionWindow;
	public TMP_Dropdown OperationDropdown;

	[Space(5)]

	public Button ButtonA;
	public Button ButtonB;
	public Button ButtonX;
	public TMP_InputField NumberInput;

	private FractionMatrix fm_A = new FractionMatrix();
	private FractionMatrix fm_B = new FractionMatrix();
	private FractionMatrix fm_X = new FractionMatrix();
	private Fraction num => Fraction.Parse(NumberInput.text);

	public List<string> OperationNames => UtilManager.SeparateChars(Enum.GetNames(typeof(Operations)));

	public enum Operations {
		Add,
		Subtract,
		MultiplyByMatrix,
		MultiplyByNumber,
		GetDeterminant,
		GetRank,
		InverseMatrix,
		InverseMatrixEquation,
		MatrixEquation,
		MainElementMethod,
		JacobiMethod,
		RelaxationMethod
	};

	public enum Matrices {
		A, B, X
	};

	private void Start() {
		var matrix = new FractionMatrix(new List<List<Fraction>> {
			new List<Fraction> { 1, 1, 2, -1},
			new List<Fraction> { -3, -4, -5, -2 },
			new List<Fraction> { 6, 2, 4, 3 },
			new List<Fraction> { 4, 5, 3, 1 },
			new List<Fraction> { 2, 7, 3, -6 }
		});

		var result = matrix.PrincipalElementMethod();

		foreach(var res in result.columns[0].rows) {
			Debug.Log(res.Value);
		}

		UpdateDropdownValues();
		UpdateOperation();
	}

	public void ToggleSolution() {
		if (SolutionWindow.parent == SolutionRegular)
			SolutionWindow.SetParent(SolutionExpanded);
		else
			SolutionWindow.SetParent(SolutionRegular);
	}

	public void Calculate() {
		foreach (RectTransform tr in EquationDisplayList) {
			Destroy(tr.gameObject);
		}

		DisplayInputs();

		DisplayFormula();
	}

	public void DisplayInputs() {
		switch ((Operations)OperationDropdown.value) {
			case Operations.Add:
				var A_DisplayA = CreateEquation();

				A_DisplayA.SetEquations(EquationScript.BuildEquation()
					.AddExpression("A")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_A)
					.GetEquation());

				var A_DisplayB = CreateEquation();

				A_DisplayB.SetEquations(EquationScript.BuildEquation()
					.AddExpression("B")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_B)
					.GetEquation());
				break;

			case Operations.Subtract:
				var S_DisplayA = CreateEquation();

				S_DisplayA.SetEquations(EquationScript.BuildEquation()
					.AddExpression("A")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_A)
					.GetEquation());

				var S_DisplayB = CreateEquation();

				S_DisplayB.SetEquations(EquationScript.BuildEquation()
					.AddExpression("B")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_B)
					.GetEquation());
				break;

			case Operations.MultiplyByMatrix:
				var MM_DisplayA = CreateEquation();

				MM_DisplayA.SetEquations(EquationScript.BuildEquation()
					.AddExpression("A")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_A)
					.GetEquation());

				var MM_DisplayB = CreateEquation();

				MM_DisplayB.SetEquations(EquationScript.BuildEquation()
					.AddExpression("B")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_B)
					.GetEquation());
				break;

			case Operations.MultiplyByNumber:
				var MN_DisplayA = CreateEquation();

				MN_DisplayA.SetEquations(EquationScript.BuildEquation()
					.AddExpression("A")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_X)
					.GetEquation());
				break;

			case Operations.GetDeterminant:
				var D_DisplayA = CreateEquation();


				D_DisplayA.SetEquations(EquationScript.BuildEquation()
					.AddExpression("A")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddDeterminant(fm_X)
					.GetEquation());
				break;

			case Operations.GetRank:
				var R_DisplayA = CreateEquation();


				R_DisplayA.SetEquations(EquationScript.BuildEquation()
					.AddExpression("A")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_X)
					.GetEquation());
				break;

			case Operations.InverseMatrix:
				var FM_DisplayA = CreateEquation();


				FM_DisplayA.SetEquations(EquationScript.BuildEquation()
					.AddExpression("A")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_X)
					.GetEquation());
				break;

			case Operations.InverseMatrixEquation:
				A_DisplayA = CreateEquation();

				A_DisplayA.SetEquations(EquationScript.BuildEquation()
					.AddExpression("A")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_A)
					.GetEquation());

				A_DisplayB = CreateEquation();

				A_DisplayB.SetEquations(EquationScript.BuildEquation()
					.AddExpression("B")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_B)
					.GetEquation());

				break;

			case Operations.RelaxationMethod:
			case Operations.JacobiMethod:
			case Operations.MainElementMethod:
			case Operations.MatrixEquation:
				var ME_DisplayA = CreateEquation();


				ME_DisplayA.SetEquations(EquationScript.BuildEquation()
					.AddExpression("A")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddAugmented(fm_A, fm_B)
					.GetEquation());
				break;
		}
	}

	public void DisplayFormula() {
		switch ((Operations)OperationDropdown.value) {
			case Operations.Add:
				var A_Display = CreateEquation();

				A_Display.SetEquations(EquationScript.BuildEquation()
					.AddExpression("A + B")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(UtilManager.convertToStrMatrix(fm_A) + UtilManager.convertToStrMatrix(fm_B))
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_A + fm_B)
					.GetEquation());
				break;

			case Operations.Subtract:
				var S_Display = CreateEquation();

				S_Display.SetEquations(EquationScript.BuildEquation()
					.AddExpression("A - B")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(UtilManager.convertToStrMatrix(fm_A) - UtilManager.convertToStrMatrix(fm_B))
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_A - fm_B)
					.GetEquation());
				break;

			case Operations.MultiplyByMatrix:
				var MM_Display = CreateEquation();

				MM_Display.SetEquations(EquationScript.BuildEquation()
					.AddExpression("A * B")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(UtilManager.convertToStrMatrix(fm_A) * UtilManager.convertToStrMatrix(fm_B))
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_A * fm_B)
					.GetEquation());
				break;

			case Operations.MultiplyByNumber:
				var MN_Display = CreateEquation();

				MN_Display.SetEquations(EquationScript.BuildEquation()
					.AddExpression("A * " + UtilManager.formatNumber(num))
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(UtilManager.convertToStrMatrix(fm_X) * num)
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_X * num)
					.GetEquation());
				break;

			case Operations.GetDeterminant:
				var D_DisplayA = CreateEquation();

				D_DisplayA.SetEquations(~UtilManager.convertToStrMatrix(fm_X));
				break;

			case Operations.GetRank:
				var R_DisplayR = CreateEquation();

				R_DisplayR.SetEquations(EquationScript.BuildEquation()
					.AddExpression("r(A)")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddNumber((double)fm_X.rank)
					.GetEquation());
				break;

			case Operations.InverseMatrix:
				// Create the initial display for the original matrix A
				var FM_DisplayA = CreateEquation();
				FM_DisplayA.SetEquations(EquationScript.BuildEquation()
					.AddExpression("A")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_X) // Assuming fm_X is the original matrix A
					.GetEquation());

				// Calculate the determinant and create the display
				var det = ~fm_X;

				var FM_DisplayDet = CreateEquation();
				FM_DisplayDet.SetEquations(EquationScript.BuildEquation()
					.AddExpression("|A|")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddExpression(det.ToString())
					.GetEquation());

				// Check if the determinant is non-zero
				if (det != 0) {
					// Create the identity matrix to augment with the original matrix
					var identityMatrix = new FractionMatrix();
					int n = fm_X.columns.Count;

					// Create identity matrix
					for (int i = 0; i < n; i++) {
						var row = new List<Fraction>();
						for (int j = 0; j < n; j++) {
							row.Add(i == j ? 1 : 0);
						}
						identityMatrix.columns.Add(new Column<Fraction>(row));
					}

					// Create the augmented matrix display
					var FM_DisplayAugmented = CreateEquation();
					FM_DisplayAugmented.SetEquations(EquationScript.BuildEquation()
						.AddAugmented(fm_X, identityMatrix) // Use AddAugmented to show [A | I]
						.GetEquation());

					// Iterate through each row to display operations
					for (int i = 0; i < fm_X.columns.Count; i++) {
						// Display row normalization
						var FM_DisplayNormalization = CreateEquation();
						FM_DisplayNormalization.SetEquations(EquationScript.BuildEquation()
							.AddExpression($"Normalize row {i + 1} by dividing by {fm_X.getValue(i, i)}")
							.GetEquation());

						// Display elimination operations
						for (int o = 0; o < fm_X.columns.Count; o++) {
							if (o != i) {
								var factor = fm_X.getValue(o, i);
								var FM_DisplayElimination = CreateEquation();
								FM_DisplayElimination.SetEquations(EquationScript.BuildEquation()
									.AddExpression($"Eliminate element in row {o + 1}, column {i + 1} by subtracting {factor} * row {i + 1}")
									.GetEquation());
							}
						}

						// Optionally, you can display the current state of the matrix after each operation
						var currentMatrixDisplay = CreateEquation();
						currentMatrixDisplay.SetEquations(EquationScript.BuildEquation()
							.AddExpression($"Current state after row operations:")
							.AddMatrix(fm_X) // Assuming fm_X is updated in the loop
							.GetEquation());
					}

					// Display the final inverse matrix
					var FM_DisplaySol = CreateEquation();
					FM_DisplaySol.SetEquations(EquationScript.BuildEquation()
						.AddExpression("A<sup>-1</sup>")
						.AddOperator(EquationBlock.Operators.Equals)
						.AddFraction("1", det.ToString())
						.AddMatrix(fm_X.Inverse()) // Assuming fm_X.Inverse() gives the final result
						.GetEquation());
				} else {
					// If no solution, display the no result message
					var FM_DisplayNoSol = CreateEquation();
					FM_DisplayNoSol.SetEquations(EquationScript.BuildEquation()
						.AddExpression("A<sup>-1</sup>")
						.AddOperator(EquationBlock.Operators.Equals)
						.AddExpression("No result")
						.GetEquation());
				}
				break;


			case Operations.InverseMatrixEquation:
				// Create the initial display for the original matrix A
				FM_DisplayA = CreateEquation();
				FM_DisplayA.SetEquations(EquationScript.BuildEquation()
					.AddExpression("A")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(fm_A) // Assuming fm_X is the original matrix A
					.GetEquation());

				// Calculate the determinant and create the display
				det = ~fm_A;

				FM_DisplayDet = CreateEquation();
				FM_DisplayDet.SetEquations(EquationScript.BuildEquation()
					.AddExpression("|A|")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddExpression(det.ToString())
					.GetEquation());

				// Check if the determinant is non-zero
				if (det != 0) {
					// Create the identity matrix to augment with the original matrix
					var identityMatrix = new FractionMatrix();
					int n = fm_A.columns.Count;

					// Create identity matrix
					for (int i = 0; i < n; i++) {
						var row = new List<Fraction>();
						for (int j = 0; j < n; j++) {
							row.Add(i == j ? 1 : 0);
						}
						identityMatrix.columns.Add(new Column<Fraction>(row));
					}

					// Create the augmented matrix display
					var FM_DisplayAugmented = CreateEquation();
					FM_DisplayAugmented.SetEquations(EquationScript.BuildEquation()
						.AddAugmented(fm_A, identityMatrix) // Use AddAugmented to show [A | I]
						.GetEquation());

					// Iterate through each row to display operations
					for (int i = 0; i < fm_A.columns.Count; i++) {
						// Display row normalization
						var FM_DisplayNormalization = CreateEquation();
						FM_DisplayNormalization.SetEquations(EquationScript.BuildEquation()
							.AddExpression($"Normalize row {i + 1} by dividing by {fm_A.getValue(i, i)}")
							.GetEquation());

						// Display elimination operations
						for (int o = 0; o < fm_A.columns.Count; o++) {
							if (o != i) {
								var factor = fm_A.getValue(o, i);
								var FM_DisplayElimination = CreateEquation();
								FM_DisplayElimination.SetEquations(EquationScript.BuildEquation()
									.AddExpression($"Eliminate element in row {o + 1}, column {i + 1} by subtracting {factor} * row {i + 1}")
									.GetEquation());
							}
						}

						// Optionally, you can display the current state of the matrix after each operation
						var currentMatrixDisplay = CreateEquation();
						currentMatrixDisplay.SetEquations(EquationScript.BuildEquation()
							.AddExpression($"Current state after row operations:")
							.AddMatrix(fm_A)
							.GetEquation());
					}

					// Display the final inverse matrix
					var FM_DisplaySol = CreateEquation();
					FM_DisplaySol.SetEquations(EquationScript.BuildEquation()
						.AddExpression("A<sup>-1</sup>")
						.AddOperator(EquationBlock.Operators.Equals)
						.AddFraction("1", det.ToString())
						.AddMatrix(fm_A.Inverse())
						.GetEquation());

					var inverse = fm_A.Inverse();

					var IMM_Display = CreateEquation();

					IMM_Display.SetEquations(EquationScript.BuildEquation()
						.AddExpression("X")
						.AddOperator(EquationBlock.Operators.Equals)
						.AddExpression("B")
						.AddOperator(EquationBlock.Operators.Multiply)
						.AddExpression("A<sup>-1</sup>")
						.AddOperator(EquationBlock.Operators.Equals)
						.AddMatrix(fm_B)
						.AddOperator(EquationBlock.Operators.Multiply)
						.AddMatrix(inverse)
						.AddOperator(EquationBlock.Operators.Equals)
						.AddMatrix(fm_B * inverse)
						.GetEquation());
				} else {
					// If no solution, display the no result message
					var FM_DisplayNoSol = CreateEquation();
					FM_DisplayNoSol.SetEquations(EquationScript.BuildEquation()
						.AddExpression("A<sup>-1</sup>")
						.AddOperator(EquationBlock.Operators.Equals)
						.AddExpression("No result")
						.GetEquation());
				}
				break;

			case Operations.MatrixEquation:
				var augment = fm_A.Augment(fm_B) as FractionMatrix;

				var solvingSteps = UtilManager.convertToStrMatrix(augment).GaussianEliminateWithSteps();

				augment.GaussianEliminate();

				var subSteps = UtilManager.convertToStrMatrix(augment).BackSubstituteWithSteps();

				foreach (var step in solvingSteps) {
					var stepDisplay = CreateEquation();

					stepDisplay.SetEquations(step.GetEquation());
				}

				foreach (var step in subSteps) {
					var stepDisplay = CreateEquation();

					stepDisplay.SetEquations(step.GetEquation());
				}

				var ME_DisplaySol = CreateEquation();

				ME_DisplaySol.SetEquations(EquationScript.BuildEquation()
					.AddExpression("X")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(new FractionMatrix(new List<List<Fraction>> { augment.BackSubstitute() }))
					.GetEquation());
				break;

			case Operations.MainElementMethod:
				augment = fm_A.Augment(fm_B) as FractionMatrix;

				var table = UtilManager.convertToStrMatrix(augment).PrincipalElementMethod();

				var X = augment.PrincipalElementMethod();

				var MEM_DisplayTable = CreateEquation();

				MEM_DisplayTable.SetEquations(EquationScript.BuildEquation()
					.AddTable(table)
					.GetEquation());

				var MEM_DisplaySol = CreateEquation();

				MEM_DisplaySol.SetEquations(EquationScript.BuildEquation()
					.AddExpression("X")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(X)
					.GetEquation());
				break;

			case Operations.JacobiMethod:
				augment = fm_A.Augment(fm_B) as FractionMatrix;

				table = UtilManager.convertToStrMatrix(augment).SolveJacobi();

				X = augment.SolveJacobi();

				var JM_DisplayIG = CreateEquation();

				JM_DisplayIG.SetEquations(EquationScript.BuildEquation()
					.AddExpression("X<sub>guess</sub>")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(new FractionMatrix(1, fm_A.columns.Count, 0))
					.GetEquation());

				var JM_DisplayTable = CreateEquation();

				JM_DisplayTable.SetEquations(EquationScript.BuildEquation()
					.AddTable(table)
					.GetEquation());

				var JM_DisplaySol = CreateEquation();

				JM_DisplaySol.SetEquations(EquationScript.BuildEquation()
					.AddExpression("X")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(X)
					.GetEquation());
				break;

			case Operations.RelaxationMethod:
				augment = fm_A.Augment(fm_B) as FractionMatrix;

				table = UtilManager.convertToStrMatrix(augment).SolveSOR();

				X = augment.SolveSOR();

				var RM_DisplayIG = CreateEquation();

				RM_DisplayIG.SetEquations(EquationScript.BuildEquation()
					.AddExpression("X<sub>guess</sub>")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(new FractionMatrix(1, fm_A.columns.Count, 0))
					.GetEquation());

				var RM_DisplayTable = CreateEquation();

				RM_DisplayTable.SetEquations(EquationScript.BuildEquation()
					.AddTable(table)
					.GetEquation());

				var RM_DisplaySol = CreateEquation();

				RM_DisplaySol.SetEquations(EquationScript.BuildEquation()
					.AddExpression("X")
					.AddOperator(EquationBlock.Operators.Equals)
					.AddMatrix(X)
					.GetEquation());
				break;
		}
	}

	public EquationScript CreateEquation() {
		var eq = Instantiate(EquationDisplay, EquationDisplayList);

		eq.name = "Line_" + (EquationDisplayList.childCount - 1);

		return eq.GetComponent<EquationScript>();
	}

	public void UpdateDropdownValues() {
		OperationDropdown.options.Clear();

		foreach (var str in OperationNames) {
			OperationDropdown.options.Add(new TMP_Dropdown.OptionData(str));
		}
	}

	public void DisableAllButtons() {
		foreach (RectTransform rect in InputList) {
			if (rect.name.Contains("Input"))
				rect.gameObject.SetActive(false);
		}
	}

	public void UpdateOperation() {
		UpdateOperation(OperationDropdown.value);
	}

	public void UpdateOperation(int value) {
		DisableAllButtons();

		switch ((Operations)value) {
			case Operations.Add:
				ButtonA.gameObject.SetActive(true);
				ButtonB.gameObject.SetActive(true);
				break;

			case Operations.Subtract:
				ButtonA.gameObject.SetActive(true);
				ButtonB.gameObject.SetActive(true);
				break;

			case Operations.MultiplyByMatrix:
				ButtonA.gameObject.SetActive(true);
				ButtonB.gameObject.SetActive(true);
				break;

			case Operations.MultiplyByNumber:
				ButtonX.gameObject.SetActive(true);
				NumberInput.transform.parent.gameObject.SetActive(true);
				break;

			case Operations.GetDeterminant:
				ButtonX.gameObject.SetActive(true);
				break;

			case Operations.GetRank:
				ButtonX.gameObject.SetActive(true);
				break;

			case Operations.InverseMatrix:
				ButtonX.gameObject.SetActive(true);
				break;

			case Operations.RelaxationMethod:
			case Operations.JacobiMethod:
			case Operations.InverseMatrixEquation:
			case Operations.MainElementMethod:
			case Operations.MatrixEquation:
				ButtonA.gameObject.SetActive(true);
				ButtonB.gameObject.SetActive(true);
				break;
		}
	}

	public void OpenInputMatrix(int matrix) {
		var m = (Matrices)matrix;

		var inputMatrix = Instantiate(MatrixInputDisplay, MatrixInputList);

		inputMatrix.name = "InputMatrix" + m.ToString();

		var inputScript = inputMatrix.GetComponent<MatrixInputScript>();

		switch (m) {
			case Matrices.A:
				inputScript.Setup(fm_A);

				inputScript.CloseAction = delegate (FractionMatrix fm) {
					fm_A = fm;
				};
				break;

			case Matrices.B:
				inputScript.Setup(fm_B);

				inputScript.CloseAction = delegate (FractionMatrix fm) {
					fm_B = fm;
				};
				break;

			case Matrices.X:
				inputScript.Setup(fm_X);

				inputScript.CloseAction = delegate (FractionMatrix fm) {
					fm_X = fm;
				};
				break;
		}
	}
}
