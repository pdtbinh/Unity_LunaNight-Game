using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    // Jumping components:

    public LayerMask platformMask;

    public float playerHeight = 0.1f;

    public float jumpForce = 5.6f;

    private bool fallen;

    // This character properties

    BoxCollider2D playerCollider;

    Rigidbody2D rb;

    Animator animator;

    private bool jumping;

    // Interaction element
    public static int currentShapeID;
    public static bool revived;

    // All frequently-called children
    private GameObject triangleImg;
    private GameObject squareImg;
    private GameObject polygonImg;
    private GameObject faceImg;
    public  GameObject effectImg; // This is manually controlled
    private GameObject particleFusion;
    private GameObject particleExp;
    private GameObject particleDefeated;
    
    // Dissolve components
    public Material[] dissolves;

    private float fadeCoefficient;

    private bool fusionInitiated;

    public Transform[] dissolvedParts;

    // Glow components
    public Material[] glows; // First instance should be SpriteDefault for the center

    // Camera
    public GameObject vcFollower;

    // Swipe components_________________________________________________________
    private bool instructionShowed;
    private Vector3 startSwipePosition;
    private Vector3 endSwipePosition;
    private float minSwipeDistance = 18f; // in pixel, I hope-_-
    //__________________________________________________________________________


    // Start is called before the first frame update
    void Start()
    {
        // Self-explanatory
        playerCollider = GetComponent<BoxCollider2D>();

        rb = GetComponent<Rigidbody2D>();

        animator = GetComponent<Animator>();

        fallen = false;

        // Used for interaction with obstacles
        currentShapeID = 2;

        revived = false;

        // All the frequently-called children of this character
        int nofChild = transform.childCount;
        for (int childIndex = 0; childIndex < nofChild; childIndex++)
        {
            if (childIndex == 0)                 { triangleImg      = transform.GetChild(childIndex).gameObject; }
            else if (childIndex == 1)            { squareImg        = transform.GetChild(childIndex).gameObject; }
            else if (childIndex == 2)            { polygonImg       = transform.GetChild(childIndex).gameObject; }
            else if (childIndex == 3)            { faceImg          = transform.GetChild(childIndex).gameObject; }
            else if (childIndex == nofChild - 3) { particleFusion   = transform.GetChild(childIndex).gameObject; }
            else if (childIndex == nofChild - 2) { particleExp      = transform.GetChild(childIndex).gameObject; }
            else if (childIndex == nofChild - 1) { particleDefeated = transform.GetChild(childIndex).gameObject; }
        }

        // Camera that follows player
        vcFollower = GameObject.FindGameObjectWithTag("VirCamBrain");

        // Player is not jumping at the start
        jumping = false;

        // At the start, character is set to Square shape
        ShapeShift(2, true);

        // Fade
        fadeCoefficient = 0f;
        fusionInitiated = false;

        // Swipe components
        instructionShowed = (PlayerPrefs.GetString("InstructionShowed", "n") == "y");
    }

    // Update is called once per frame
    void Update()
    {
        if (!MainCtrlManager.GamePaused)
        {
            // Set rigidbody to dynamic so that it falls down
            if (rb.bodyType != RigidbodyType2D.Dynamic)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;

                // Dissolve back to initial state
                fadeCoefficient = 0;
                for (int index = 0; index < dissolves.Length; index++) { dissolves[index].SetFloat("_Fade", fadeCoefficient); }

                Invoke("MoveAnimation", 0.5f);
                particleExp.SetActive(false);
            }

            // Stabilize character
            transform.rotation = Quaternion.identity;

            // Avoid being pushed left and right
            rb.velocity = new Vector3(0, rb.velocity.y, 0);

            // Shapeshifting and jumping by key arrows__________________________
            if (Input.GetKey(KeyCode.UpArrow) && instructionShowed) { Jump(); }

            else if (Input.GetKey(KeyCode.RightArrow) && instructionShowed)
            { if (currentShapeID != 0) ShapeShift(0, false); }

            else if (Input.GetKey(KeyCode.DownArrow) && instructionShowed)
            { if (currentShapeID != 1) ShapeShift(1, false); }

            else if (Input.GetKey(KeyCode.LeftArrow) && instructionShowed)
            { if (currentShapeID != 2) ShapeShift(2, false); }
            
            // Shapeshifting and jumping by swipe_______________________________
            if (Input.touchCount > 0 && instructionShowed)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        startSwipePosition = touch.position;
                        break;
                    
                    case TouchPhase.Ended:
                        endSwipePosition = touch.position;
                        ActAccordingToSwipe(DecideSwipeDirection(startSwipePosition, endSwipePosition));
                        break;
                }
            }
            // _________________________________________________________________

            if (jumping && IsGrounded())
            {
                animator.SetTrigger("Move");
                jumping = false;
            }

            // This section is PRIVATE to some of the characters________________

            if (effectImg != null)
            {
                // -> Electric
                for (int index = 0; index < effectImg.transform.childCount; index++)
                {
                    ElectricShow eShow = effectImg.transform.GetChild(index).GetComponent<ElectricShow>();
                    if (eShow != null)
                    {
                        if (Mathf.Abs(rb.velocity.y) >= 0.5f)
                        {
                            eShow.StopLighting();
                            effectImg.SetActive(false);
                        }
                    }
                }

                // -> Forest
                if (!effectImg.activeSelf && effectImg.transform.childCount == 1) { effectImg.SetActive(true); }
            }
        }

        else if (!MainCtrlManager.GameStarted)
        {
            // Set rigidbody to static so that it does not fall down
            if (!fusionInitiated)
            {
                rb.bodyType = RigidbodyType2D.Static;
                particleFusion.SetActive(true);
                Invoke("SquareGlow", 5.001f);
                fusionInitiated = true;
            }

            // Only call if fusion is not completed
            if (fadeCoefficient < 1f) ShapeFusion();
        }

        else
        {
            if (rb.bodyType != RigidbodyType2D.Static) { rb.bodyType = RigidbodyType2D.Static; }
        }
    }

    // To be called right after fusion (5 first seconds), make an impact and get ready for the game 
    private void SquareGlow()
    {
        // Make the particle effect for fusion inactive
        particleFusion.SetActive(false);

        // Set glow effect appropriately
        for (int index = 0; index < dissolvedParts.Length; index++)
        {
            dissolvedParts[index].GetComponent<SpriteRenderer>().material = glows[index];
        }

        // Make this glow has some explosive effect
        particleExp.SetActive(true);

        // Shake camera harder
        vcFollower.GetComponent<CamShake>().ShakeCamera(3.5f, 6, 0.5f);
    }

    // Checks if player is standing on any platform (ground)
    private bool IsGrounded()
    {
        RaycastHit2D raycastHit2D = Physics2D.BoxCast
            (playerCollider.bounds.center, playerCollider.bounds.size, 0f, Vector2.down, playerHeight, platformMask);
        return raycastHit2D.collider != null;
    }

    // Player jump - Button
    public void Jump()
    {
        if (IsGrounded())
        {
            animator.SetTrigger("Jump");
            Invoke("DelayedJump", Time.deltaTime);
            Invoke("SetJumping", 0.2f);
        }
    }

    // Jump after character presses a bit
    private void DelayedJump() { rb.velocity = Vector2.up * jumpForce; }

    // Simply set jumping to false, to avoid incorrect animation while playing
    private void SetJumping() { jumping = true; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Field"))
        {
            if (effectImg != null)
            {
                for (int index = 0; index < effectImg.transform.childCount; index++)
                {
                    // -> Electric
                    ElectricShow eShow = effectImg.transform.GetChild(index).GetComponent<ElectricShow>();
                    if (eShow != null)
                    {
                        effectImg.SetActive(true);
                        eShow.ChargeLighting();
                    }
                }
            }
        }

        else if (collision.collider.CompareTag("FallBorder"))
        {
            if (!fallen)
            {
                Defeated();
                fallen = true;
            }

            if (PlayerPrefs.GetString("InstructionShowed", "n") == "n")
            {
                GameObject.FindGameObjectWithTag("GMScript").GetComponent<PlrPrefsCtrl>().OutGamePlay();
            }
        }
    }

    // To be called on user's swipes, in order to change shape
    public void ShapeShift(int shapeID, bool calledBeforeFusion)
    {
        // Change shape ID and trigger "Shift", need both to change shape
        animator.SetInteger("ShapeID", shapeID);
        animator.SetTrigger("Shift");

        // Check what shape to be set active
        List<GameObject> shapes = new List<GameObject>() { triangleImg, squareImg, polygonImg };
        for (int index = 0; index < shapes.Count; index++)
        {
            if (index == shapeID) shapes[index].SetActive(true);
            else shapes[index].SetActive(false);
        }

        // Change current shape ID to interact with obstacle
        currentShapeID = shapeID;

        // Switch to moving animation, but if this shape-shifting is called before fusion (at the start or at revive), don't switch
        if (!calledBeforeFusion) Invoke("MoveAnimation", 0.1f);
    }

    // To trigger moving animation
    private void MoveAnimation()
    {
        animator.SetTrigger("Move");
    }

    // Slowly fuse the player at the first 5 seconds
    private void ShapeFusion()
    {
        // Slowly increase dissolve parameter to fuse character
        fadeCoefficient += Time.deltaTime * 0.19f; //0.2f seems a bit too abundant

        // Set the fadeCoefficient to each dissolve material
        for (int index = 0; index < dissolves.Length; index++) { dissolves[index].SetFloat("_Fade", fadeCoefficient); }

        // Adjust all particle effects according to "fadeCoefficient", the higher the coefficient, the faster and more intensive the effects
        int pfChildren = particleFusion.transform.childCount;
        for (int pIndex = 1; pIndex < pfChildren; pIndex++)
        {
            var ps = particleFusion.transform.GetChild(pIndex).GetComponent<ParticleSystem>();
            var tempMain = ps.velocityOverLifetime;
            var tempEmission = ps.emission;
            tempMain.radial = Mathf.Lerp(-5, -10, fadeCoefficient);
            tempEmission.rateOverTime = (int)Mathf.Lerp(15, 20, fadeCoefficient);
        }

        // Shake camera when fusing character
        vcFollower.GetComponent<CamShake>().ShakeCamera(Mathf.Lerp(1.5f, 2.5f, fadeCoefficient), 7, 0.5f);
    }

    // Called when defeated
    public void Defeated()
    {
        // Character explode and disappear
        int nofChild = transform.childCount;
        for (int index = 0; index < nofChild; index++)
        {
            if (index == nofChild - 1) { transform.GetChild(index).gameObject.SetActive(true); }
            else                       { transform.GetChild(index).gameObject.SetActive(false); }
        }

        // Close the bool gate first to stop the fields and on-airs
        MainCtrlManager.GamePaused = true;

        // Actually pause game after 1 second (to let user see visual effects)
        Invoke("PassivePauseGame", 1.5f);
    }

    // Called when player finishes watching rewarded ad
    public void ReviveCharacter()
    {
        // Close revived bool gate
        revived = true;

        // Get ready for fusion
        SetToInitialState();

        // Character reappear
        int nofChild = transform.childCount;
        for (int index = 0; index < nofChild; index++)
        {
            if (index == 2 || index == 3 || index == nofChild - 3)
            { transform.GetChild(index).gameObject.SetActive(true); }

            else
            { transform.GetChild(index).gameObject.SetActive(false); }
        }
    }

    // Pause game
    private void PassivePauseGame()
    {
        // GM Script
        MainCtrlManager gmScript = GameObject.FindGameObjectWithTag("GMScript").GetComponent<MainCtrlManager>();
        gmScript.PauseAfterDefeated();
    }

    // Get character ready for revive fusion
    private void SetToInitialState()
    {
        // Relocate character
        transform.position = GameObject.FindGameObjectWithTag("OfficialSpawnPostion").transform.position;

        // Open fallen gate
        fallen = false;

        // Open fusionInitiated bool gate
        fusionInitiated = false;

        // Dissolve back to initial state
        for (int index = 0; index < dissolves.Length; index++) { dissolves[index].SetFloat("_Fade", fadeCoefficient); }

        // Replace glow with dissolve material
        for (int index = 0; index < dissolvedParts.Length; index++)
        {
            dissolvedParts[index].GetComponent<SpriteRenderer>().material = dissolves[index];
        }

        // Shape shift
        ShapeShift(2, true);
    }

    // Handle swipes____________________________________________________________
    public int DecideSwipeDirection(Vector3 startVector, Vector3 endVector)
    {
        if (Vector3.Distance(startVector, endVector) >= minSwipeDistance)
        {
            Vector3 direction = endVector - startVector;

            // Direction is in quad I
            if (direction.x >= 0 && direction.y >= 0)
            {
                if (Vector3.Angle(Vector3.up, direction) < 45f){ return 3; }
                else { return 0; }
            }

            // Direction is in quad II
            else if (direction.x < 0 && direction.y >= 0)
            {
                if (Vector3.Angle(Vector3.up, direction) < 45f) { return 3; }
                else { return 2; }
            }

            // Direction is in quad III
            else if (direction.x < 0 && direction.y < 0)
            {
                if (Vector3.Angle(Vector3.down, direction) < 45f) { return 1; }
                else { return 2; }
            }

            // Direction is in quad IV
            else
            {
                if (Vector3.Angle(Vector3.down, direction) < 45f) { return 1; }
                else { return 0; }
            }
        }

        else
        {
            return 4;
        }
    }

    private void ActAccordingToSwipe(int inputSwipe)
    {
        if (inputSwipe == 0)
        {
            if (currentShapeID != 0) { ShapeShift(0, false); }
        }

        else if (inputSwipe == 1)
        {
            if (currentShapeID != 1) { ShapeShift(1, false); }
        }

        else if (inputSwipe == 2)
        {
            if (currentShapeID != 2) { ShapeShift(2, false); }
        }

        else if (inputSwipe == 3)
        {
            Jump();
        }
    }

}
