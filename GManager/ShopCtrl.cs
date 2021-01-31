using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopCtrl : MonoBehaviour
{
    // The current page (also the current id of the character) being viewed
    private int currentPage;
    public TextMeshProUGUI currentPageText;

    // The whole shop panel
    public GameObject shopPanel;

    // Where all character images are stored
    public Transform charCollection;

    // Select and buy buttons
    public GameObject buyButton;
    public GameObject selectButton;

    // Character names
    public TextMeshProUGUI nameTextOfChars;

    private string[] characterNames = {
        "<color=#00ff00ff><i>Greenie</i></color>",

        "<color=#0000ffff><i>Eleeshia</i></color>",

        "<color=#c0c0c0ff><i>Agent #08</i></color>",

        "<color=#00ffffff><i>M'Leville</i></color>",

        "<color=#ff00ffff><i>Sandra Pink</i></color>" };

    public GameObject confirmHolderPanel;
    public GameObject confirmPurchasePanel;

    public TextMeshProUGUI purchaseInfo;

    // Start is called before the first frame update
    void Start()
    {
        currentPage = 1;

        // Setting player preferences should be called only once 
        if (PlayerPrefs.GetInt("CostSet", 0) < 1)
        {
            // Set the cost for each character
            PlayerPrefs.SetInt("Cost_C1", 0);
            PlayerPrefs.SetInt("Cost_C2", 3000);
            PlayerPrefs.SetInt("Cost_C3", 3500);
            PlayerPrefs.SetInt("Cost_C4", 4000);
            PlayerPrefs.SetInt("Cost_C5", 5000);

            // Close this bool gate
            PlayerPrefs.SetInt("CostSet", 10);
        }

        // Set the first showed button correctly
        ShowVisualEffects();
    }

    // PREV/NEXT BUTTON_________________________________________________________

    // When player clicks prev button
    public void PreviousPage()
    {
        // Change current page data
        if (currentPage == 1) { currentPage = 5; }
        else { currentPage -= 1; }

        // Show visual effects
        ShowVisualEffects();
    }

    // When player clicks next button
    public void NextPage()
    {
        // Change current page data
        if (currentPage == 5) { currentPage = 1; }
        else { currentPage += 1; }

        // Show visual effect
        ShowVisualEffects();
    }

    // Decide which character to show and which button to show
    private void ShowVisualEffects()
    {
        // Show visual effects
        ShowCharacter(currentPage);

        string currentCostID = "Cost_C" + currentPage.ToString();
        ShowStatusButton(PlayerPrefs.GetInt(currentCostID), currentPage);

        // Change page text
        currentPageText.text = currentPage.ToString() + "/5";

        // Change name text
        nameTextOfChars.text = characterNames[currentPage - 1];
    }

    // VISUAL EFFECTS___________________________________________________________

    // Decide which character to show when player clicks prev/next button
    private void ShowCharacter(int charPageIndex) // Page indexes start at 1
    {
        for (int index = 0; index < charCollection.childCount; index++)
        {
            if (index == charPageIndex - 1)
            {
                charCollection.GetChild(index).gameObject.SetActive(true);
            }
            else
            {
                charCollection.GetChild(index).gameObject.SetActive(false);
            }   
        }
    }

    // Decide which button to show on the viewed character
    private void ShowStatusButton(int charCost, int charID) // charID is also the current page
    {
        if (charCost > 1)
        {
            buyButton.SetActive(true);
            selectButton.SetActive(false);
        }

        else if (charCost < 1)
        {
            buyButton.SetActive(false);

            if (PlayerPrefs.GetInt("ChosenCharacter", 1) == charID)
            {
                selectButton.SetActive(false);
            }

            else
            {
                selectButton.SetActive(true);
            }
        }
    }

    // BUYING AND SELECTING CHARACTER___________________________________________

    public void SelectCharacter()
    {
        // Visuals
        selectButton.SetActive(false);
        
        // Data
        PlayerPrefs.SetInt("ChosenCharacter", currentPage);
    }

    public void PurchaseCharacter()
    {
        // Choose current charcter name
        string characterName = characterNames[currentPage - 1];

        string currentCostID = "Cost_C" + currentPage.ToString();

        string price = PlayerPrefs.GetInt(currentCostID, 10).ToString();

        purchaseInfo.text = "Purchase " + characterName + " for " + price + " Coins?";

        confirmPurchasePanel.SetActive(true);
        confirmHolderPanel.SetActive(true);
    }

    public void AgreeToBuyCharacter()
    {
        // Choose current charcter name
        string characterName = characterNames[currentPage - 1];

        string currentCostID = "Cost_C" + currentPage.ToString();

        if (PlayerPrefs.GetInt("CurrentMoney", 0) >= PlayerPrefs.GetInt(currentCostID, 10))
        {
            ActuallyBuyCharacter();

            GetComponent<MainCtrlManager>().PrintErrorText("Thank you for your purchase!\n" + characterName + " has joined your squad.");
        }

        else
        {
            confirmPurchasePanel.SetActive(false);
            GetComponent<MainCtrlManager>().PrintErrorText("Sorry, you don't have enough Coins.");
        }
    }

    public void ActuallyBuyCharacter()
    {
        string currentCostID = "Cost_C" + currentPage.ToString();

        // Economic effect
        GetComponent<PlrPrefsCtrl>().decreaseMoney(PlayerPrefs.GetInt(currentCostID, 10));

        // Visuals
        buyButton.SetActive(false);

        // Data
        PlayerPrefs.SetInt(currentCostID, 0);

        // And select this character as chosen character
        SelectCharacter();

        // Close bill after purchase
        CloseConfirmPurchasePanel();
    }

    public void CloseConfirmPurchasePanel()
    {
        confirmHolderPanel.SetActive(false);
        confirmPurchasePanel.SetActive(false);
    }

    // OPEN AND CLOSE SHOP PANEL________________________________________________

    public void PopUpShopPanel() { shopPanel.SetActive(true); }

    public void CloseShopPanel() { shopPanel.SetActive(false); }
}
