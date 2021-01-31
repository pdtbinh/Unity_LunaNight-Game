using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCtrl : MonoBehaviour
{
    // Moving left-ward components:
    private float speed;

    // Generate platforms
    private bool platformGeneratedCalled;

    // Diving and Lifting components:
    private bool isDiving;

    private int numberOfDiveCalled;

    private bool isLifting;

    // Glowing material
    public Material OnAirGlow;

    public Material SpriteDefault;
   
    // Start is called before the first frame update
    void Start()
    {
        // Moving left-ward components:
        speed = -3f;

        // Diving and Lifting components:
        isDiving = false;

        numberOfDiveCalled = 0;

        isLifting = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!MainCtrlManager.GamePaused)
        {
            // Constantly move left-ward
            Move(1, speed * Time.deltaTime);

            // Enable if player jumps on this platform
            if (isDiving) { Dive(-1f, 3.2f); }

            // Enable automatically after diving
            if (isLifting) { Lift(0.2f); }

            // Constantly checks whether this platform is still needed
            Lasting();
        } 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Collision -> Dive (bool) -> StopDiving -> Lift (bool) -> StopLifting
        if (collision.collider.CompareTag("Player") && numberOfDiveCalled == 0)
        {
            // Glow
            Glow();

            // Enable diving
            isDiving = true;

            // Call generate platform
            if (PlayerPrefs.GetString("InstructionShowed", "n") == "y" && !platformGeneratedCalled)
            {
                GameObject generator = GameObject.FindGameObjectWithTag("PlatformGenerator");
                generator.GetComponent<PlatformGenerator>().PassiveGenerate();

                platformGeneratedCalled = true;
            }

            // Stop diving automatically after a while
            Invoke("StopDiving", 0.5f);
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

    // Invoke when character jumps on
    private void Dive(float initialVelocity, float acceleration)
    {
        // Decrease y-coordinate value
        Move(2, (initialVelocity + acceleration * Time.deltaTime * numberOfDiveCalled) * Time.deltaTime);
        numberOfDiveCalled += 1;
    }

    // Stop the dive and invoke Lift()
    private void StopDiving()
    {
        // Stop diving
        isDiving = false;

        // Enable lifting and stop lifting
        isLifting = true;
        Invoke("StopLifting", 0.5f);
    }

    // Lift this platform up
    private void Lift(float velocity)
    {
        // Increase y-coordinate value
        Move(2, velocity * Time.deltaTime);
    }

    // Stop lifting
    private void StopLifting() { isLifting = false; }

    // Adjust glow
    private void Glow()
    {
        // Add glow material to this platform
        transform.GetChild(0).GetComponent<SpriteRenderer>().material = OnAirGlow;
    }

    private void Lasting()
    {
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerTransform != null)
        {
            float distanceToPlayer_x = transform.position.x - playerTransform.position.x;

            if (distanceToPlayer_x < -10f) { DeleteThisGameObject(); }
        }
    }

    // To be called a while after player jumps on (when this platform is no longer needed)
    private void DeleteThisGameObject() { Destroy(gameObject); }
}
