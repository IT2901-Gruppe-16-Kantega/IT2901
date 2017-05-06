using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class GenerateRoads : MonoBehaviour {
	[HideInInspector]
	public static bool IsCreatingRoads = true;

	[SerializeField]
	private ApiWrapper _apiWrapper;

	private bool _useLocalData;

	public Material RoadMaterial;

	public GameObject RoadsParent;

	private void Start() {
		_apiWrapper = GetComponent<ApiWrapper>();
		_useLocalData = PlayerPrefs.GetInt("UseLocalData", 1) == 1;
	}

	/// <summary>
	///     Gets nearby roads and instantiates them
	/// </summary>
	public void FetchRoad() {
		string localData = LocalStorage.GetData("roads.json");
		_useLocalData = true; // TODO in react native -> send roads as well.
		if (_useLocalData) {
			_apiWrapper.FetchObjects(532, GpsManager.MyLocation, roads => {
				UiScripts.RoadsToInstantiate = roads.Count;
				StartCoroutine(CreateRoadMesh(roads));
			});
		} else {
			if (string.IsNullOrEmpty(localData)) {
				// TODO Do something if data loaded is not there. Query the user maybe?
			}
			// Make a new RootObject and parse the json data from the request
			NvdbObjekt data = JsonUtility.FromJson<NvdbObjekt>(localData);
			UiScripts.RoadsToInstantiate = data.objekter.Count;

			// Go through each Objekter in the data.objekter (the road objects)
			List<Objekter> roadList = data.objekter.Select(obj => _apiWrapper.ParseObject(obj))
				.ToList();
			StartCoroutine(CreateRoadMesh(roadList));
		}
	}

	/// <summary>
	///     Creates a roadmesh based on the list of roads from nvdb
	/// </summary>
	/// <param name="roads">The list of roads to create</param>
	public IEnumerator CreateRoadMesh(List<Objekter> roads) {
		IsCreatingRoads = true;
		float height = 0.0000f;
		for (int index = 0; index < roads.Count; index++) {
			Objekter road = roads[index];
			GameObject roadObject = new GameObject("Road");
			roadObject.transform.parent = RoadsParent.transform;
			roadObject.layer = 10;

			List<Vector3> vertices = new List<Vector3>();
			for (int i = 0; i < road.parsedLocation.Count; i++) {
				GpsManager.GpsLocation coords = road.parsedLocation[i];

				Vector3 location = HelperFunctions.GetPositionFromCoords(coords);

				const float roadWidth = 15f;

				Quaternion rotation = i + 1 < road.parsedLocation.Count
					? Quaternion.FromToRotation(Vector3.forward,
						HelperFunctions.GetPositionFromCoords(coords, road.parsedLocation[i + 1]))
					: Quaternion.FromToRotation(Vector3.back, HelperFunctions.GetPositionFromCoords(coords, road.parsedLocation[i - 1]));
				float deltaX = (float) (-Math.Cos((rotation.eulerAngles.y - 180) * (Math.PI / 180)) * roadWidth / 2);
				float deltaZ = (float) (Math.Sin((rotation.eulerAngles.y - 180) * (Math.PI / 180)) * roadWidth / 2);

				vertices.Add(new Vector3(location.x + deltaX, height, location.z + deltaZ));
				vertices.Add(new Vector3(location.x - deltaX, height, location.z - deltaZ));
			}
			roadObject.transform.position = vertices[0] - vertices[vertices.Count - 1];
			List<int> triangles = new List<int>();

			for (int i = 0; i < vertices.Count; i++)
				vertices[i] -= roadObject.transform.position;

			for (int j = 0; j < vertices.Count / 2 - 1; j++) {
				triangles.Add(2 * j);
				triangles.Add(2 * j + 1);
				triangles.Add(2 * j + 3);

				triangles.Add(2 * j);
				triangles.Add(2 * j + 3);
				triangles.Add(2 * j + 2);
			}
			roadObject.name = road.parsedLocation[0] + " - " + road.parsedLocation[road.parsedLocation.Count - 1];

			Mesh mesh = new Mesh {
				name = roadObject.name
			};

			// Create a mesh filter
			MeshFilter meshFilter = roadObject.AddComponent<MeshFilter>();

			mesh.vertices = vertices.ToArray();
			mesh.triangles = triangles.ToArray();
			Vector3[] normals = new Vector3[vertices.Count];
			for (int i = 0; i < normals.Length; i++)
				normals[i] = Vector3.up;
			mesh.normals = normals;
			Vector2[] uvs = new Vector2[vertices.Count];
			for (int i = 0; i < vertices.Count; i += 2) {
				uvs[i] = new Vector2((float) i / vertices.Count, 0);
				uvs[i + 1] = new Vector2((float) i / vertices.Count, 1);
			}
			mesh.uv = uvs;

			meshFilter.mesh = mesh;

			// Create a mesh renderer for the mesh to show
			MeshRenderer meshRenderer = roadObject.AddComponent<MeshRenderer>();

			meshRenderer.material = RoadMaterial;
			meshRenderer.material.mainTextureScale = new Vector2(vertices.Count / 2f, 1);
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			height -= 0.001f;
			UiScripts.RoadsInstantiated++;
			if (index % 10 == 0)
				yield return new WaitForEndOfFrame(); // 10 road references per frame
		}
		IsCreatingRoads = false;
	}
}