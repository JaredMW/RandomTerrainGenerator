// Jared White
// September 23, 2016

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Generates a non-uniform random ("weighted" random) assortment of objects
/// across the terrain over a selected area.
/// </summary>
public class NonUniformGenerator : MonoBehaviour {

    // Fields
    #region Fields & Constants
    public int numberOfObjects = 75;
    public List<GameObject> objects;
    public List<GameObject> landObjects;
    public float xPosition = 0;
    public float zPosition = 0;
    public float boxRotation = 0;
    public float maxObjectRotation = 0;
    public float spawnWidth = 100;
    public float spawnLength = 100;
    public Terrain terrain;

    private float baseLandHeight;
    private float yPosition;

    private const int RETRIES = 2000;
    #endregion

    // Use this for initialization
    void Start ()
    {
        // Ensure values are valid
        if (numberOfObjects < 0)
        {
            numberOfObjects = 0;
        }
	    if (spawnWidth < 1)
        {
            spawnWidth = 100;
        }
        if (spawnLength < 1)
        {
            spawnLength = 100;
        }
        
        // Set default y position if terrain does not exist
        if (terrain == null)
        {
            yPosition = 0;
        }
        else
        {
            // Set the base land height for land-based objects
            if (terrain.gameObject.GetComponent<TerrainGenerator>() != null)
            {
                baseLandHeight
                    = terrain.GetComponent<TerrainGenerator>().maxWaterHeight
                    + terrain.transform.position.y
                    + (terrain.transform.lossyScale.y
                        * terrain.terrainData.size.y
                        * terrain.GetComponent<TerrainGenerator>()
                        .waterHeightPercentage);
            }
            else
            {
                baseLandHeight = 0;
            }

            // Set the default y position if the X, Z coordinates are valid
            yPosition
                = terrain.SampleHeight(new Vector3(xPosition, 0, zPosition))
                + terrain.transform.position.y;
        }

        // Generate the random non-uniform objects
        GenerateNonUniformObjects();
	}
	
	// Update is called once per frame
	void Update ()
    { 
	    
	}


    /// <summary>
    /// Generate a "clump" of objects randomly within a defined box on top of
    /// the terrain, with the majority of the objects weighted towards the front
    /// center of the box.
    /// Land Objects will only be placed on land.
    /// </summary>
    void GenerateNonUniformObjects()
    {
        if (terrain != null && numberOfObjects > 0)
        {
            GameObject nonUniformObjects
                = new GameObject("Non-Uniform Random Objects");
            nonUniformObjects.transform.parent = terrain.transform;
            nonUniformObjects.transform.position
                = new Vector3(xPosition, yPosition, zPosition);
            nonUniformObjects.transform.rotation
                = Quaternion.Euler(0, boxRotation, 0);

            int objCount;
            int landCount;
            float xCoord = 0;
            float yCoord = yPosition;
            float zCoord = 0;
            float weightScale;


            // Non-uniformly populate the terrain with neutral objects
            #region Object Population
            if (objects.Count > 0)
            {
                // Determine a random portion of neutral to land-based objects
                // to generate
                if (landObjects.Count > 0)
                {
                    objCount = Random.Range(0, objects.Count);
                }
                else
                {
                    objCount = numberOfObjects;
                }


                for (int i = 0; i < objCount; i++)
                {
                    GameObject obj = objects[Random.Range(0, objects.Count)];

                    // Determine non-uniform random distribution
                    #region Non-Uniform Random Determination
                    weightScale = Random.Range(1, 11) / 10f;

                    // 10% chance to be placed in the 4th horizontal quadrant
                    if (weightScale == .1f)
                    {
                        zCoord = zPosition + terrain.transform.position.z
                            + Random.Range(spawnLength * .75f, spawnLength);
                    }

                    // 20% chance to be placed in the 3rd horizontal quadrant
                    else if (weightScale <= .3f)
                    {
                        zCoord = zPosition + terrain.transform.position.z
                            + Random.Range(spawnLength * .5f,
                            spawnLength * .75f);
                    }

                    // 30% chance to be placed in the 2nd horizontal quadrant
                    else if (weightScale <= .6f)
                    {
                        zCoord = zPosition + terrain.transform.position.z
                            + Random.Range(spawnLength * .25f,
                            spawnLength * .5f);
                    }

                    // 40% chance to be placed in the 1st horizontal quadrant
                    else
                    {
                        zCoord = zPosition + terrain.transform.position.z
                            + Random.Range(0, spawnLength * .25f);
                    }
                    #endregion


                    // Set the x position for the object
                    xCoord = xPosition + terrain.transform.position.x
                        + Random.Range(-spawnWidth / 2, spawnWidth / 2);

                    // Instantiate the object with its x and z coordinates.
                    obj = Instantiate(
                            obj,
                            new Vector3(xCoord, yCoord, zCoord),
                            Quaternion.Euler(
                                0,
                                Random.Range(-maxObjectRotation,
                                    maxObjectRotation) + 180,
                                0)) as GameObject;

                    // Set the object as a child of the base object
                    obj.transform.parent = nonUniformObjects.transform;

                    // Rotate the object about the box's rotation
                    obj.transform.RotateAround(
                        nonUniformObjects.transform.position,
                        Vector3.up,
                        boxRotation);

                    // Set the object's Y to on top of the terrain
                    xCoord = obj.transform.position.x;
                    zCoord = obj.transform.position.z;
                    yCoord = terrain.transform.position.y
                        + terrain.SampleHeight(new Vector3(xCoord, 0, zCoord));
                    
                    obj.transform.position = new Vector3(
                        xCoord,
                        yCoord,
                        zCoord);
                }
            }

            else
            {
                objCount = 0;
            }
            #endregion


            // Non-uniformly populte the terrain above max water bob height with
            // land-based objects
            #region Land Object Population
            if (landObjects.Count > 0)
            {
                float lowerZBound = 0;
                float upperZBound = spawnWidth;
                int attempts = 0;
                landCount = numberOfObjects - objCount;


                for (int i = 0; i < landCount; i++)
                {
                    GameObject landObj
                        = landObjects[Random.Range(0, landObjects.Count)];

                    // Determine non-uniform random distribution
                    #region Non-Uniform Random Determination
                    weightScale = Random.Range(1, 11) / 10f;

                    // 10% chance to be placed in the 4th horizontal quadrant
                    if (weightScale == .1f)
                    {
                        lowerZBound = spawnLength * .75f;
                        upperZBound = spawnLength;
                    }

                    // 20% chance to be placed in the 3rd horizontal quadrant
                    else if (weightScale <= .3f)
                    {
                        lowerZBound = spawnLength * .5f;
                        upperZBound = spawnLength * .75f;
                    }

                    // 30% chance to be placed in the 2nd horizontal quadrant
                    else if (weightScale <= .6f)
                    {
                        lowerZBound = spawnLength * .25f;
                        upperZBound = spawnLength * .5f;
                    }

                    // 40% chance to be placed in the 1st horizontal quadrant
                    else
                    {
                        lowerZBound = 0;
                        upperZBound = spawnLength * .25f;
                    }
                    #endregion


                    // Set the x and z potential coordinates for the object
                    xCoord = xPosition + terrain.transform.position.x
                        + Random.Range(-spawnWidth / 2, spawnWidth / 2);
                    zCoord = zPosition + terrain.transform.position.z
                        + Random.Range(lowerZBound, upperZBound);


                    // Assign the object a potential coordinate & rotate it
                    // to the (X, Z) it would be at when the box is rotated
                    landObj.transform.position
                        = new Vector3(xCoord, yPosition, zCoord);

                    landObj.transform.RotateAround(
                        nonUniformObjects.transform.position,
                        Vector3.up,
                        boxRotation);

                    yCoord = terrain.SampleHeight(landObj.transform.position)
                        + terrain.transform.position.y;


                    // If y location is not above water level, keep making
                    // another attempt to place it until so many tries.
                    while (yCoord < baseLandHeight && attempts < RETRIES)
                    {
                        xCoord = xPosition + terrain.transform.position.x
                            + Random.Range(-spawnWidth / 2, spawnWidth / 2);
                        zCoord = zPosition + terrain.transform.position.z
                            + Random.Range(lowerZBound, upperZBound);

                        landObj.transform.position
                            = new Vector3(xCoord, yCoord, zCoord);

                        landObj.transform.RotateAround(
                            nonUniformObjects.transform.position,
                            Vector3.up,
                            boxRotation);

                        yCoord
                            = terrain.SampleHeight(landObj.transform.position)
                            + terrain.transform.position.y;

                        attempts++;
                    }


                    // Reset the number of attempts to place the object, and
                    // place the object if it can be successfully placed.
                    attempts = 0;

                    if (yCoord > baseLandHeight)
                    {
                        xCoord = landObj.transform.position.x;
                        zCoord = landObj.transform.position.z;

                        landObj = Instantiate(
                            landObj,
                            new Vector3(xCoord, yCoord, zCoord),
                            Quaternion.Euler(
                                0,
                                180 + boxRotation + Random.Range(
                                    -maxObjectRotation, maxObjectRotation),
                                0)) as GameObject;

                        landObj.transform.parent = nonUniformObjects.transform;
                    }
                    else
                    {
                        Destroy(landObj);
                    }
                }
            }
            #endregion
        }
    }
}
