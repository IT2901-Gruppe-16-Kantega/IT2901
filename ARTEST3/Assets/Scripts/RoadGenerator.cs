using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoadGenerator : MonoBehaviour {
	
    public Material matRoad;

    [SerializeField]
    private APIWrapper apiWrapper;

    void Start() {
        apiWrapper = GetComponent<APIWrapper>();
    }

    public void FetchRoad() {
        apiWrapper.FetchObjects(532, GPSManager.myLocation, objects => {
            CreateRoadMesh(objects);
        });
    }

    public void CreateRoadMesh(List<Objekt> roads) {
        foreach(Objekt road in roads) {

            float height = -1.0000f;
            GameObject roadObject = new GameObject("Road");
            roadObject.transform.parent = gameObject.transform;

            Vector3 nextPosition = Vector3.zero;

            List<Vector3> vertices = new List<Vector3>();
            for(int i = 0; i < road.parsedLocation.Count; i++) {
                var coords = road.parsedLocation[i];

                Vector3 location = HelperFunctions.GetPositionFromCoords(coords);

                float roadWidth = 15f;

                Quaternion rotation;
                if(i + 1 < road.parsedLocation.Count) {
                    rotation = Quaternion.FromToRotation(Vector3.forward, HelperFunctions.GetPositionFromCoords(coords, road.parsedLocation[i + 1])); 
                }
                else {
                    rotation = Quaternion.FromToRotation(Vector3.back, HelperFunctions.GetPositionFromCoords(coords, road.parsedLocation[i - 1])); 
                }
                float deltaX = (float)(-System.Math.Cos((rotation.eulerAngles.y - 180) * (System.Math.PI / 180)) * roadWidth / 2);
                float deltaZ = (float)(System.Math.Sin((rotation.eulerAngles.y - 180) * (System.Math.PI / 180)) * roadWidth / 2);

                vertices.Add(new Vector3(location.x + deltaX, height, location.z + deltaZ));
                vertices.Add(new Vector3(location.x - deltaX, height, location.z - deltaZ));
            }

            List<int> triangles = new List<int>();

            for(int j = 0; j < vertices.Count / 2 - 1; j++) {
                triangles.Add(2 * j);
                triangles.Add((2 * j) + 1);
                triangles.Add((2 * j) + 3);

                triangles.Add(2 * j);
                triangles.Add((2 * j) + 3);
                triangles.Add((2 * j) + 2);
            }
            roadObject.name = vertices.Count + " vertices, " + (triangles.Count / 3) + " triangles.";

            Mesh mesh = new Mesh();

            // Create a mesh filter
            MeshFilter meshFilter = roadObject.AddComponent<MeshFilter>();

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            Vector3[] normals = new Vector3[vertices.Count];
            for(int i = 0; i < normals.Length; i++) normals[i] = Vector3.up;
            mesh.normals = normals;

            meshFilter.mesh = mesh;

            // Create a mesh renderer for the mesh to show
            MeshRenderer meshRenderer = roadObject.AddComponent<MeshRenderer>();
            meshRenderer.material = matRoad;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            height -= 0.001f;
        }
    }
}
