using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerDefenseUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenu;
    public GameObject gameUI;
    public GameObject pauseMenu;
    public GameObject gameOverPanel;
    public GameObject upgradePanel;
    
    [Header("Game Status UI")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI scoreText;
    public Slider healthBar;
    
    [Header("Tower Selection UI")]
    public Button[] towerButtons;
    public TextMeshProUGUI[] towerCostTexts;
    
    [Header("Upgrade UI")]
    public TextMeshProUGUI towerLevelText;
    public TextMeshProUGUI upgradeCostText;
    public Button upgradeButton;
    public Button sellButton;
    
    [Header("Game Control")]
    public Button pauseButton;
    public Button playButton;
    public Button fastForwardButton;
    
    private bool isPaused = false;
    private bool gameStarted = false;
    private Tower selectedTower;
    
    void Start()
    {
        SetupUI();
    }
    
    void Update()
    {
        UpdateGameUI();
        HandleInput();
    }
    
    void SetupUI()
    {
        if (mainMenu != null) mainMenu.SetActive(true);
        if (gameUI != null) gameUI.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (upgradePanel != null) upgradePanel.SetActive(false);
        
        SetupTowerButtons();
    }
    
    void SetupTowerButtons()
    {
        TowerPlacement placement = FindObjectOfType<TowerPlacement>();
        if (placement != null && towerButtons != null)
        {
            for (int i = 0; i < towerButtons.Length && i < placement.towerPrefabs.Length; i++)
            {
                int index = i;
                towerButtons[i].onClick.AddListener(() => SelectTowerType(index));
                
                Tower towerScript = placement.towerPrefabs[i].GetComponent<Tower>();
                if (towerScript != null && towerCostTexts != null && i < towerCostTexts.Length)
                {
                    towerCostTexts[i].text = "$" + towerScript.cost;
                }
            }
        }
    }
    
    void UpdateGameUI()
    {
        if (!gameStarted || GameManager.Instance == null) return;
        
        if (healthText != null)
            healthText.text = "Health: " + GameManager.Instance.currentHealth;
        
        if (moneyText != null)
            moneyText.text = "Money: $" + GameManager.Instance.currentMoney;
        
        if (waveText != null)
            waveText.text = "Wave: " + GameManager.Instance.currentWave;
        
        if (scoreText != null)
            scoreText.text = "Score: " + GameManager.Instance.currentScore;
        
        if (healthBar != null)
            healthBar.value = (float)GameManager.Instance.currentHealth / GameManager.Instance.startingHealth;
        
        if (!GameManager.Instance.gameActive && gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleSpeed();
        }
    }
    
    public void StartGame()
    {
        gameStarted = true;
        if (mainMenu != null) mainMenu.SetActive(false);
        if (gameUI != null) gameUI.SetActive(true);
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(isPaused);
        }
    }
    
    public void ToggleSpeed()
    {
        if (Time.timeScale == 1f)
        {
            Time.timeScale = 2f;
        }
        else if (Time.timeScale == 2f)
        {
            Time.timeScale = 1f;
        }
    }
    
    public void SelectTowerType(int towerIndex)
    {
        TowerPlacement placement = FindObjectOfType<TowerPlacement>();
        if (placement != null)
        {
            placement.SendMessage("SelectTower", towerIndex, SendMessageOptions.DontRequireReceiver);
        }
    }
    
    public void ShowUpgradePanel(Tower tower)
    {
        selectedTower = tower;
        
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
            
            if (towerLevelText != null)
                towerLevelText.text = "Level: " + tower.currentLevel;
            
            if (upgradeCostText != null)
                upgradeCostText.text = "Upgrade: $" + tower.upgradeCost;
            
            if (upgradeButton != null)
                upgradeButton.interactable = tower.CanUpgrade();
        }
    }
    
    public void HideUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
        selectedTower = null;
    }
    
    public void UpgradeTower()
    {
        if (selectedTower != null)
        {
            selectedTower.Upgrade();
            ShowUpgradePanel(selectedTower);
        }
    }
    
    public void SellTower()
    {
        if (selectedTower != null)
        {
            int sellValue = selectedTower.cost / 2;
            GameManager.Instance.AddMoney(sellValue);
            Destroy(selectedTower.gameObject);
            HideUpgradePanel();
        }
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}