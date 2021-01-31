using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformGenerator : MonoBehaviour
{
    // ON-AIR PLATFORM
    private GameObject platformPrefab;  // Generate this

    public GameObject latestPlatform;   // Use to gain relative position of the new platform about to be created
    Vector3 latestPlatformPosition;     // Position 

    private float farDistance_min = 3f; // x variation range_min
    private float farDistance_max = 4f; // _                _max

    // Reset number of platforms to 0 everytime this section starts
    private int nofPlatforms;
    private int maxPlatforms = 5;       // Maximum number of on-air platforms created in each section

    // Use to avoid mistake in float approximation
    private float epsilon = 0.2f;


    // FIELD PLATFORM
    private GameObject fieldPrefab;

    // Reset the number of fields to 0 everytime this section starts
    private int nofFields;
    private int maxFields = 10;


    // Prefab storage:__________________________________________________________

    // -> On-airs:
    public GameObject onAir_1;
    public GameObject onAir_2;
    public GameObject onAir_3;
    public GameObject onAir_4;
    public GameObject onAir_5;

    public List<GameObject> onAirs;

    // -> Fields:
    public GameObject field_1;
    public GameObject field_2;
    public GameObject field_3;
    public GameObject field_4;
    public GameObject field_5;

    public List<GameObject> fields;

    //__________________________________________________________________________

    // Current stage and theme (the higher the harder)
    public static int stage;

    // Start is called before the first frame update
    void Start()
    {
        // Difficulty level and theme id start from 0 and 1, respectively
        stage = 0;

        // Store all platforms in two lists for randomization
        onAirs = new List<GameObject>() { onAir_1, onAir_2, onAir_3, onAir_4, onAir_5 };
        fields = new List<GameObject>() { field_1, field_2, field_3, field_4, field_5 };

        nofFields = 0;
        nofPlatforms = 0;

        // Set seed differently each time to make it seems random
        Random.InitState((int)System.DateTime.Now.Ticks / 1000);
    }

    public void PassiveGenerate()
    {
        if (nofFields < maxFields)
        {
            GenerateField();
        }

        else
        {
            GeneratePlatform();
            if (nofPlatforms == maxPlatforms)
            {
                nofFields = 0;
                nofPlatforms = 0;
            }
        }
    }

    // Generate on-air platforms
    private void GeneratePlatform()
    {
        if (nofPlatforms < maxPlatforms)
        {
            // Everytime this method starts, set relative position to the position of latest-created platform
            latestPlatformPosition = latestPlatform.transform.position;

            // Generate new y-coordinate (y=0,1,2,3)
            float newY;
            if (-0.5f >= latestPlatformPosition.y)
            {
                newY = 0f;
            }

            else if (0f - epsilon <= latestPlatformPosition.y && latestPlatformPosition.y <= 0f + epsilon)
            {
                newY = 1f;
            }

            else if (1f - epsilon <= latestPlatformPosition.y && latestPlatformPosition.y <= 1f + epsilon)
            {
                float chance = Random.Range(0f, 1f);
                if (chance <= 0.3f) newY = 0f;
                else newY = 2f;
            }

            else if (2f - epsilon <= latestPlatformPosition.y && latestPlatformPosition.y <= 2f + epsilon)
            {
                float chance = Random.Range(0f, 1f);
                if (chance <= 0.5f) newY = 0f;
                else if (chance <= 0.66f) newY = 1f;
                else newY = 3f;
            }

            else 
            {
                float chance = Random.Range(0f, 1f);
                if (chance <= 0.8f) newY = 0f;
                else newY = 1f;
            }

            // Set new x-coordinate range according to newly found y-coordinate
            if (newY > latestPlatformPosition.y) { farDistance_min = 3.5f; farDistance_max = 3.7f; }
            else { farDistance_min = 2.5f; farDistance_max = 3.5f; }

            // Generate new x-coordinate
            float randomX = Random.Range(farDistance_min, farDistance_max);
            float newX = latestPlatformPosition.x + randomX;

            // If lastest platform is a field, put this new platform further
            if (latestPlatform.CompareTag("Field")) newX = latestPlatformPosition.x + 5.8f + 3.2f;
            
            // Create new platform at the "newPosition"
            Vector3 newPosition = new Vector3(newX, newY, 0f);

            chooseRandomOnAirPlatform();
            GameObject newPlatform = Instantiate(platformPrefab, newPosition, Quaternion.identity, transform);

            // Set the relative position to the newly created platform
            latestPlatform = newPlatform;

            // Add 1 to the number of platforms created in this one section
            nofPlatforms += 1;
        }
    }

    // Generate field platform
    private void GenerateField()
    {
        if (nofFields < maxFields)
        {
            // Everytime this method starts, set relative position to the position of latest-created platform
            latestPlatformPosition = latestPlatform.transform.position;

            // Generate new x-coordinate
            float newX = latestPlatformPosition.x + (1.45f * 5); 
            if (latestPlatform.CompareTag("Platform")) newX = latestPlatformPosition.x + 3.2f;

            // Create new field at the "newPosition"
            Vector3 newPosition = new Vector3(newX, -1f, 0);

            chooseRandomFieldPlatform();
            GameObject newPlatform = Instantiate(fieldPrefab, newPosition, Quaternion.identity, transform);

            // Set the stage (difficulty) of the newly created platform
            stage += 1;
            newPlatform.GetComponent<FieldCtrl>().currentStage = stage;

            // Set order layer setting of this newly created platform
            int orderLayer;
            if (latestPlatform.CompareTag("Platform")) { orderLayer = 100; }
            else { orderLayer = latestPlatform.transform.GetChild(4).GetComponent<SpriteRenderer>().sortingOrder + 1; }

            for (int index = 0; index < 5; index++)
            {
                newPlatform.transform.GetChild(index).GetComponent<SpriteRenderer>().sortingOrder = orderLayer + index;
            }

            // Set the relative position to the newly created platform
            latestPlatform = newPlatform;

            // Add 1 to the number of fields created in this one section
            nofFields += 1;
        }
    }

    // Choose a random on-air platform to be created
    private void chooseRandomOnAirPlatform()
    {
        int randomIndex = Random.Range(0, onAirs.Count);
        platformPrefab = onAirs[randomIndex];
    }

    // Choose a random field to be created
    private void chooseRandomFieldPlatform()
    {
        int randomIndex = Random.Range(0, fields.Count);
        fieldPrefab = fields[randomIndex];
    }
}
