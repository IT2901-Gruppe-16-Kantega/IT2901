using System.IO;
using UnityEngine;

public static class LocalStorage {
	/*
	#if UNITY_STANDALONE
	const string url = "./data/data.json";
	#elif UNITY_STANDALONE_WIN
	private static string url = "./data/data.json";
	#elif UNITY_ANDROID
	private static string url = "/data/data/com.nvdb/files/data.json";
	#else
	private static string url = "/data/data/com.nvdb/files/data.json";
	#endif
	*/

	//const string url = "/data/data/com.nvdb/files/data.json";
	private static string dataPath = Application.persistentDataPath + "/";
	const string fileName = "data.json";

	/// <summary>
	/// Saves the data
	/// </summary>
	/// <param name="data"></param>
	/// <returns>true if the file saved successfully, else otherwise.</returns>
	public static bool SaveData(string data) {
		string n = Application.persistentDataPath;
		Debug.Log(n);
		File.WriteAllText(dataPath + fileName, data, System.Text.Encoding.UTF8);
		return File.Exists(dataPath + fileName);
	}

	/// <summary>
	/// Gets the data.
	/// </summary>
	/// <returns>The data.</returns>
	public static string GetData() {
		if (File.Exists(dataPath + fileName)) {
			return File.ReadAllText(dataPath + fileName);
		} else {
			return null;
		}
	}
}