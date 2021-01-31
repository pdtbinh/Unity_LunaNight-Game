using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageTransform : MonoBehaviour
{
    // Virtual camera
    private GameObject cmBrain;

    // Village generator
    private GameObject villageGenerator;
    private bool generateVillageCalled;

    // Moving speed of this village
    private float speed = -0.5f;

    // Start is called before the first frame update
    void Start()
    {
        cmBrain = GameObject.FindGameObjectWithTag("VirCamBrain");

        villageGenerator = GameObject.FindGameObjectWithTag("VillageGenerator");

        generateVillageCalled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!MainCtrlManager.GamePaused) { Move(1, speed * Time.deltaTime); }

        if (!generateVillageCalled && cmBrain.transform.position.x - transform.position.x > 17.5f)
        {
            villageGenerator.GetComponent<VillageGenerator>().GenerateVillage();
            generateVillageCalled = true;

            Destroy(gameObject, 0.5f);
        }
    }

    // Move this platform on x-axis or y-axis by "movingUnit"
    private void Move(int axisID, float movingUnit)
    /* axisID = 1 -> moving on x-axis
       axisID = 2 -> moving on y-axis */
    {
        if (axisID == 1)
        {
            float x = transform.position.x;
            x += movingUnit;
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }

        else if (axisID == 2)
        {
            float y = transform.position.y;
            y += movingUnit;
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }
    }
}
