using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;

public static class LocalStorage {
	private static readonly string DataPath = Application.persistentDataPath + "/";

    [DllImport ("__Internal")]
    private static extern void defaultSetString(string key, string value);

    [DllImport ("__Internal")]
    private static extern string defaultGetString(string key);

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
		#if UNITY_IOS
        //iCloudKV_SetInt("HEI", 122);
        Debug.Log("VALUE: " + defaultGetString("HEI"));

       // defaultSetString("HALLO", "hvordan går det?");
		#endif
		// TODO make it work on both iOS and Android
		return File.Exists(DataPath + fileName) ? File.ReadAllText(DataPath + fileName) : null;
	}


}