using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UILayoutManager : MonoBehaviour
{
    void Start()
    {
        SetupUILayout();
    }
    
    void SetupUILayout()
    {
        // Setup Health Text
        SetupTextElement("Health_Text", new Vector2(10, -10), new Vector2(150, 30), TextAnchor.MiddleLeft);
        
        // Setup Money Text  
        SetupTextElement("Money_Text", new Vector2(10, -50), new Vector2(150, 30), TextAnchor.MiddleLeft);
        
        // Setup Score Text
        SetupTextElement("Score_Text", new Vector2(10, -90), new Vector2(150, 30), TextAnchor.MiddleLeft);
        
        // Setup Wave Info Text
        SetupTextElement("Wave_Info_Text", new Vector2(10, -130), new Vector2(200, 30), TextAnchor.MiddleLeft);
        
        // Setup Tower Panel (right side)
        SetupPanel("Tower_Panel", new Vector2(-200, 0), new Vector2(180, 400));
        
        // Setup Tower Buttons
        SetupButton("Basic_Tower_Button", new Vector2(-190, -50), new Vector2(80, 60));
        SetupButton("Cannon_Tower_Button", new Vector2(-100, -50), new Vector2(80, 60));
        SetupButton("Laser_Tower_Button", new Vector2(-190, -120), new Vector2(80, 60));
        
        // Setup Tower Labels
        SetupLabel("Basic_Tower_Label", new Vector2(-150, -30), new Vector2(60, 20), "Basic\\n$100");
        SetupLabel("Cannon_Tower_Label", new Vector2(-60, -30), new Vector2(60, 20), "Cannon\\n$150");
        SetupLabel("Laser_Tower_Label", new Vector2(-150, -100), new Vector2(60, 20), "Laser\\n$200");
        
        // Setup Upgrade Menu (center-right)
        SetupPanel("Upgrade_Menu", new Vector2(-300, 100), new Vector2(200, 150));
        
        // Setup Upgrade Buttons
        SetupButton("Upgrade_Damage_Button", new Vector2(-350, 120), new Vector2(80, 40));
        SetupButton("Upgrade_Range_Button", new Vector2(-250, 120), new Vector2(80, 40));
        SetupButton("Sell_Tower_Button", new Vector2(-300, 80), new Vector2(100, 30));
        
        // Setup Upgrade Labels
        SetupLabel("Upgrade_Damage_Label", new Vector2(-310, 140), new Vector2(60, 20), "Damage");
        SetupLabel("Upgrade_Range_Label", new Vector2(-210, 140), new Vector2(60, 20), "Range");
        SetupLabel("Sell_Tower_Label", new Vector2(-250, 100), new Vector2(60, 20), "Sell");
        
        // Setup HUD Panel (top bar)
        SetupPanel("HUD_Panel", new Vector2(0, -25), new Vector2(1920, 50));
        
        // Hide upgrade menu initially
        GameObject upgradeMenu = GameObject.Find("Upgrade_Menu");
        if (upgradeMenu != null)
        {
            upgradeMenu.SetActive(false);
        }
    }
    
    void SetupTextElement(string objectName, Vector2 position, Vector2 size, TextAnchor alignment)
    {
        GameObject textObj = GameObject.Find(objectName);
        if (textObj != null)
        {
            RectTransform rectTransform = textObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Set anchor to top-left
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.anchoredPosition = position;
                rectTransform.sizeDelta = size;
            }
            
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.fontSize = 16;
                text.color = Color.white;
                text.alignment = TextAlignmentOptions.Left;
            }
        }
    }
    
    void SetupPanel(string objectName, Vector2 position, Vector2 size)
    {
        GameObject panelObj = GameObject.Find(objectName);
        if (panelObj != null)
        {
            RectTransform rectTransform = panelObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                if (objectName == "HUD_Panel")
                {
                    // Top bar - anchor to top
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                }
                else
                {
                    // Right side panels - anchor to right
                    rectTransform.anchorMin = new Vector2(1, 0.5f);
                    rectTransform.anchorMax = new Vector2(1, 0.5f);
                }
                rectTransform.anchoredPosition = position;
                rectTransform.sizeDelta = size;
            }
            
            Image image = panelObj.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black
            }
        }
    }
    
    void SetupButton(string objectName, Vector2 position, Vector2 size)
    {
        GameObject buttonObj = GameObject.Find(objectName);
        if (buttonObj != null)
        {
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(1, 0.5f);
                rectTransform.anchorMax = new Vector2(1, 0.5f);
                rectTransform.anchoredPosition = position;
                rectTransform.sizeDelta = size;
            }
            
            Image image = buttonObj.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(0.2f, 0.3f, 0.8f, 0.8f); // Blue button
            }
        }
    }
    
    void SetupLabel(string objectName, Vector2 position, Vector2 size, string text)
    {
        GameObject labelObj = GameObject.Find(objectName);
        if (labelObj != null)
        {
            RectTransform rectTransform = labelObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(1, 0.5f);
                rectTransform.anchorMax = new Vector2(1, 0.5f);
                rectTransform.anchoredPosition = position;
                rectTransform.sizeDelta = size;
            }
            
            TextMeshProUGUI textComponent = labelObj.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = text;
                textComponent.fontSize = 12;
                textComponent.color = Color.white;
                textComponent.alignment = TextAlignmentOptions.Center;
            }
        }
    }
    
    // Public method to show/hide upgrade menu
    public void ShowUpgradeMenu(bool show)
    {
        GameObject upgradeMenu = GameObject.Find("Upgrade_Menu");
        if (upgradeMenu != null)
        {
            upgradeMenu.SetActive(show);
        }
    }
    
    // Update UI text values
    void Update()
    {
        if (GameManager.Instance != null)
        {
            UpdateText("Health_Text", "Health: " + GameManager.Instance.currentHealth);
            UpdateText("Money_Text", "Money: $" + GameManager.Instance.currentMoney);
            UpdateText("Score_Text", "Score: " + GameManager.Instance.currentScore);
            UpdateText("Wave_Info_Text", "Wave: " + GameManager.Instance.currentWave);
        }
    }
    
    void UpdateText(string objectName, string text)
    {
        GameObject textObj = GameObject.Find(objectName);
        if (textObj != null)
        {
            TextMeshProUGUI textComponent = textObj.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = text;
            }
        }
    }
}