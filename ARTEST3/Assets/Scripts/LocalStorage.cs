using System.Collections;
using System.IO;

public static class LocalStorage  {
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

	const string url = "/data/data/com.nvdb/files/data.json";

	/// <summary>
	/// Saves the data.
	/// </summary>
	/// <returns><c>true</c>, if data was saved, <c>false</c> otherwise.</returns>
	/// <param name="data">Data.</param>
	public static bool saveData(string data){
		if (File.Exists (url)) {
			File.WriteAllText (url, data);
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Gets the data.
	/// </summary>
	/// <returns>The data.</returns>
	public static string getData(){
		if (File.Exists (url)) {
			return File.ReadAllText (url);
		} else {
			return "false";
		}
	}
}
