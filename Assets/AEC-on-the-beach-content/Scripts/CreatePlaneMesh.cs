using System.Collections;
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

    public float separation;  // Percent of Parcel Size that separates parcels.
    public float scale = 0.02f;   //  This is the real life conversion from grid size to Unity space.  Taken from scale factor on parent object.
    public int gridPointsPerParcel;   // Considered the Parcel Size.  Use even numbers.  
    public float bldgAspectRatio;   //  Ratio of the Height to the Parcel Size.
    public float elevationTolerance;   // High gain value for filtering bad elevation data.
    public float maxElevationDelta;     //  Value for the max expected real Unity-scale difference between high and low;

    public Building[] Buildings;

    private void Awake() {
        DV.ConnectToTcpServer();
        MeshData meshData = GenerateTerrainMesh();
        mesh = meshData.CreateMesh();
        GetComponent<MeshFilter>().mesh = mesh;
        //CCR.CreateHousesInBegin(mesh.vertices, mesh.normals, xSize, zSize);

        //  Procedural City Variables
        gridPointsPerParcel = 10; 
        separation = 0.2f;  
        scale = 0.02f;  
        bldgAspectRatio = 3f;
        elevationTolerance = 1f;
        maxElevationDelta = 0.1f;  // Figure 10 centimeters;

        UpdateMeshes(DV.GetHeightMap());
        PlaceCity();
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
        // CCR.Checkhouses(mesh.vertices, mesh.normals, xSize, zSize);

        yield return null;
    }

    private void UpdateMeshes(float[] heightMap)
    {
        int width = xSize;
        int height = zSize;

        int vertexIndex = 0;
        Color[] colorMap = new Color[xSize * zSize];

        var positions = mesh.vertices;

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                positions[vertexIndex].y = heightMap[vertexIndex];
                colorMap[vertexIndex] = terranColorsGradient.Evaluate(heightMap[vertexIndex] / 700f); //Take color from gradient
                vertexIndex++;
            }
        }

        mesh.vertices = positions;
        mesh.colors = colorMap;
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

    // Use this for initialization
    public void PlaceCity()
    {
        int gridX = Mathf.FloorToInt(xSize / (gridPointsPerParcel + gridPointsPerParcel * separation));
        int gridZ = Mathf.FloorToInt(zSize / (gridPointsPerParcel + gridPointsPerParcel * separation));

        Buildings = new Building[gridX * gridZ];

        float realOffset = scale * (gridPointsPerParcel) * (1 + separation);    

        for (int z = 0; z < gridZ; z++)
        {
            for (int x = 0; x < gridX; x++)
            {
                Vector3 offset = new Vector3(x * realOffset + gridPointsPerParcel * scale / 2, 0f, z * realOffset + gridPointsPerParcel * scale / 2);
                Buildings[z * gridX + x] = PlaceBuildingSingle(offset, Mathf.RoundToInt(x * realOffset + gridPointsPerParcel / 2 + z * realOffset + gridPointsPerParcel / 2));
            }
        }

        /// <summary>
        /// Places a single self-building object
        /// </summary>
         Building PlaceBuildingSingle(Vector3 loc, int MeshIndex)
         {
            Vector3 center = transform.position + loc;
            string bldgName = "Building" + MeshIndex;

            GameObject go = new GameObject();

            go.AddComponent<MeshRenderer>();
            go.AddComponent<MeshFilter>();

            Building bldg = go.AddComponent<Building>();
            bldg.size = gridPointsPerParcel * scale;
            bldg.height = gridPointsPerParcel * scale * bldgAspectRatio;

            /*
            
            Vector3 lowHighAvg = ElevationData(MeshIndex);
            Debug.Log(lowHighAvg);

            if (lowHighAvg.x >= 0)
            {
                bldg.Build();
                center.y = lowHighAvg.z;
                float diff = lowHighAvg.y - lowHighAvg.x;
                diff = diff / maxElevationDelta;    // Normalize. 
                bldg.size = (1 - diff) * bldg.size;
            }
            */

            go.transform.position = center;
            go.transform.parent = transform;

            bldg.Build();
            return bldg;
            
         }
    }

    /// <summary>
    /// Returns a vector 3 with the low, high, and average of the corners points centered on the Mesh Grid reference.
    /// </summary>
    public Vector3 ElevationData(int GridReference)
    {
        float[] elevations = new float[gridPointsPerParcel * gridPointsPerParcel];
        for (int i = 0; i < gridPointsPerParcel; i++)
        {
            for(int j = 0; j < gridPointsPerParcel; j++)
            {
                //  Exception handling for weird elevation data.
                int convertedIndex = GridReference - gridPointsPerParcel * xSize * i / 2 - gridPointsPerParcel / 2 + j;
                Debug.Log(convertedIndex);

                float rawElevation = mesh.vertices[GridReference - gridPointsPerParcel * xSize * (gridPointsPerParcel - i) / 2 - gridPointsPerParcel / 2 + j].y;

                if (rawElevation < -0.02f || rawElevation > elevationTolerance) rawElevation = 0;

                elevations[i * gridPointsPerParcel + j] = rawElevation;
            }
        }
        float sum = 0;
        int err = 0;
        float high = 0;
        float low = 0;

        for (var i = 0; i < elevations.Length; i++)
        {
            if (elevations[i] == 0) err++;
            else
            {
                sum += elevations[i];
                if (elevations[i] < low) low = elevations[i];
                if (elevations[i] > high) high = elevations[i];
            }

        }
        Vector3 result = new Vector3(low, high, sum / elevations.Length - err);
        return result;
    }
}
