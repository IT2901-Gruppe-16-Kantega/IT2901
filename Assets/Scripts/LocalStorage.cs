using System.IO;
using UnityEngine;

public static class LocalStorage {
	private static readonly string DataPath = Application.persistentDataPath + "/";

	/// <summary>
	/// Saves the data
	/// </summary>
	/// <param name="fileName"></param>
	/// <param name="data"></param>
	/// <returns>true if the file saved successfully, else otherwise.</returns>
	public static bool SaveData(string fileName, string data) {
		string n = Application.persistentDataPath;
		Debug.Log(n);
		File.WriteAllText(DataPath + fileName, data, System.Text.Encoding.UTF8);
		return File.Exists(DataPath + fileName);
	}

	/// <summary>
	/// Gets the data.
	/// </summary>
	/// <returns>The data.</returns>
	public static string GetData(string fileName) {
		return File.Exists(DataPath + fileName) ? File.ReadAllText(DataPath + fileName) : null;
	}
}