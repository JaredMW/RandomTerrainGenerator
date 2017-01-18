// Jared White
// September 20, 2016

using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.ImageEffects;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Material))]

/// <summary>
/// Causes a plane to behave as if it were water, animating using Perlin noise.
/// Object being applied to should be a plane with a specified width and height
/// of vertices. FPS controller should have the ScreenOverlay script to function
/// with this script.
/// </summary>
public class WaterBehavior : MonoBehaviour
{
    // Fields
    #region Fields
    public FirstPersonController fpsController;
    public ScreenOverlay waterOverlay;
    public int planeWidth;
    public int planeLength;
    public float xSpeed;
    public float ySpeed;
    public float zSpeed;
    public float maxBobHeight = 1.5f;
    public float maxFogDistance = 3.25f;
    public float maxFogStrength = 3.25f;

    private MeshFilter meshFilter;
    private Material material;
    private Vector3[] vertices;
    private Vector2 textureOffset;
    private Camera waterCamera;
    private float xCoord;
    private float xOriginal;
    private float zCoord;
    private float zOriginal;
    private float minX;
    private float maxX;
    private float minZ;
    private float maxZ;
    private float fpsY;
    private int counter;
    #endregion

    // Properties
    #region Properties
    /// <summary>
    /// The set of vertices for this water plane
    /// </summary>
    public Vector3[] Vertices
    {
        get { return vertices; }
    }

    /// <summary>
    /// The width of the water plane
    /// </summary>
    public int PlaneWidth
    {
        get { return planeWidth; }
    }

    /// <summary>
    /// The length of the water plane
    /// </summary>
    public int PlaneLength
    {
        get { return planeLength; }
    }

    /// <summary>
    /// Return the size of the plane, or resize it, as long as the new size
    /// is equivalent to the number of vertices within the plane.
    /// </summary>
    public Vector2 PlaneSize
    {
        get { return new Vector2(planeWidth, planeLength); }
        set
        {
            if (vertices != null && value.x * value.y == vertices.Length)
            {
                planeWidth = (int)value.x;
                planeLength = (int)value.y;
            }
        }
    }

    /// <summary>
    /// The maximum displacement from the heighest to lowest "bob" of the water
    /// </summary>
    public float BobDistance
    {
        get { return maxBobHeight; }
        set { maxBobHeight = value; }
    }

    /// <summary>
    /// The speed in the x-direction (step at which Perlin x shifts)
    /// </summary>
    public float XSpeed
    {
        get { return xSpeed; }
        set
        {
            xSpeed = value;
            textureOffset.x = value / 4;
        }
    }

    /// <summary>
    /// The speed in the y-direction (step at which Perlin y shifts)
    /// </summary>
    public float YSpeed
    {
        get { return ySpeed; }
        set { ySpeed = value; }
    }

    /// <summary>
    /// The speed in the z-direction (step at which Perlin z shifts)
    /// </summary>
    public float ZSpeed
    {
        get { return zSpeed; }
        set
        {
            zSpeed = value;
            textureOffset.y = value / 4;
        }
    }

    /// <summary>
    /// The Camera that the underwater imaging effects should be applied to.
    /// </summary>
    public Camera WaterCamera
    {
        get { return waterCamera; }
        set
        {
            waterCamera = value;

            if (waterCamera.gameObject.GetComponent<ScreenOverlay>() == null)
            {
                waterCamera.gameObject.AddComponent<ScreenOverlay>();
                Debug.Log("Oh no");
                waterOverlay = waterCamera.gameObject
                    .GetComponent<ScreenOverlay>();
                waterOverlay.intensity = 0;
            }
            else
            {
                waterOverlay = waterCamera.gameObject
                    .GetComponent<ScreenOverlay>();
            }

            fpsY = /*fpsController.transform.localScale.y
                **/ waterCamera.transform.localPosition.y
                /** fpsController.GetComponent<CharacterController>().height*/;
        }
    }
    #endregion


    // Use this for initialization
    void Start()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
            meshFilter.gameObject.GetComponent<MeshRenderer>().shadowCastingMode
                = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        if (vertices == null)
        {
            vertices = meshFilter.mesh.vertices;
        }

        // If perlin shifts are not specified, specify them.
        if (xSpeed == 0)
        {
            xSpeed = Random.Range(.001f, .023f);
            xSpeed *= (Random.Range(1, 3) * 2 - 3);
        }
        if (ySpeed == 0)
        {
            ySpeed = Random.Range(.1f, 1f);
        }
        if (zSpeed == 0)
        {
            zSpeed = Random.Range(.001f, .023f);
            zSpeed *= (Random.Range(1, 3) * 2 - 3);
        }

        // Calculate the relative y position of the camera
        if (fpsController != null)
        {
            WaterCamera = fpsController.GetComponentInChildren<Camera>();
        }

        // Reference to the water plane's Material
        material = GetComponent<Renderer>().material;

        // Ensure the planeWidth(x) and planeLength(z) are valid.
        RecalculateSize();
        RecalculateMinMaxVerts();
    }

    // Update is called once per frame
    void Update()
    {
        // Update the original and shifted coordinates for the perlin selection
        xOriginal += xSpeed;
        xCoord = xOriginal;
        zOriginal += zSpeed;
        zCoord = zOriginal;

        // Shift the y direction of every vertex within the perlin selection
        counter = 0;
        for (int r = 0; r < planeWidth; r++)
        {
            for (int c = 0; c < planeLength; c++)
            {
                vertices[counter] = new Vector3(
                    vertices[counter].x,
                    Mathf.PerlinNoise(xCoord, zCoord) * maxBobHeight
                        - maxBobHeight / 2,
                    vertices[counter].z);

                xCoord += ySpeed;
                counter++;
            }

            xCoord = xOriginal;
            zCoord += ySpeed;
        }


        // Update the mesh vertices with the new vertices
        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.RecalculateNormals();

        // Offset the water texture
        material.mainTextureOffset += textureOffset;

        // Apply the underwater fog overlay to the FPS camera based on depth
        // from the surface of the water, with a set max fog intensity.
        #region Water Fog
        if (fpsController != null && waterCamera.enabled == true)
        {
            if (fpsController.transform.position.x >= minX
                && fpsController.transform.position.x <= maxX
                && waterCamera.transform.position.y
                    <= transform.position.y
                && fpsController.transform.position.z >= minZ
                && fpsController.transform.position.z <= maxZ)
            {
                if (waterCamera.transform.position.y
                    > transform.position.y - maxFogDistance)
                {
                    waterOverlay.intensity = maxFogStrength * (1f
                        - (maxFogDistance + waterCamera.transform.position.y
                            - transform.position.y) / maxFogDistance);
                }

                else
                {
                    waterOverlay.intensity = maxFogStrength;
                }
            }
        

            // If not underwater, don't enable any overlay intensity
            else if(
                (fpsController.transform.position.y + fpsY
                    > transform.position.y
                || fpsController.transform.position.x < minX
                || fpsController.transform.position.x > maxX
                || fpsController.transform.position.z < minZ
                || fpsController.transform.position.z > maxZ)
                && waterOverlay.intensity != 0)
            {
                waterOverlay.intensity = 0;
            }
        }
        #endregion
    }


    /// <summary>
    /// Force set the MeshFilter and Vertices of the water plane
    /// </summary>
    public void SetVertices(Vector3[] vertices)
    {
        this.vertices = vertices;
        RecalculateSize();
        RecalculateMinMaxVerts();
    }

    /// <summary>
    /// Recalculate the positions of the minimum and maximum X and Z vertex
    /// components in world space
    /// </summary>
    void RecalculateMinMaxVerts()
    {
        if (vertices.Length > 0)
        {
            minX = vertices[0].x;
            maxX = vertices[0].x;
            minZ = vertices[0].z;
            maxZ = vertices[0].z;

            if (vertices.Length > 1)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    if (vertices[i].x < minX)
                    {
                        minX = vertices[i].x;
                    }
                    else if (vertices[i].x > maxX)
                    {
                        maxX = vertices[i].x;
                    }

                    if (vertices[i].z < minZ)
                    {
                        minZ = vertices[i].z;
                    }
                    else if (vertices[i].z > maxZ)
                    {
                        maxZ = vertices[i].z;
                    }
                }
            }

            minX *= transform.lossyScale.x;
            maxX *= transform.lossyScale.x;
            minZ *= transform.lossyScale.z;
            maxZ *= transform.lossyScale.z;
        }

        else
        {
            minX = 0;
            maxX = 0;
            minZ = 0;
            maxZ = 0;
        }

        minX += transform.position.x;
        maxX += transform.position.x;
        minZ += transform.position.z;
        maxZ += transform.position.z;
    }

    /// <summary>
    /// Ensure the planeWidth(x) and planeLength(z) are valid by resetting them.
    /// Not guaranteed to be accurate; only an estimation.
    /// </summary>
    void RecalculateSize()
    {
        if (vertices != null && planeWidth * planeLength != vertices.Length)
        {
            // 2 or more vertices
            if (vertices.Length > 1)
            {
                planeWidth = Mathf.CeilToInt(Mathf.Sqrt(vertices.Length));
                planeLength = Mathf.CeilToInt(Mathf.Sqrt(vertices.Length));

                while (planeWidth * planeLength > vertices.Length)
                {
                    // Reduce plane width until it is either valid or 1.
                    if (planeWidth > 1)
                    {
                        planeWidth--;
                    }

                    // If plane width is 2 and still not valid, reduce length.
                    else
                    {
                        planeLength--;
                    }
                }
            }

            // 1 or 0 vertices
            else
            {
                if (vertices.Length == 1)
                {
                    planeWidth = 1;
                    planeLength = 0;
                }
                else
                {
                    planeWidth = 0;
                    planeLength = 0;
                }
            }
        }
    }
}