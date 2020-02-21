using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePlaneMesh : MonoBehaviour
{
	private Mesh mesh;

	[SerializeField]
	private int xSize = 10;
	[SerializeField]
	private int zSize = 10;

	private Vector3[] vertices;

	private void Awake() {
		Generate();
        UpdateMeshes();

    }

    private void UpdateMeshes()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(xSize + 1, zSize + 1, 0, 10, 2, 1, 1, new Vector2(0,0));
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                float currentHeight = noiseMap[x, z];
                vertices[i] = new Vector3(x, currentHeight, z);
            }
        }
        mesh.vertices = vertices;
    }

	private void Generate() {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Terrain";

		vertices = new Vector3[(xSize + 1) * (zSize + 1)];
		for (int i = 0, z = 0; z <= zSize; z++) {
			for (int x = 0; x <= xSize; x++, i++) {
				vertices[i] = new Vector3(x, 0, z);
			}
		}
		mesh.vertices = vertices;

		int[] triangles = new int[xSize * zSize * 6];
		for (int ti = 0, vi = 0, z = 0; z < zSize; z++, vi++) {
			for (int x = 0; x < xSize; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
				triangles[ti + 5] = vi + xSize + 2;
			}
		}
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
	}
}
