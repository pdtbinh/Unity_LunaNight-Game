using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldCtrl : MonoBehaviour
{
    // Moving left-ward components:
    private float speed;
    public int currentStage;

    // Animation components:
    private Animator animator;

    // Generate platforms
    private bool platformGeneratedCalled;

    // x coordinates of obstacle spawn positions
    private List<float> xCoordsOfObstacles = new List<float>() {3.3f};

    // Obstacles:_______________________________________________________________

    public GameObject obs_1;
    public GameObject obs_2;

    private List<GameObject> obstacles;

    //__________________________________________________________________________


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        // Moving left-ward components:
        speed = -3f;

        // Store all obstacle of this theme into 1 list to randomize index of the created obstacle
        obstacles = new List<GameObject>() { obs_1, obs_2 };

        platformGeneratedCalled = false;

        // Set seed differently each time to make it seems random
        Random.InitState((int)System.DateTime.Now.Ticks / 1500);

        if (!gameObject.CompareTag("InstructionComponents"))
        {
            // Loop through all the positions and instantiate obstacles
            for (int index = 0; index < xCoordsOfObstacles.Count; index++)
            {
                // A random number to check whether this position should contain obstacle
                int chance = Random.Range(0, 101);

                // The probability that this position contains obstacle (increases along with the stage-difficulty level)
                int obstacleProb = Mathf.Min(90 + (currentStage / 2), 100);

                if (chance <= obstacleProb) // "obstacleProb" % probability that this position contains obstacle
                {
                    // x coordinate of the position (relative to the scene, not this field) of the obstacle to be created
                    float xCoord = xCoordsOfObstacles[index];

                    // Random index of obstacle to be created
                    chance = Random.Range(0, obstacles.Count);

                    // Create obstacle based on current theme
                    Instantiate(obstacles[chance], new Vector3(xCoord + transform.position.x, -1f, 0f), Quaternion.identity, transform);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!MainCtrlManager.GamePaused)
        {
            // Constantly move left-ward
            Move(1, speed * Time.deltaTime);

            // Check player's position to activate aniamtion
            if (!animator.GetBool("Activated") && MainCtrlManager.GameStarted) { InitiateAnimation(); }

            // Constantly checks whether this platform is still needed
            Lasting();
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

    // Used to trigger monsters and obstacles actions
    private void OnCollisionEnter2D(Collision2D collision)
    {
        /* If this field collides with the player (player jumps on this platform),
           initiate monsters and obstacles on this field */ 
        if (collision.collider.CompareTag("Player"))
        {
            // Call generate platform
            if (PlayerPrefs.GetString("InstructionShowed", "n") == "y" && !platformGeneratedCalled)
            {
                FastenTheGame();

                GameObject generator = GameObject.FindGameObjectWithTag("PlatformGenerator");
                generator.GetComponent<PlatformGenerator>().PassiveGenerate();

                platformGeneratedCalled = true;
            }
        }
    }

    // Checks if this platform is still needed for the game
    private void Lasting()
    {
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerTransform != null)
        {
            float distanceToPlayer_x = transform.position.x - playerTransform.position.x;

            if (distanceToPlayer_x < -20f) { DeleteThisGameObject(); }
        }
    }

    // Fasten the game when user reaches this field
    private void FastenTheGame()
    {
        float newTimeScale = Mathf.Min(1.1f + (0.015f * currentStage), 2f);
        Time.timeScale = newTimeScale;
        MainCtrlManager.currentGameSpeed = newTimeScale;
    }

    // To be called a while after player jumps on (when this platform is no longer needed)
    private void DeleteThisGameObject() { Destroy(gameObject); }

    // Use this to initiate animation of this field
    private void InitiateAnimation()
    {
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (playerTransform != null)
        {
            float distanceToPlayer_x = transform.position.x - playerTransform.position.x;

            if (Mathf.Abs(distanceToPlayer_x) <= 3f) { animator.SetBool("Activated", true); }
        }
    }
}
