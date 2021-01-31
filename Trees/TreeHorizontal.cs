using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeHorizontal : MonoBehaviour
{
    // Obstacles:_______________________________________________________________
    public GameObject[] obstaclesList;

    // Positions to spawn obstacles, stored as children of this tree
    private Transform obsPos1;
    private Transform obsPos2;
    private Transform obsPos3;
    
    private List<Transform> coordsOfObstacles;

    //__________________________________________________________________________


    // Start is called before the first frame update
    void Start()
    {
        obsPos1 = transform.GetChild(1);
        obsPos2 = transform.GetChild(2);

        if (gameObject.CompareTag("HorizontalTree"))
        {
            obsPos3 = transform.GetChild(3);
            coordsOfObstacles = new List<Transform>() { obsPos1, obsPos2, obsPos3 };
        }

        else if (gameObject.CompareTag("DiagonalTree"))
        {
            coordsOfObstacles = new List<Transform>() { obsPos1, obsPos2 };
        }

        int lastObsIndex = 10;

        // Loop through all the positions and instantiate obstacles
        for (int index = 0; index < coordsOfObstacles.Count; index++)
        {
            // A random number to check whether this position should contain obstacle
            int chance = Random.Range(0, 101);

            // The probability that this position contains obstacle (increases along with the stage-difficulty level)
            int obstacleProb = Mathf.Min(70 + (PlatformGenerator.stage / 2), 100);

            if (chance <= obstacleProb) // "obstacleProb" % probability that this position contains obstacle
            {
                // x coordinate of the position (relative to the scene, not this field) of the obstacle to be created
                Vector3 coord = coordsOfObstacles[index].position;

                // Random index of obstacle to be created
                chance = Random.Range(0, obstaclesList.Length);

                if (chance == lastObsIndex || chance+3==lastObsIndex || chance-3==lastObsIndex)
                { chance = (chance + 1) % obstaclesList.Length; }

                lastObsIndex = chance;

                // Create obstacle based on current theme
                Instantiate(obstaclesList[chance], coord, Quaternion.identity, transform);
            }
        }
    }
}
