// Jared White
// September 20, 2016

using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(Terrain))]
[RequireComponent(typeof(TerrainCollider))]

/// <summary>
/// When applied to a Terrain object, a water plane object will be
/// created using the TerrainData. Modified version of a terrain generation
/// script.
/// </summary>
public class TerrainGenerator : MonoBehaviour
{
    // Fields & Constants
    #region Fields & Constants
    //public bool randomizeTerrain;
    public bool generateTerrain = false;
    public bool generateWater = false;
    public int resolution = 129;
    public float xCoord;
    public float yCoord;
    public float step = 0.025f;
    public float texturesPerUnit = .15f;
    public float waterHeightPercentage = 0.5f;
    public float maxWaterHeight = .75f; // Max height from top to center of bob
    public float minWaterSpeed = .001f;
    public float maxWaterSpeed = .023f;
    public float minWaterBob = .3f;
    public float maxWaterBob = 1;
    public float maxFogDistance = 1;
    public float maxFogStrength = 3.1f;
    public Material waterMaterial;
    public FirstPersonController fpsController;

    private TerrainData terrainData;
    private float[,] heights;
    #endregion


    // Properties
    #region Properties
    /// <summary>
    /// The Width of the Terrain object
    /// </summary>
    public float Width
    {
        get
        {
            return terrainData.size.x;
        }
    }

    /// <summary>
    /// The Length of the Terrain object
    /// </summary>
    public float Length
    {
        get
        {
            return terrainData.size.z;
        }
    }
    #endregion


    // Use this for initialization
    void Start()
    {
        // Cases
        #region Case Catching
        // Make sure resolution is valid. If not, default to 129.
        #region Resolution
        if (resolution < 2)
        {
            resolution = 129;
        }

        // Make sure resolution is a power of 2, plus 1.
        // If not, raise it to the next power of 2, plus 1.
        else
        {
            double res = resolution - 1;
            int count = 0;
            while ((res / 2) > 1)
            {
                res /= 2;
                count++;
            }
            count++;

            resolution = (int)Mathf.Pow(2, count) + 1;
        }
        #endregion

        // Ensure that the coordinates for Perlin terrain generation are valid
        #region Perlin Terrain Coordinates
        if (xCoord < 0)
        {
            xCoord = 0;
        }
        if (yCoord < 0)
        {
            yCoord = 0;
        }
        #endregion

        // Ensure textures per unit for water plane is valid
        #region Texture Tiling
        if (texturesPerUnit <= 0)
        {
            texturesPerUnit = 0.15f;
        }
        #endregion

        // Make sure water speeds & height are valid
        #region Water Speeds + Height
        if (minWaterSpeed <= 0)
        {
            minWaterSpeed = .001f;
        }
        if (maxWaterSpeed <= 0)
        {
            maxWaterSpeed = .023f;
        }
        if (maxWaterSpeed <= minWaterSpeed)
        {
            maxWaterSpeed = minWaterSpeed + .01f;
        }

        if (maxWaterHeight <= 0)
        {
            maxWaterHeight = .75f;
        }
        #endregion
        #endregion

        // PERLIN TERRAIN GENERATION
        #region Terrain Generation
        terrainData = gameObject.GetComponent<Terrain>().terrainData;

        // Ensure the water percentage height is valid.
        // If not, set it to 50%.
        if (waterHeightPercentage > 1 || waterHeightPercentage < 0)
        {
            waterHeightPercentage = 0.5f;
        }


        if (generateTerrain)
        {
            terrainData.size = new Vector3(Width, terrainData.size.y, Length);
            terrainData.heightmapResolution = resolution;

            heights = new float[resolution, resolution];

            float y = yCoord;

            // Acquire a set of randomly generated values for a heightmap
            for (int c = 0; c < resolution; c++)
            {
                for (int r = 0; r < resolution; r++)
                {
                    heights[c, r] = Mathf.PerlinNoise(xCoord, yCoord);
                    yCoord += step;
                }

                yCoord = y;
                xCoord += step;
            }

            // Set the heightmap of the terrain to the randomly generated values
            terrainData.SetHeights(0, 0, heights);
        }
        #endregion

        // Create a water plane object if enabled
        if (generateWater)
        {
            CreateWaterPlane();
        }
    }


    // Update is called once per frame
    void Update()
    {

    }


    /// <summary>
    /// Instantiate a water plane object based on the current properties of the
    /// terrain (width, length, and height)
    /// </summary>
    void CreateWaterPlane()
    {
        // Create the water plane object
        GameObject waterObject = new GameObject("WaterPlane");

        // Create the mesh of the water plane object
        waterObject.AddComponent<MeshRenderer>();
        MeshFilter filter = waterObject.AddComponent<MeshFilter>();
        filter.mesh = new Mesh();

        // Instantiating required variables
        #region Water plane instance instantiations
        int fractionalSize = 4;
        int relativeResolution;      // Relative resolution of water to terrain

        // Must be two or greater. If not, default relative resolution to two.
        if (resolution / fractionalSize < 2)
        {
            relativeResolution = 2;
        }

        // If greater than two, set relative resolution to a fraction of the
        // full resolution, and round up.
        else
        {
            relativeResolution
                = Mathf.CeilToInt((float)resolution / fractionalSize);
        }

        // Create texture and Perlin shift offset speed (the amount they shift)
        float xStep = Random.Range(minWaterSpeed, maxWaterSpeed);
        float zStep = Random.Range(minWaterSpeed, maxWaterSpeed);

        // Randomly assign x and z shifts to be positive or negative
        xStep *= (Random.Range(1, 3) * 2 - 3);
        zStep *= (Random.Range(1, 3) * 2 - 3);


        // Create a 2D array of vertex coordinates for the water plane object
        Vector3[,] vertexCoordinates
            = new Vector3[relativeResolution, relativeResolution];
        #endregion


        // Calculate, store, and assign the vertices for the water plane object
        // Initial y vertices are determined w/ random Perlin noise-based values
        #region Vertex Generation
        Vector3[] vertices
            = new Vector3[relativeResolution * relativeResolution];
        float xPerRes = Width / (relativeResolution - 1);
        float yPerRes = Length / (relativeResolution - 1);
        float yStep = Random.Range(.1f, 1f);
        float xPerlin = 0;
        float yPerlin = 0;
        int vertCount = 0;

        for (int r = 0; r < relativeResolution; r++)
        {
            for (int c = 0; c < relativeResolution; c++)
            {
                vertexCoordinates[r, c] = new Vector3(
                    (r * xPerRes) - (Width / 2),
                    Mathf.PerlinNoise(xPerlin, yPerlin)
                        * (maxWaterHeight * 2) - (maxWaterHeight / 2),
                    (c * yPerRes) - (Length / 2));

                vertices[vertCount] = vertexCoordinates[r, c];
                vertCount++;
                xPerlin += yStep;
            }

            xPerlin = 0;
            yPerlin += yStep;
        }
        filter.mesh.vertices = vertices;
        #endregion

        yStep = Random.Range(minWaterBob, maxWaterBob);


        // Set the triangles of the water plane object
        #region Triangle Generation
        int[] triangles
            = new int[(relativeResolution - 1) * (relativeResolution - 1) * 6];
        int triCount = 0;
        for (int r = 0; r < relativeResolution - 1; r++)
        {
            for (int c = 0; c < relativeResolution - 1; c++)
            {
                // Top side
                triangles[triCount] = (r * relativeResolution) + c;
                triangles[triCount + 1] = (r * relativeResolution) + c + 1;
                triangles[triCount + 2] = ((r + 1) * relativeResolution) + c
                    + 1;

                triangles[triCount + 3] = ((r + 1) * relativeResolution) + c
                    + 1;
                triangles[triCount + 4] = ((r + 1) * relativeResolution) + c;
                triangles[triCount + 5] = (r * relativeResolution) + c;

                // Bottom side
                //triangles[triCount + 6] = ((r + 1) * relativeResolution) + c
                //    + 1;
                //triangles[triCount + 7] = (r * relativeResolution) + c + 1;
                //triangles[triCount + 8] = (r * relativeResolution) + c;

                //triangles[triCount + 9] = (r * relativeResolution) + c;
                //triangles[triCount + 10] = ((r + 1) * relativeResolution) + c;
                //triangles[triCount + 11] = ((r + 1) * relativeResolution) + c
                //    + 1;

                triCount += 6;
            }
        }
        filter.mesh.triangles = triangles;
        #endregion

        filter.mesh.RecalculateNormals();


        // Set the UVs of the water plane object
        #region UV Generation
        Vector2[] uvs = new Vector2[vertCount];
        int uvCount = 0;
        for (int r = 0; r < vertexCoordinates.GetLength(0); r++)
        {
            for (int c = 0; c < vertexCoordinates.GetLength(1); c++)
            {
                uvs[uvCount] = new Vector2(
                    (float)r / vertexCoordinates.GetLength(0),
                    (float)c / vertexCoordinates.GetLength(1));
                uvCount++;
            }
        }
        filter.mesh.uv = uvs;
        #endregion


        // Assign the water material to the water plane object & scale it
        waterObject.GetComponent<Renderer>().material = waterMaterial;
        waterObject.GetComponent<Renderer>().material.mainTextureScale
            = new Vector2(Width * texturesPerUnit, Length * texturesPerUnit);

        // Place the water plane object in the center of the terrain at the
        // specified water height percentage of the maximum height.
        waterObject.transform.position = new Vector3(
            (Width / 2) * transform.lossyScale.x + transform.position.x,
            waterHeightPercentage * terrainData.size.y * transform.lossyScale.y
                + transform.position.y,
            (Length / 2) * transform.lossyScale.z
                + transform.position.z);

        waterObject.transform.parent = transform;

        // Add the Perlin noise-based water script to the water plane object
        #region WaterBehavior Script
        waterObject.AddComponent<WaterBehavior>();
        waterObject.GetComponent<WaterBehavior>().SetVertices(filter.mesh.vertices);
        waterObject.GetComponent<WaterBehavior>().PlaneSize
            = new Vector2(vertexCoordinates.GetLength(0), vertexCoordinates.GetLength(1));
        waterObject.GetComponent<WaterBehavior>().BobDistance
            = maxWaterHeight * 2;
        waterObject.GetComponent<WaterBehavior>().maxFogDistance = maxFogDistance;
        waterObject.GetComponent<WaterBehavior>().maxFogStrength = maxFogStrength;
        waterObject.GetComponent<WaterBehavior>().XSpeed = xStep;
        waterObject.GetComponent<WaterBehavior>().YSpeed = yStep;
        waterObject.GetComponent<WaterBehavior>().ZSpeed = zStep;
        waterObject.GetComponent<WaterBehavior>().fpsController = fpsController;
        #endregion
    }
}