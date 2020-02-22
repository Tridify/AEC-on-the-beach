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


    private float time = 0;
    private float timeToBuild = 1;

    private void Awake() {
        DV.ConnectToTcpServer();
        MeshData meshData = GenerateTerrainMesh();
        mesh = meshData.CreateMesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CCR.CreateHousesInBegin(mesh.vertices, mesh.normals, xSize, zSize);
    }

    private void Update()
    {
        UpdateMeshes(DV.GetHeightMap());
        if(time >= timeToBuild)
        {
            StartCoroutine("Build");
            time = 0;
        }
        time += Time.deltaTime;
    }


    IEnumerator Build()
    {
        CCR.Checkhouses(mesh.vertices, mesh.normals, xSize, zSize);
        yield return null;
    }

    private void UpdateMeshes(float[] heightMap)
    {
        int width = xSize;
        int height = zSize;

        int vertexIndex = 0;

        var positions = mesh.vertices;

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                positions[vertexIndex].y = heightMap[vertexIndex];
                vertexIndex++;
            }
        }

        mesh.vertices = positions;
    }

    public MeshData GenerateTerrainMesh()
    {
        int width = xSize;
        int height = zSize;

        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                meshData.vertices[vertexIndex] = new Vector3(x, 0, z);
                if (x < width - 1 && z < height - 1)
                {
                    meshData.AddTriangle(vertexIndex + width, vertexIndex + width + 1, vertexIndex);
                    meshData.AddTriangle(vertexIndex + 1, vertexIndex, vertexIndex + width + 1);
                }
                vertexIndex++;
            }
        }
        return meshData;
    }

    public class MeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Color[] colors;
        public Vector2 size;

        int triangleIndex;

        public MeshData(int meshWidth, int meshHeight)
        {
            size = new Vector2(meshWidth, meshHeight);
            vertices = new Vector3[meshWidth * meshHeight];
            triangles = new int[meshWidth * meshHeight * 6];
        }
        public void AddTriangle(int a, int b, int c)
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }

        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.name = "terrain";
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.colors = colors;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
