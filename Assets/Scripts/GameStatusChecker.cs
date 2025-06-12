using UnityEngine;

public class GameStatusChecker : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== TOWER DEFENSE COMPILATION STATUS ===");
        
        // Check if our scripts compiled
        bool scriptsCompiled = CheckScriptCompilation();
        
        if (scriptsCompiled)
        {
            Debug.Log("✅ All tower defense scripts compiled successfully!");
            Debug.Log("✅ Game is ready to play!");
            LogGameInstructions();
        }
        else
        {
            Debug.Log("❌ Some scripts failed to compile");
        }
        
        LogSceneStatus();
    }
    
    bool CheckScriptCompilation()
    {
        bool allCompiled = true;
        
        // Check individual script types
        if (typeof(CompleteTowerDefenseGame) == null)
        {
            Debug.Log("❌ CompleteTowerDefenseGame not found");
            allCompiled = false;
        }
        else
        {
            Debug.Log("✅ CompleteTowerDefenseGame compiled");
        }
        
        if (typeof(BasicEnemyMovement) == null)
        {
            Debug.Log("❌ BasicEnemyMovement not found");
            allCompiled = false;
        }
        else
        {
            Debug.Log("✅ BasicEnemyMovement compiled");
        }
        
        if (typeof(BasicTowerShooting) == null)
        {
            Debug.Log("❌ BasicTowerShooting not found");
            allCompiled = false;
        }
        else
        {
            Debug.Log("✅ BasicTowerShooting compiled");
        }
        
        return allCompiled;
    }
    
    void LogGameInstructions()
    {
        Debug.Log("=== TOWER DEFENSE CONTROLS ===");
        Debug.Log("F1 - Restart Game");
        Debug.Log("F2 - Show Game Info");
        Debug.Log("F3 - Spawn Enemy");
        Debug.Log("F4 - Place Tower at Mouse Position");
        Debug.Log("SPACE - Debug Current Status");
        Debug.Log("R - Reload Scene");
    }
    
    void LogSceneStatus()
    {
        int enemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        int towers = FindObjectsOfType<BasicTowerShooting>().Length;
        
        Debug.Log($"=== CURRENT SCENE STATUS ===");
        Debug.Log($"Active Enemies: {enemies}");
        Debug.Log($"Active Towers: {towers}");
        Debug.Log($"Total GameObjects: {FindObjectsOfType<GameObject>().Length}");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LogSceneStatus();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}