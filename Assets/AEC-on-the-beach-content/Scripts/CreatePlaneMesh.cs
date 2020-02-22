using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreatePlaneMesh : MonoBehaviour
{
	private Mesh mesh;

	private int xSize = 640;

    private int zSize = 480;

    public AnimationCurve meshHeightCurve;

	public Gradient terranColorsGradient;

    private Vector3[] vertices;
    private Color[] colorMap;

    public CreateCubesRandomly CCR;
    public DepthView DV;

    private void Awake() {
        DV.ConnectToTcpServer();
		Generate();
    }

    private void LateUpdate()
    {
        UpdateMeshes(DV.GetHeightMap());
    }

    private void UpdateMeshes(int[] heightMap)
    {
        /*if (heightMap[22] != 0) {
            Debug.Log(heightMap[22]);
            Debug.Log("max "+heightMap.Max());
            Debug.Log("min " + heightMap.Min());
        }*/
        //float[,] noiseMap = Noise.GenerateNoiseMap(xSize + 1, zSize + 1, 0, 10, 2, 1, 1, new Vector2(0,0));
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        colorMap = new Color[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++, i++)
            {
                //Vector3 currPosition = vertices[i];
                float currentHeight = heightMap[i];//noiseMap[x, z];
                vertices[i] = new Vector3(x, currentHeight, z);
                colorMap[i] = terranColorsGradient.Evaluate(currentHeight); //Take color from gradient
            }
        }
        mesh.vertices = vertices;
        mesh.colors = colorMap;
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
	}

	private void Generate() {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		mesh.name = "Terrain";

		vertices = new Vector3[(xSize + 1) * (zSize + 1)];
		Vector2[] uv = new Vector2[vertices.Length];
		Vector4[] tangents = new Vector4[vertices.Length];
		Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
		for (int i = 0, z = 0; z <= zSize; z++) {
			for (int x = 0; x <= xSize; x++, i++) {
				vertices[i] = new Vector3(x, 0, z);
				uv[i] = new Vector2((float)x / xSize, (float)z / zSize);
				tangents[i] = tangent;
			}
		}
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.tangents = tangents;

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
