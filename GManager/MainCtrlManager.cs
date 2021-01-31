using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainCtrlManager : MonoBehaviour
{
    // Game components:
    public static bool GameStarted;

    public static bool GamePaused;

    // Track the current game speed, decided by player and platforms
    public static float currentGameSpeed;

    // Menu components:
    public GameObject StartPanel;

    // Player component:
    public GameObject virtualCamera;

    public Transform instructionPlSpnPos;

    public Transform instructionParent;

    public Transform officialPlSpnPos;

    public GameObject playerPref_1;

    public GameObject playerPref_2;

    public GameObject playerPref_3;

    public GameObject playerPref_4;

    public GameObject playerPref_5;

    public GameObject platformGenerator;

    public GameObject backgroundGenerator;

    // Countdown components:
    public GameObject cdText_object;

    // Ingame components:
    public GameObject ingamePanel;

    // Audios
    public AudioSource bdgMusic;

    public AudioSource clickSound;

    public AudioSource cdSound;

    // Pause components:
    public GameObject pausePanel;

    public GameObject confirmPausePanel;

    // Screen changing components:
    public GameObject screenPanel;

    public Material doorGlow;

    public Material nodeGlow;

    // Revive components:
    public GameObject revivePanel;

    public GameObject reviveButton;

    public GameObject quitButton;

    public GameObject quitAfterReviveButton;

    public GameObject confirmRevivePanel;

    public TextMeshProUGUI titleReviveText;

    // -> E.g., need x more points to y (before revive), congratulations you created a new record (after revivie)
    public TextMeshProUGUI encouragingText;

    public GameObject questionReviveText;

    // Error log components for player:
    public GameObject notiHolderPanel;

    public GameObject errorNotificationPanel;

    public TextMeshProUGUI printedErrorText;

    // Instruction components:
    public GameObject instructionPanel;

    public GameObject instructionConfirmPanel;

    // Rate us components
    public GameObject rateUsPanel;

    // Safe gate:
    public bool playGameTap;

    // Start is called before the first frame update
    void Start()
    {
        // Initial state of the game is paused and not started
        GameStarted = false;

        GamePaused = true;

        playGameTap = false;

        // Adjust this to make smooth gameplay
        currentGameSpeed = 1f;

        // At the start, timeScale is at normal
        Time.timeScale = 1f;

        // Suggest rating

        // Set seed differently each time to make it seems random
        Random.InitState((int)System.DateTime.Now.Ticks / 1500);

        int chanceToSuggest = Random.Range(0, 101);
        if (chanceToSuggest <= 15 && PlayerPrefs.GetInt("PlayerRated", 0) < 1 && PlayerPrefs.GetInt("CurrentScore", 0) >= 200)
        { SuggestRating(); }
    }

    // ___________________________STARTING THE GAME_____________________________

    // To be called when player tap the start-game button
    public void StartGame()
    {
        if (!playGameTap)
        {
            // Stop music
            bdgMusic.Stop();

            // Call screen change immediately
            ScreenChange();

            // Set up surrounding environment 1 second after screen change starts
            Invoke("SetEnvironment", 1f);

            // Start the game after screen changing is done
            Invoke("SpawnPlayerAndResumeGame", 3f);

            playGameTap = true;
        }   
    }

    // Activate screen-changing protocol
    private void ScreenChange()
    {
        screenPanel.SetActive(true);

        // Make door glow 2 seconds after the screen changing is activated
        Invoke("MakeDoorGlow", 1.75f);
    }

    // Make the door glow
    private void MakeDoorGlow()
    {
        // Play music again
        bdgMusic.Play();

        screenPanel.transform.GetChild(0).GetComponent<Image>().material = doorGlow;
        screenPanel.transform.GetChild(1).GetComponent<Image>().material = doorGlow;
        screenPanel.transform.GetChild(1).transform.GetChild(0).GetComponent<Image>().material = nodeGlow;

        // 1 second after the glow takes effect, close this panel
        Invoke("CloseScreenPanel", 1.25f);
    }

    // Close the screen panel
    private void CloseScreenPanel() { PopUpThePanel(screenPanel, false); }

    // This does not set up surrounding environment anymore
    private void SpawnPlayerAndResumeGame()
    {
        // Spawn the player
        SpawnPlayerAtStart();

        // Start the game after player's fusion
        ResumeGame();
    }

    // Combine with "SpawnPlayer" to decide which character to spawn and spawn at instruction or actual gameplay
    private void SpawnPlayerAtStart()
    {
        if (PlayerPrefs.GetString("InstructionShowed", "n") == "y")
        {
            SpawnPlayer(PlayerPrefs.GetInt("ChosenCharacter", 1), officialPlSpnPos);
        }

        else
        {
            SpawnPlayer(PlayerPrefs.GetInt("ChosenCharacter", 1), instructionPlSpnPos, instructionParent);
        }
    }

    // This is the helper method to be called on start game
    private void SpawnPlayer(int chosenCharID, Transform positionToSpawn, Transform parent = null)
    {
        GameObject pl;
        if (chosenCharID == 1)
        {
            pl = Instantiate(playerPref_1, positionToSpawn.position, Quaternion.identity, parent);
        }

        else if (chosenCharID == 2)
        {
            pl = Instantiate(playerPref_2, positionToSpawn.position, Quaternion.identity, parent);
        }

        else if (chosenCharID == 3)
        {
            pl = Instantiate(playerPref_3, positionToSpawn.position, Quaternion.identity, parent);
        }

        else if (chosenCharID == 4)
        {
            pl = Instantiate(playerPref_4, positionToSpawn.position, Quaternion.identity, parent);
        }

        else
        {
            pl = Instantiate(playerPref_5, positionToSpawn.position, Quaternion.identity, parent);
        }
        virtualCamera.GetComponent<CamShake>().FollowPlayer(pl.transform);
        GetComponent<InstructionSteps>().player = pl;
    }


    // Set up the surrounding environment for player. This should happen at one exact moment :)
    private void SetEnvironment()
    {
        // Set the main menu panel to inactive and ingame panel to active
        StartPanel.SetActive(false);
        ingamePanel.SetActive(true);

        // Ingame components
        backgroundGenerator.SetActive(true);

        if (PlayerPrefs.GetString("InstructionShowed", "n") == "y")
        {
            platformGenerator.SetActive(true);
        }

        else
        {
            instructionParent.gameObject.SetActive(true);
        }
    }

    // Error notifications
    public void PrintErrorText(string textToPrint)
    {
        // Change the text
        printedErrorText.text = textToPrint;

        // Pop-up the error panel
        PopUpThePanel(notiHolderPanel, true);
        PopUpThePanel(errorNotificationPanel, true);
    }

    public void CloseNotiPanel()
    {
        PopUpThePanel(errorNotificationPanel, false);
        PopUpThePanel(notiHolderPanel, false);
    }

    // _________________________________IN-GAMES________________________________

    // To be called when player presses pause
    public void PauseGame()
    {
        if (!GamePaused)
        {
            GamePaused = true;
            Time.timeScale = 0f;

            PopUpThePanel(pausePanel, true);
        }
    }

    // To be called when player presses resume
    public void ResumeGame()
    {
        if (GamePaused)
        {
            StartCoroutine(CountDownTo());

            PopUpThePanel(pausePanel, false);
        }
    }

    // Pop-up the confirm panel
    public void PopUpConfirmPausePanel() { PopUpThePanel(confirmPausePanel, true); }

    // Close the confirm panel
    public void CloseConfirmPausePanel() { PopUpThePanel(confirmPausePanel, false); }

    // _________________________________REVIVE__________________________________

    // To be called when player is defeated
    public void PauseAfterDefeated()
    {
        // Pause game
        GamePaused = true;
        Time.timeScale = 0f;
        
        // Pop up panel
        PopUpThePanel(revivePanel, true);

        // Adjust the texts appropriately
        titleReviveText.text = GetComponent<PlrPrefsCtrl>().reviveTitle();
        encouragingText.text = GetComponent<PlrPrefsCtrl>().encourageRevive();

        // Player is only able to revive once
        if (!PlayerCtrl.revived)
        {
            officialPlSpnPos.gameObject.SetActive(true);
        }

        else
        {
            // Don't ask if player want to revive
            questionReviveText.SetActive(false);

            // Don't show pre-revive revive and quit buttons
            reviveButton.SetActive(false);
            quitButton.SetActive(false);

            // Show post-revive quit button
            quitAfterReviveButton.SetActive(true);
        }
    }

    // To be called when player finishes watching rewarded ad
    public void ResumeAfterDefeated()
    {
        // Resume game as normal
        GameStarted = false;
        ResumeGame();

        // Close panel
        PopUpThePanel(revivePanel, false);

        // Turn on audio
        AudioListener.pause = false;
    }

    // Pop-up the confirm panel
    public void PopUpConfirmRevivePanel() { PopUpThePanel(confirmRevivePanel, true); }

    // Close the confirm panel
    public void CloseConfirmReivePanel() { PopUpThePanel(confirmRevivePanel, false); }

    // _________________________________HELPERS__________________________________

    // Pop-up or close the selected panel
    private void PopUpThePanel(GameObject panel, bool popUp) { panel.SetActive(popUp); }

    // Everytime the game starts or resumes, always countdown to get player ready
    IEnumerator CountDownTo()
    {
        Time.timeScale = currentGameSpeed;

        // If this countdown is at the start of the game, wait longer as the character need to be fused
        int seconds = (GameStarted) ? 4 : 9;

        // Actual countdown happens here
        while (seconds > 0)
        {
            // Play audio

            if (seconds == 3) { cdText_object.SetActive(true); }
            if (cdText_object.activeSelf) { cdSound.Play(); }
            cdText_object.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = seconds.ToString();

            yield return new WaitForSeconds(1f);

            seconds -= 1;
        }

        // After count down is done, make countdown text inactive
        cdText_object.SetActive(false);

        // Spawn position is set to inactive - just to make sure
        officialPlSpnPos.gameObject.SetActive(false);

        // Set the state of the game to be not paused anymore
        GamePaused  = false;

        // Set the state of the game to be started already - just to make sure
        GameStarted = true;

        if (PlayerPrefs.GetString("InstructionShowed", "n") != "y")
        {
            PopUpThePanel(instructionPanel, true);
            GetComponent<InstructionSteps>().InitiateInstruction();
        }
    }

    public void PlayClickSound() { clickSound.Play(); }

    // _______________________________RATE US___________________________________
    public void RateUs()
    {
        PlayerPrefs.SetInt("PlayerRated", 2);
        Application.OpenURL("market://details?id=com.Gamellary001.LunaNight");
    }

    public void SuggestRating() { PopUpThePanel(rateUsPanel, true); }

    public void CloseRateUsPanel() { PopUpThePanel(rateUsPanel, false); }

    // ____________________________INSTRUCTION__________________________________
    public void PopUpInstructionConfirm() { PopUpThePanel(instructionConfirmPanel, true); }
    public void CloseInstructionConfirm() { PopUpThePanel(instructionConfirmPanel, false); }

    public void EnterInstructionSession() { PlayerPrefs.SetString("InstructionShowed", "n"); StartGame(); }
}
