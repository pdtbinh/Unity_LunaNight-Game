using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;

public class PlrPrefsCtrl : MonoBehaviour, IUnityAdsListener
{
    // Currency: PlayerPrefs_Int -> CurrentMoney
    public static int currentMoney; // Total money
    public TextMeshProUGUI currentMoneyText;
    
    public static int moneyGained; // Money gained in each gameplay (reset to 0 after each gameplay finishes)
    public TextMeshProUGUI moneyGainedText;

    // High score: PlayerPrefs_Int -> BestScore
    public static int currentScore; // Current highest score
    public TextMeshProUGUI currentScoreText;

    // Quests
    public static int pointsRequired;
    public TextMeshProUGUI pointsRequiredText; // PlayerPrefs_Int -> PtsRequired

    // PlayerPrefs_Int -> PtsCompleted: This variable only exists logically
    public static int pointsCompleted;

    public static int pointsGained; // Points gained in each gameplay (reset to 0 after each gameplay finishes)
    public TextMeshProUGUI pointsGainedText;   

    public static int adWatched;         
    public TextMeshProUGUI adWatchedText;      // PlayerPrefs_Int -> AdWatched (0 means false, 1 means true)

    // Start is called before the first frame update
    void Start()
    {
        // Set currency
        currentMoney = PlayerPrefs.GetInt("CurrentMoney", 0);
        currentMoneyText.text = "0";

        // Set high score
        currentScore = PlayerPrefs.GetInt("CurrentScore", 0);
        currentScoreText.text = "0";

        // Set quests
        pointsRequired = PlayerPrefs.GetInt("PtsRequired", 1);
        pointsRequiredText.text = "1. Earn " + pointsRequired.ToString() + " pts.";

        pointsCompleted = PlayerPrefs.GetInt("PtsCompleted", 0);
        if (pointsCompleted > 0) { pointsRequiredText.text = "1. Done!"; }
        else { }

        adWatchedText.text = "2. Watch ad video.";

        adWatched = PlayerPrefs.GetInt("AdWatched", 0);
        if (adWatched > 0) { adWatchedText.text = "2. Done!"; }
        else { }

        // At the start, only reset money gained and points gained. After that, quickly generate current money and score
        RestartMoneyGained();
        RestartPointsGained();

        // Start increasing current money text in step of 2
        int moneySteps = (int)Mathf.Max(1, (int)currentMoney / 100);
        StartCoroutine(IncreaseInStep(currentMoneyText,
            0, currentMoney,
            0f, moneySteps)
            );

        // Start increasing current score text in step of 2
        int scoreSteps = (int)Mathf.Max(1, (int)currentScore / 150);
        StartCoroutine(IncreaseInStep(currentScoreText,
            0, currentScore,
            0f, scoreSteps)
            );

        //_________________________________AD___________________________________
        Advertisement.AddListener(this);
        InitializeAd();
    }

    // CURRENCY_________________________________________________________________
    // To be called after user's purchase or gameplay
    public void ChangeCurrentMoney(int addedMoney)
    {
        // Actual current money data changes here
        currentMoney += addedMoney;

        // Checks if money has reached out of range
        currentMoney = Mathf.Min(99999, currentMoney);

        PlayerPrefs.SetInt("CurrentMoney", currentMoney);
    }

    // To be called continuously during gameplay
    public void ChangeMoneyGained(int addedMoney)
    {
        if (moneyGained < 99999)
        {
            // Actual money gained data changes here
            int initialMoney = moneyGained;
            moneyGained += addedMoney;

            // Start increasing money gained text in step of 1
            StartCoroutine(IncreaseInStep(moneyGainedText, initialMoney, addedMoney, 0.1f, 1));
        }
    }

    // Money gained always starts at 0 at the start of each gameplay
    public void RestartMoneyGained()
    {
        moneyGained = 0;
        moneyGainedText.text = "0";
    }
    
    // SCORE____________________________________________________________________
    // To be called after gameplay if highest score record is broken
    public void ChangeCurrentScore(int newScore)
    {
        // Actual current score data changes here and checks if score has reached out of range
        currentScore = Mathf.Min(999999, newScore);

        PlayerPrefs.SetInt("CurrentScore", currentScore);
    }

    // To be called continuously during gameplay
    public void ChangePointsGained(int addedPoints)
    {
        if (pointsGained < 999999)
        {
            // Actual points gained data changes here
            int initialPoints = pointsGained;
            pointsGained += addedPoints;

            // Start increasing points gained text in step of 1
            StartCoroutine(IncreaseInStep(pointsGainedText, initialPoints, addedPoints, 0.05f, 1));
        }
    }

    // To be called on claim
    public void ChangePointsRequired(int addedPoints)
    {
        if (pointsRequired < 999999)
        {
            // Actual data changes here
            PlayerPrefs.SetInt("PtsRequired", pointsRequired + addedPoints);
            pointsRequired = PlayerPrefs.GetInt("PtsRequired", 1);

            // Visuals
            pointsRequiredText.text = "1. Earn " + pointsRequired.ToString() + " pts.";
        }
    }

    // Points gained always starts at 0 at the start of each gameplay
    public void RestartPointsGained()
    {
        pointsGained = 0;
        pointsGainedText.text = "0";
    }

    // Method to increase fields IN STEPS, does not change actual data__________
    IEnumerator IncreaseInStep(TextMeshProUGUI valueField,
        int initialValue, int addedValue,
        float stepInSeconds, int inStepOf)
    {
        int add = 1;
        while (add < addedValue + 1)
        {
            valueField.text = (initialValue + add).ToString();
            
            if (add < addedValue) { add += (addedValue - add > inStepOf) ? inStepOf : (addedValue - add); }
            else { break; }

            yield return new WaitForSeconds(stepInSeconds);
        }
    }

    // Oposite to the IncreaseInStep method
    IEnumerator DecreaseInStep(TextMeshProUGUI valueField,
        int initialValue, int addedValue,
        float stepInSeconds, int inStepOf)
    {
        int add = 1;
        while (add < addedValue + 1)
        {
            valueField.text = (initialValue - add).ToString();

            if (add < addedValue) { add += (addedValue - add > inStepOf) ? inStepOf : (addedValue - add); }
            else { break; }

            yield return new WaitForSeconds(stepInSeconds);
        }
    }

    // Combined with DecreaseInStep method to be used in Shop control
    public void decreaseMoney(int cost)
    {
        // Store to be used in visuals
        int initialMoney = currentMoney;

        // Actual data changes here
        currentMoney -= cost;
        currentMoney = Mathf.Max(currentMoney, 0);
        PlayerPrefs.SetInt("CurrentMoney", currentMoney);

        // Visuals
        StartCoroutine(DecreaseInStep(currentMoneyText, initialMoney, cost, 0.05f, 1));
    }

    // Called after each gameplay is done
    public void OutGamePlay()
    {
        // Effect money and score
        ChangeCurrentMoney(moneyGained);

        if (pointsGained > currentScore)
        {
            ChangeCurrentScore(pointsGained);
        }

        // Effect quest
        if (pointsGained >= pointsRequired)
        {
            // Adjusting variables
            PlayerPrefs.SetInt("PtsCompleted", 1);
            pointsCompleted = 1;
        }

        if (PlayerPrefs.GetString("InstructionShowed", "n") == "n")
        { PlayerPrefs.SetString("InstructionShowed", "y"); }

        Advertisement.RemoveListener(this);
        SceneManager.LoadScene(0);
    }

    // Called when player finishes watching ad
    public void FinishWatchingAd()
    {
        if (adWatched < 1)
        {
            // Adjusting variables
            PlayerPrefs.SetInt("AdWatched", 1);
            adWatched = 1;

            // Switch text color to green - move to AdManager
            adWatchedText.text = "2. Done!";
        }
        // Play audio again
        AudioListener.pause = false; 
    }

    // Called when player click "Claim" the quest reward
    public void ClaimReward()
    {
        if (pointsCompleted == 0)
        {
            GetComponent<MainCtrlManager>().PrintErrorText("Quest 1 hasn't been completed.");
        }

        else if (adWatched == 0)
        {
            GetComponent<MainCtrlManager>().PrintErrorText("Quest 2 hasn't been completed.");
        }

        else if (pointsCompleted > 0 && adWatched > 0 && currentMoney < 99999)
        {
            // Adjusting variables: Points
            PlayerPrefs.SetInt("PtsCompleted", 0);
            pointsCompleted = 0;

            if (pointsRequired < 2) { ChangePointsRequired(9); }
            else { ChangePointsRequired(10); }

            // Adjusting variables: Ad
            PlayerPrefs.SetInt("AdWatched", 0);
            adWatched = 0;

            

            // GIVING REWARDS:
            int tempMoney = currentMoney;
            int addedRewardMoney = (int)(Random.Range(1, 5) * 10);

            GetComponent<MainCtrlManager>().PrintErrorText("Congratulations, you have won " + addedRewardMoney.ToString() + " Coins.");

            ChangeCurrentMoney(addedRewardMoney);
            StartCoroutine(IncreaseInStep(currentMoneyText, tempMoney, addedRewardMoney, 0.05f, 1));
        }

        else if (currentMoney >= 99999)
        {
            GetComponent<MainCtrlManager>().PrintErrorText("Money over capacity!");
        }
    }

    // Used to decide which title to print on title of revive panel
    public string reviveTitle()
    {
        string printedText = "";

        if (!PlayerCtrl.revived)
        {
            if (pointsGained > currentScore)
            {
                printedText = "CONGRATS";
            }

            else
            {
                printedText = "DEFEATED";
            }
        }
        else
        {
            if (pointsGained > currentScore)
            {
                printedText = "CONGRATS";
            }

            else
            {
                printedText = "NICE RUN";
            }
        }

        return printedText;
    }

    // Used to decide which text to print on encouragingText
    public string encourageRevive()
    {
        string printedText = "";

        if (!PlayerCtrl.revived)
        {
            if (pointsGained < pointsRequired)
            {
                printedText = "You need "
                    + (pointsRequired - pointsGained).ToString()
                    + " more point(s)\nto complete quest."; 
            }
            else if (pointsGained < currentScore)
            {
                printedText = "You need "
                    + (currentScore - pointsGained).ToString()
                    + " more point(s)\nto break current record.";
            }
            else if (pointsGained == currentScore)
            {
                printedText = "You need 1 more point\nto break current record.";
            }
            else if (pointsGained > currentScore)
            {
                printedText = "You created a new record!\nRevive to go even further.";
            }
        }
        else
        {
            if (pointsGained > currentScore)
            {
                printedText = "You created a new record!";
            }

            else
            {
                printedText = "Let's play it again!";
            }
        }

        return printedText;
    }

    // Only for developing purposes
    public void RestartAllVariables()
    {
        PlayerPrefs.DeleteAll();
        Advertisement.RemoveListener(this);
        SceneManager.LoadScene(0);
    }

    //__________________________________________________________________________
    //_______________________________AD MANAGER_________________________________
    //__________________________________________________________________________

    private string playStoreID = "3936807";
    private string appStoreID = "3936806";

    private string rewardedVideoAd = "rewardedVideo";

    public bool targetPlayStore;
    public bool isTestAd;

    private void InitializeAd()
    {
        if (targetPlayStore) { Advertisement.Initialize(playStoreID, isTestAd); return; }
        Advertisement.Initialize(appStoreID, isTestAd);
    }

    public void PlayRewardedVideoAd()
    {
        if (!Advertisement.IsReady(rewardedVideoAd)) { return; }
        Advertisement.Show(rewardedVideoAd);
    }

    // Interface for IUnityAdsListener
    public void OnUnityAdsReady(string placementId)
    {
        //throw new System.NotImplementedException();
    }

    public void OnUnityAdsDidError(string message)
    {
        //throw new System.NotImplementedException();
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        //throw new System.NotImplementedException();
        AudioListener.pause = true;

    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        //throw new System.NotImplementedException();
        switch (showResult)
        {
            case ShowResult.Failed:
                GetComponent<MainCtrlManager>().PrintErrorText("Sorry, failed to finish ad video.");
                break;
            case ShowResult.Skipped:
                GetComponent<MainCtrlManager>().PrintErrorText("Sorry, failed to finish ad video.");
                break;
            case ShowResult.Finished:
                if (placementId == rewardedVideoAd)
                {
                    // Called when player is trying to complete quest
                    if (!MainCtrlManager.GameStarted)
                    {
                        // Reward player
                        FinishWatchingAd();
                    }

                    // Called when player is trying to revive
                    else
                    {
                        // Revive player at official postion
                        PlayerCtrl player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCtrl>();
                        player.ReviveCharacter();

                        // Start the game
                        MainCtrlManager gmScript = GameObject.FindGameObjectWithTag("GMScript").GetComponent<MainCtrlManager>();
                        gmScript.ResumeAfterDefeated();
                    }
                }
                break;
        }
    }
}
