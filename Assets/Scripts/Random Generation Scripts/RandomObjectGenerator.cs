// Jared White
// September 23, 2016

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Generate random objects across the terrain. Land Objects will be exclusively
/// placed on land, above the water bob height.
/// </summary>
public class RandomObjectGenerator : MonoBehaviour {

    // Fields & Constants
    #region Fields & Constants
    public int numberOfObjects;
    public float distanceFromEdge = .2f;
    public List<GameObject> objects;
    public List<GameObject> landObjects;
    public Terrain terrain;
    
    private float baseLandHeight;
    private const int Retries = 2000;
    #endregion

    // Use this for initialization
    void Start ()
    {
        // Ensure values are valid
	    if (numberOfObjects < 0)
        {
            numberOfObjects = 0;
        }
        if (distanceFromEdge > 1 || distanceFromEdge < 0)
        {
            distanceFromEdge = .2f;
        }
        
        if (terrain != null
            && terrain.gameObject.GetComponent<TerrainGenerator>() != null)
        {
            baseLandHeight
                = terrain.GetComponent<TerrainGenerator>().maxWaterHeight
                + terrain.transform.position.y
                + (terrain.transform.lossyScale.y * terrain.terrainData.size.y
                    * terrain.GetComponent<TerrainGenerator>()
                    .waterHeightPercentage);
        }
        else
        {
            baseLandHeight = 0;
        }

        GenerateRandomObjects();
	}
	
	// Update is called once per frame
	void Update ()
    {
	    
	}

    /// <summary>
    /// Randomly generate objects across the terrain, taking into account
    /// whether they may be placed underwater
    /// </summary>
    void GenerateRandomObjects()
    {
        if (terrain != null && numberOfObjects > 0)
        {
            GameObject randomObjects = new GameObject("Random Objects");
            randomObjects.transform.parent = terrain.transform;

            int objCount;
            int landCount;
            float xCoord;
            float yCoord;
            float zCoord;
            Vector3 location;

            // Randomly populate the terrain with neutral objects
            #region Object Population
            if (objects.Count > 0)
            {
                objCount = Random.Range(0, numberOfObjects);

                for (int i = 0; i < objCount; i++)
                {
                    // Generate random object location
                    xCoord = Random.Range(distanceFromEdge,
                        terrain.terrainData.size.x - distanceFromEdge)
                        + terrain.transform.position.x;
                    zCoord = Random.Range(distanceFromEdge,
                        terrain.terrainData.size.z - distanceFromEdge)
                        + terrain.transform.position.z;
                    yCoord
                        = terrain.SampleHeight(new Vector3(xCoord, 0, zCoord))
                        + terrain.transform.position.y;

                    location = new Vector3(xCoord, yCoord, zCoord);

                    // Instantiate an instance of the object
                    (Instantiate(
                        objects[Random.Range(0, objects.Count)],
                        location,
                        Quaternion.Euler(0, Random.Range(0f, 359f), 0))
                        as GameObject).transform.parent
                            = randomObjects.transform;
                }
            }
            else
            {
                objCount = 0;
            }
            #endregion

            // Randomly populate the terrain above water level with land objects
            #region Land Object Population
            if (landObjects.Count > 0)
            {
                landCount = numberOfObjects - objCount;
                int attempts = 0;

                for (int i = 0; i < landCount; i++)
                {
                    // Generate initial random object location
                    xCoord = Random.Range(distanceFromEdge,
                        terrain.terrainData.size.x - distanceFromEdge)
                        + terrain.transform.position.x;
                    zCoord = Random.Range(distanceFromEdge,
                        terrain.terrainData.size.z - distanceFromEdge)
                        + terrain.transform.position.z;
                    yCoord
                        = terrain.SampleHeight(new Vector3(xCoord, 0, zCoord))
                        + terrain.transform.position.y;

                    // Keep trying to find new locations if the sampled location
                    // is below the water. If can't find new spot after so many
                    // attempts, give up.
                    while (yCoord < baseLandHeight && attempts < Retries)
                    {
                        xCoord = Random.Range(distanceFromEdge,
                            terrain.terrainData.size.x - distanceFromEdge)
                            + terrain.transform.position.x;
                        zCoord = Random.Range(distanceFromEdge,
                            terrain.terrainData.size.z - distanceFromEdge)
                            + terrain.transform.position.z;
                        yCoord = terrain.SampleHeight(
                            new Vector3(xCoord, 0, zCoord))
                            + terrain.transform.position.y;

                        attempts++;
                    }


                    // Reset the attempt count. If successful, place the object.
                    attempts = 0;
                    
                    if (yCoord > baseLandHeight)
                    {
                        location = new Vector3(xCoord, yCoord, zCoord);

                        // Instantiate an instance of the object
                        (Instantiate(
                            landObjects[Random.Range(0, landObjects.Count)],
                            location,
                            Quaternion.Euler(0, Random.Range(0f, 359f), 0))
                            as GameObject).transform.parent
                                = randomObjects.transform;
                    }
                }
            }
            #endregion
        }
    }
}
