using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class InstructionSteps : MonoBehaviour
{
    // Gates:
    private bool instructionStarted; // Open this gate to initiate instruction
    
    // Phases:
    private int currentPhase; // The current phase, based on the quotesFromInstructor
    private bool phaseLoaded; // Check if the new phase is loaded
    private bool waitingTilDetection; // To avoid making characater auto run when waiting til detection
    private bool allowDetection; // Allow recieving action (touch and swipe) from player
   
    // Player
    public GameObject player;

    // Platforms:
    public GameObject[] platforms;

    // UI texts:
    public TextMeshProUGUI tutorials;

    // Arrows:
    public Transform guidancesTool;

    // Tutorial texts:
    private string[] quotesFromInstructor = {
           "Greetings, heroes. We have long waited for your help to save our planet."
            + " But first, let me walk you through a brief training session.", // 0

        "There is <color=#00ff00ff>your character</color>.", // 1

        "And there is the <color=#ff0000ff>enemy character</color>.", // 2

        "Shapeshift your character to <color=#ffa500ff>the same shape</color> as the enemy's to destroy it."
            + "Your mission is to destroy as many enemies as possible.", // 3

        "<color=#00ff00ff>Swipe right</color> to transform to <color=#00ff00ff>Triangular</color> form.", // 4

        "<color=#00ff00ff>Swipe down</color> to transform to <color=#00ff00ff>Square</color> form.", // 5

        "<color=#00ff00ff>Swipe left</color> to transform to <color=#00ff00ff>Pentagon</color> form.", // 6

        "<color=#00ff00ff>Swipe up</color> to make your character <color=#00ff00ff>jump</color>.", // 7

        "Well done!", // 8

        "This is the <color=#ff00ffff>Points</color> you gain through destroying any enemy.", // 9

        "This is the <color=#ffa500ff>Coins</color> you earn through destroying special enemies."
            + "\nYou can also earn <color=#ffa500ff>Coins</color> from <color=#00ffffff>Quests</color> in the Main Menu,"
            + " and use them to purchase <color=#00ff00ff>more characters</color>.", // 10

        "This session ends here.\nLet's get ready and restore our people's faith in the Legends." // 11
    };

    private KeyCode[] keyCodes = { KeyCode.RightArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.UpArrow };

    public Transform[] tool_0;
    public Transform[] tool_1;
    public Transform[] tool_2;
    public Transform[] tool_3;
    public Transform[] tool_4;
    public Transform[] tool_5;
    public Transform[] tool_6;
    public Transform[] tool_7;
    public Transform[] tool_8;
    public Transform[] tool_9;
    public Transform[] tool_10;
    public Transform[] tool_11;
    private List<Transform[]> tools;

    // Swipe components_________________________________________________________
    private Vector3 startSwipePosition;
    private Vector3 endSwipePosition;
    //__________________________________________________________________________

    // Start is called before the first frame update
    void Start()
    {
        // Gates:
        instructionStarted = false;
        
        // Phases:
        currentPhase = 0;
        phaseLoaded = false;
        waitingTilDetection = false;
        allowDetection = false;

        // Tools:
        tools = new List<Transform[]>() { tool_0, tool_1, tool_2, tool_3, tool_4, tool_5,
            tool_6, tool_7, tool_8, tool_9, tool_10, tool_11 };
    }

    // Update is called once per frame
    void Update()
    {
        if (instructionStarted)
        {
            if (!phaseLoaded)
            {
                MoveToPhase();
                StartCoroutine(WaitTilDetection(0.5f));
            }

            else if (waitingTilDetection) { } // do nothing, just wait

            else if (allowDetection)
            {
                if (currentPhase < 4 || currentPhase > 7)
                {
                    DetectPlayerTouch();
                }

                else
                {
                    DetectPlayerSwipe();
                }
            }

            else
            {
                CharacterAutoRun();
            }
        }
    }

    // This signals that the tutorial is starting
    public void InitiateInstruction() { instructionStarted = true; }

    
    /* Every phase is divided to 3 (4) sections: 
     * 1   - MoveToPhase 
     * 2   - Detect action 
     *(2.5 - Auto run) 
     * 3   - Clear up phase */ 
    // Section 1:
    private void MoveToPhase()
    {
        // Pause the game
        MainCtrlManager.GamePaused = true;
        
        // And let the teacher talks
        tutorials.text = quotesFromInstructor[currentPhase];
        tutorials.GetComponent<Animator>().Play("Base Layer.TextAppear", -1, 0f);

        // Turn on all tools needed for this instruction
        TurnOnTheTools(tools[currentPhase], true);

        // Close the gate
        phaseLoaded = true;
        waitingTilDetection = true;
    }

    // Section 2:
    private void DetectPlayerTouch()
    {
        if (Input.touchCount > 0 || Input.GetKey(KeyCode.Space)) { ClearThisPhase(false); }
    }

    private void DetectPlayerSwipe()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startSwipePosition = touch.position;
                    break;
                case TouchPhase.Ended:
                    endSwipePosition = touch.position;
                    if (player != null)
                    {
                        if (player
                            .GetComponent<PlayerCtrl>()
                            .DecideSwipeDirection(startSwipePosition, endSwipePosition) == (currentPhase-4))
                        {
                            ClearThisPhase(true);
                            if (currentPhase < 7) { player.GetComponent<PlayerCtrl>().ShapeShift(currentPhase - 4, false); }
                            else { player.GetComponent<PlayerCtrl>().Jump(); }
                        }
                    } 
                    break;
            }
        }

        else
        {
            if (Input.GetKey(keyCodes[currentPhase-4]))
            {
                ClearThisPhase(true);
                if (currentPhase < 7) { player.GetComponent<PlayerCtrl>().ShapeShift(currentPhase - 4, false); }
                else { player.GetComponent<PlayerCtrl>().Jump(); }
            }
        }
    }

    // Section 2.5:
    private void CharacterAutoRun()
    {
        int indexOfAutoRun = currentPhase - 4;
        float distance = platforms[indexOfAutoRun].transform.position.x - player.transform.position.x;

        if ( (indexOfAutoRun == 0 && distance < -7.25f) || (indexOfAutoRun == 1 && distance < -7.25f)
            || (indexOfAutoRun == 2 && distance < -6.4f) || (indexOfAutoRun == 3 && distance < -3.5f))
        { currentPhase += 1; phaseLoaded = false; }
    }

    // Section 3:
    private void ClearThisPhase(bool afterSwipe)
    {
        if (currentPhase < 11)
        {
            // Resume the game
            MainCtrlManager.GamePaused = false;

            // Turn off all tools needed for this instruction
            TurnOnTheTools(tools[currentPhase], false);

            // Move on-to waiting for next phase
            if (!afterSwipe) { currentPhase += 1; phaseLoaded = false; }
            allowDetection = false;
        }
        else
        {
            PlayerPrefs.SetString("InstructionShowed", "y");
            SceneManager.LoadScene(0);
        }
    }

    // Helper method to turn on and off tutorial tools
    private void TurnOnTheTools(Transform[] tools, bool on)
    { for (int index = 0; index < tools.Length; index++) { tools[index].gameObject.SetActive(on); } }

    // Helper method to pause between instruction section of each phase
    IEnumerator WaitTilDetection(float seconds)
    {  yield return new WaitForSeconds(seconds); allowDetection = true; waitingTilDetection = false; }
}
