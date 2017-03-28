// ReSharper disable RedundantUsingDirective
// ReSharper disable UnusedMember.Local
using System.Collections.Generic;
using System.Deployment.Internal;
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
		File.WriteAllText(DataPath + fileName, data, System.Text.Encoding.UTF8);
		return File.Exists(DataPath + fileName);
	}

	/// <summary>
	/// Gets the data.
	/// </summary>
	/// <returns>The data.</returns>
	public static string GetData(string fileName) {
		// TODO get from iOS
		#if UNITY_IOS
        //iCloudKV_SetInt("HEI", 122);
        Debug.Log("VALUE: " + defaultGetString("HEI"));

       // defaultSetString("HALLO", "hvordan går det?");
		#endif
		// TODO make it work on both iOS and Android
		return File.Exists(DataPath + fileName) ? File.ReadAllText(DataPath + fileName) : null;
	}

	public static bool CreateReport(string fileName, List<Objekter> signList) {
		NvdbObjekt nvdbObjekt = new NvdbObjekt {objekter = signList};
		// TODO if everything is wanted, using JsonUtility is easiest. If only a few things are wanted, solution below works
		//Debug.Log(JsonUtility.ToJson(nvdbObjekt));
		//Debug.Log(signList[signList.Count - 1]);
		//string data = "{ \"objekter\" : [ ";
		//for (int i = 0; i < signList.Count; i++) {
		//	data += "{ ";
		//	data += "\"id\" : " + signList[i].id + ", ";
		//	data += "\"href\" : " + signList[i].href + ", ";
		//	data += "\"distance\" : " + signList[i].distance + ", ";
		//	data += "\"bearing\" : " + signList[i].bearing;
		//	data += " }";
		//	if (i < signList.Count - 2) data += ", ";
		//}
		//data += "} ] }";
		return SaveData(fileName, JsonUtility.ToJson(nvdbObjekt, true));
	}
}