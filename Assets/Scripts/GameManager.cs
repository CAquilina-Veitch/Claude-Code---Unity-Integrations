using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int startingHealth = 100;
    public int startingMoney = 500;
    public int currentWave = 0;
    
    [Header("UI References")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI scoreText;
    
    [Header("Game Status")]
    public int currentHealth;
    public int currentMoney;
    public int currentScore;
    public bool gameActive = true;
    
    public static GameManager Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        currentHealth = startingHealth;
        currentMoney = startingMoney;
        currentScore = 0;
        
        // Fix UI layout after a short delay
        Invoke("FixUILayout", 0.1f);
        
        UpdateUI();
    }
    
    void FixUILayout()
    {
        Debug.Log("Fixing UI Layout...");
        
        // Fix Health Text (top-left)
        FixTextElement("Health_Text", new Vector2(20, -20), new Vector2(200, 30));
        
        // Fix Money Text
        FixTextElement("Money_Text", new Vector2(20, -60), new Vector2(200, 30));
        
        // Fix Score Text
        FixTextElement("Score_Text", new Vector2(20, -100), new Vector2(200, 30));
        
        // Fix Wave Text
        FixTextElement("Wave_Info_Text", new Vector2(20, -140), new Vector2(200, 30));
        
        // Fix Tower Panel (right side)
        FixUIPanel("Tower_Panel", new Vector2(-200, 0), new Vector2(200, 600), true);
        
        // Fix HUD Panel (top)
        FixUIPanel("HUD_Panel", new Vector2(0, -30), new Vector2(0, 60), false);
        
        // Fix Tower Buttons
        FixButton("Basic_Tower_Button", new Vector2(-180, -100), new Vector2(80, 60));
        FixButton("Cannon_Tower_Button", new Vector2(-90, -100), new Vector2(80, 60));
        FixButton("Laser_Tower_Button", new Vector2(-180, -180), new Vector2(80, 60));
        
        // Update button labels
        UpdateButtonLabel("Basic_Tower_Label", "Basic\n$100");
        UpdateButtonLabel("Cannon_Tower_Label", "Cannon\n$150");
        UpdateButtonLabel("Laser_Tower_Label", "Laser\n$200");
        
        Debug.Log("UI Layout Fixed!");
    }
    
    void FixTextElement(string objName, Vector2 position, Vector2 size)
    {
        GameObject obj = GameObject.Find(objName);
        if (obj != null)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.anchoredPosition = position;
                rect.sizeDelta = size;
            }
            
            TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.fontSize = 18;
                text.color = Color.white;
            }
        }
    }
    
    void FixUIPanel(string objName, Vector2 position, Vector2 size, bool rightSide)
    {
        GameObject obj = GameObject.Find(objName);
        if (obj != null)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect != null)
            {
                if (rightSide)
                {
                    rect.anchorMin = new Vector2(1, 0.5f);
                    rect.anchorMax = new Vector2(1, 0.5f);
                }
                else
                {
                    rect.anchorMin = new Vector2(0, 1);
                    rect.anchorMax = new Vector2(1, 1);
                }
                rect.anchoredPosition = position;
                rect.sizeDelta = size;
            }
            
            UnityEngine.UI.Image img = obj.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                img.color = new Color(0, 0, 0, 0.3f);
            }
        }
    }
    
    void FixButton(string objName, Vector2 position, Vector2 size)
    {
        GameObject obj = GameObject.Find(objName);
        if (obj != null)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(1, 0.5f);
                rect.anchorMax = new Vector2(1, 0.5f);
                rect.anchoredPosition = position;
                rect.sizeDelta = size;
            }
            
            UnityEngine.UI.Image img = obj.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                img.color = new Color(0.2f, 0.4f, 0.8f, 0.8f);
            }
        }
    }
    
    void UpdateButtonLabel(string objName, string text)
    {
        GameObject obj = GameObject.Find(objName);
        if (obj != null)
        {
            TextMeshProUGUI textComp = obj.GetComponent<TextMeshProUGUI>();
            if (textComp != null)
            {
                textComp.text = text;
                textComp.fontSize = 14;
                textComp.color = Color.white;
            }
        }
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            GameOver();
        }
        UpdateUI();
    }
    
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateUI();
    }
    
    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateUI();
    }
    
    public void NextWave()
    {
        currentWave++;
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (healthText) healthText.text = "Health: " + currentHealth;
        if (moneyText) moneyText.text = "Money: $" + currentMoney;
        if (waveText) waveText.text = "Wave: " + currentWave;
        if (scoreText) scoreText.text = "Score: " + currentScore;
    }
    
    void GameOver()
    {
        gameActive = false;
        Debug.Log("Game Over!");
    }
}