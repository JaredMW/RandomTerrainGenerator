// Jared White
// September 23, 2016

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Generates a number of objects in a non-uniform line, each with a y scale
/// determined by a Gaussian random generator.
/// </summary>
public class GaussianGenerator : MonoBehaviour {

    // Fields
    #region Fields
    public int numberOfObjects = 10;
    public float distBetweenObjects = 3;
    public float maxLineDisplacement = 1;
    public List<GameObject> objects;
    public float positionX = 0;
    public float positionZ = 0;
    public float lineRotation = 0;
    public float objectRotation = 0;
    public float gaussianMean = 1;
    public float standardDeviation = .24f;
    public Terrain terrain;

    private float positionY;
    #endregion

    // Use this for initialization
    void Start ()
    {
        if (numberOfObjects < 0)
        {
            numberOfObjects = 0;
        }

        // Make sure positions are valid, and determine Y position
        if (terrain == null
            || positionX < terrain.transform.position.x
            || positionX > terrain.transform.position.x
                + terrain.transform.localScale.x * terrain.terrainData.size.x
            || positionZ < terrain.transform.position.z
            || positionZ > terrain.transform.position.z
                + terrain.transform.localScale.z * terrain.terrainData.size.z)
        {
            positionY = 0;
        }
        else if (terrain != null)
        {
            positionY
                = terrain.SampleHeight(new Vector3(positionX, 0, positionZ))
                + terrain.transform.position.y;
        }

        // Generate the Gaussian objects
        GenerateGaussianObjects();
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    /// <summary>
    /// Generate a specified number of objects with their heights altered by a
    /// random Gaussian distribution
    /// </summary>
    void GenerateGaussianObjects()
    {
        if (terrain != null && numberOfObjects > 0 && objects.Count > 0)
        {
            // Create the object to hold the line of objects
            GameObject gaussianLineObject = new GameObject("Gaussian Line");

            // Place the line appropriately
            gaussianLineObject.transform.position
                = new Vector3(positionX, positionY, positionZ);
            gaussianLineObject.transform.parent = terrain.transform;

            // Create the objects
            GameObject[] objs = new GameObject[numberOfObjects];

            // Appropriate the X and Z values for each object
            for (int count = 0; count < numberOfObjects; count++)
            {
                objs[count] = Instantiate(
                    objects[Random.Range(0, objects.Count)],
                    new Vector3(
                        gaussianLineObject.transform.position.x
                            + (distBetweenObjects * count),
                        positionY,
                        gaussianLineObject.transform.position.z
                            + (Random.Range(0f, maxLineDisplacement)
                            * (Random.Range(1, 3) * 2 - 3))),
                    Quaternion.Euler(0, objectRotation, 0))
                    as GameObject;

                // Make each object a child of the line object
                objs[count].transform.parent = gaussianLineObject.transform;
            }

            // Rotate the line and each of the objects
            gaussianLineObject.transform.Rotate(
                new Vector3(0, lineRotation, 0));

            // Appropriate the proper y value for each object's location,
            // and use Gaussian distribution to scale the y values, as well
            for (int count = 0; count < objs.Length; count++)
            {
                objs[count].transform.position = new Vector3(
                    objs[count].transform.position.x,
                    terrain.SampleHeight(objs[count].transform.position)
                        + terrain.transform.position.y,
                    objs[count].transform.position.z);

                objs[count].transform.localScale = new Vector3(
                    objs[count].transform.localScale.x,
                    GaussianValue(gaussianMean, standardDeviation),
                    objs[count].transform.localScale.x);
            }
        }
    }

    /// <summary>
    /// Return a random Gaussian value
    /// </summary>
    /// <param name="mean">Average value</param>
    /// <param name="stdDev">Standard deviation size</param>
    /// <returns>Randomized Gaussian value</returns>
    float GaussianValue(float mean, float stdDev)
    {
        float val1 = Random.Range(.001f, 1f);
        float val2 = Random.Range(0f, 1f);
        float gaussValue = Mathf.Sqrt(-2.0f * Mathf.Log(val1))
            * Mathf.Sin(2.0f * Mathf.PI * val2);
        return mean + stdDev * gaussValue;
    }
}
