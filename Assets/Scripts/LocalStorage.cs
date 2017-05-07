// ReSharper disable RedundantUsingDirective
// ReSharper disable UnusedMember.Local

using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;

public static class LocalStorage {
	private static readonly string DataPath = Application.persistentDataPath + "/";

	[DllImport("__Internal")]
	private static extern void defaultSetString(string key, string value);

	[DllImport("__Internal")]
	private static extern string defaultGetString(string key);

	/// <summary>
	///     Saves the data
	/// </summary>
	/// <param name="fileName"></param>
	/// <param name="data"></param>
	/// <returns>true if the file saved successfully, else otherwise.</returns>
	public static bool SaveData(string fileName, string data) {
		Encoding utf8 = new UTF8Encoding(false);
		File.WriteAllText(DataPath + fileName, data, utf8);
		return File.Exists(DataPath + fileName);
	}

	/// <summary>
	///     Gets the data.
	/// </summary>
	/// <returns>The data.</returns>
	public static string GetData(string fileName) {
#if UNITY_IOS
		return defaultGetString("HEI");
#endif
		if (!File.Exists(DataPath + fileName))
			return null;

		string data = File.ReadAllText(DataPath + fileName);
		long key;
		if (long.TryParse(data, out key))
			return File.Exists(DataPath + "/searches/" + key + ".json")
				? File.ReadAllText(DataPath + "/searches/" + key + ".json")
				: null;
		return null;
	}

	/// <summary>
	///     Creates a report for RN
	/// </summary>
	/// <param name="fileName">The file to save to</param>
	/// <param name="signList">The list of signs</param>
	/// <returns>True if it succeeded, false if it failed</returns>
	public static bool CreateReport(string fileName, List<Objekter> signList) {
		Report report = new Report {
			reportObjects = new List<ReportObject>()
		};
		foreach (Objekter sign in signList) {
			ReportObject reportObject = new ReportObject {
				vegobjekt = sign.id,
				endringer = new List<ReportEgenskap>()
			};
			foreach (Egenskaper egenskap in sign.egenskaper) {
				if (egenskap.id != 4795)
					continue;
				ReportEgenskap reportEgenskap = new ReportEgenskap();
				ReportEgenskap2 reportEgenskap2 = new ReportEgenskap2 {
					id = egenskap.id,
					navn = egenskap.navn,
					datatype = egenskap.datatype,
					datatype_tekst = egenskap.datatype_tekst,
					verdi = egenskap.verdi
				};
				reportEgenskap.egenskap = reportEgenskap2;
				reportEgenskap.dato = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
				reportEgenskap.type = "EGENSKAP_FEIL";
				reportEgenskap.beskrivelse = sign.metadata.distance + ". Grader mot klokken: " + sign.metadata.bearing +
											". Andre notater: " + sign.metadata.notat;
				reportObject.endringer.Add(reportEgenskap);
			}
			report.reportObjects.Add(reportObject);
		}
		string data = JsonUtility.ToJson(report);
		return SaveData(fileName, data);
	}
}