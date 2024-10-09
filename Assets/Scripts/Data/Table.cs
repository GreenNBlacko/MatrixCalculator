using System.Collections.Generic;
using UnityEngine;

public class Table {
	private List<List<(string label, Color32 color)>> _data;

	public Table() {
		_data = new();
	}

	// Method to retrieve the current table
	public List<List<(string label, Color32 color)>> GetTable() {

		return _data;
	}

	// Method to add a new empty row
	public Table AddRow(int cols = 0) {
		_data.Add(new List<(string label, Color32 color)>(cols));
		return this;
	}

	// Method to add a new row with provided values
	public Table AddRow(List<(string label, Color32 color)> rowValues) {
		// Adding a new row with specific values
		_data.Add(rowValues);
		return this;
	}

	// Method to set a value at a specific row and column
	public Table SetValue(int rowIndex, int colIndex, (string label, Color32 color) value) {
		// Check if the row index is valid
		if (rowIndex >= _data.Count) {
			// If the row does not exist, add empty rows until we reach the desired index
			for (int i = _data.Count; i <= rowIndex; i++) {
				_data.Add(new List<(string label, Color32 color)>());
			}
		}

		// Ensure the specific row has enough columns
		while (_data[rowIndex].Count <= colIndex) {
			_data[rowIndex].Add((string.Empty, new Color32(0, 0, 0, 0))); // Add empty placeholders
		}

		// Set the value at the specified row and column
		_data[rowIndex][colIndex] = value;
		return this;
	}
}
