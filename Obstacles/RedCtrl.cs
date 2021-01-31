using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class RedCtrl : MonoBehaviour
{
    private int redShapeID;

    private BoxCollider2D redBoxCollider;

    // Particle effect
    public ParticleSystem[] particleExps;

    // Virtual camera
    private GameObject virtualCam;

    // Game manager
    private GameObject GM;
    private PlrPrefsCtrl GM_prefsCtrl;
    private bool resourceAddedToPlayer;
    private bool playerDefeated;

    // Audio source
    private AudioSource expSound;

    // Start is called before the first frame update
    void Start()
    {
        if      (gameObject.CompareTag("RedTriangle"))    { redShapeID = 0; }
        else if (gameObject.CompareTag("RedSquare")  )    { redShapeID = 1; }
        else if (gameObject.CompareTag("RedPolygon") )    { redShapeID = 2; }
        else if (gameObject.CompareTag("YellowTriangle")) { redShapeID = 3; }
        else if (gameObject.CompareTag("YellowSquare"))   { redShapeID = 4; }
        else if (gameObject.CompareTag("YellowPolygon"))  { redShapeID = 5; }

        redBoxCollider = GetComponent<BoxCollider2D>();

        virtualCam = GameObject.FindGameObjectWithTag("VirCamBrain");

        GM = GameObject.FindGameObjectWithTag("GMScript");
        GM_prefsCtrl = GM.GetComponent<PlrPrefsCtrl>();
        resourceAddedToPlayer = false;
        playerDefeated = false;

        /*Set seed differently each time to make it seems random.
          Random is used to calculate points player can earn from this obstacle*/
        Random.InitState((int)System.DateTime.Now.Ticks / 1000);

        expSound = GameObject.FindGameObjectWithTag("AudiosCombination")
            .transform.GetChild(1)
            .GetComponent<AudioSource>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // If player hits with the wrong shape, stop the game
                if (PlayerCtrl.currentShapeID != (redShapeID % 3) && !playerDefeated)
                {
                    player.GetComponent<PlayerCtrl>().Defeated();
                    playerDefeated = true;
                }

                // Else, continue, and create effect
                else
                {
                    // LOGICAL EFFECTS
                    if (!resourceAddedToPlayer)
                    {
                        if (redShapeID < 3) // If this is red obstacle, add point 
                        {
                            GM_prefsCtrl.ChangePointsGained((int)Random.Range(1, 6));
                        }
                        else if (redShapeID >= 3) // If this is yellow obstacle, add point and gold
                        {
                            GM_prefsCtrl.ChangePointsGained((int)Random.Range(1, 6));
                            GM_prefsCtrl.ChangeMoneyGained((int)Random.Range(1, 3));
                        }
                        resourceAddedToPlayer = true;
                    }
                    
                    // VISUAL EFFECTS
                    // Disappear the visible image and collider of this obstacle
                    for (int childIndex = 0; childIndex < transform.childCount; childIndex++)
                    {
                        if (childIndex < transform.childCount-1)
                        {
                            transform.GetChild(childIndex).gameObject.SetActive(false);
                        }

                        // Spawn particle effect
                        else
                        {
                            transform.GetChild(childIndex).gameObject.SetActive(true);
                        }
                    }
                    redBoxCollider.enabled = false;

                    // Shake camera
                    if (virtualCam != null)
                    {
                        CamShake shakeScript = virtualCam.GetComponent<CamShake>();
                        if (shakeScript != null)
                        {
                            // Change this according to the current time scale
                            shakeScript.ShakeCamera(2.5f, 7, 0.5f);
                        }
                    }

                    // SOUND EFFECTS
                    expSound.Play();

                    Destroy(gameObject, 1f);
                }
            }
        }
    }

    // Called when fusion exp of player is awake
    public void BlownByFusionExp()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Only destroy obstacle within a specific range
        if (player != null  && Vector3.Distance(player.transform.position, transform.position) <= 15f)
        {
            // Disappear the visible image and collider of this obstacle
            for (int childIndex = 0; childIndex < transform.childCount; childIndex++)
            {
                if (childIndex < transform.childCount - 1)
                {
                    transform.GetChild(childIndex).gameObject.SetActive(false);
                }

                // Spawn particle effect
                else
                {
                    transform.GetChild(childIndex).gameObject.SetActive(true);
                }
            }
            redBoxCollider.enabled = false;

            Destroy(gameObject, 1f);
        }
    }
}
