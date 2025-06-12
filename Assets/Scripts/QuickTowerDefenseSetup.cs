using UnityEngine;

public class QuickTowerDefenseSetup : MonoBehaviour
{
    public bool autoRun = true;
    
    void Start()
    {
        if (autoRun)
        {
            Invoke("SetupGame", 0.5f);
        }
    }
    
    void SetupGame()
    {
        Debug.Log("Setting up Tower Defense Game...");
        
        // Clear existing demo objects
        CleanupExisting();
        
        // Create game elements
        CreatePath();
        CreateEnemies();
        CreateTowers();
        
        Debug.Log("Tower Defense Game Setup Complete!");
    }
    
    void CleanupExisting()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy.name.StartsWith("Demo"))
                DestroyImmediate(enemy);
        }
    }
    
    void CreatePath()
    {
        Vector3[] pathPoints = {
            new Vector3(-8, 0, 0),
            new Vector3(-2, 0, 0),
            new Vector3(-2, 0, 4),
            new Vector3(2, 0, 4),
            new Vector3(8, 0, 4)
        };
        
        // Create visual path
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            CreatePathSegment(pathPoints[i], pathPoints[i + 1]);
        }
    }
    
    void CreatePathSegment(Vector3 start, Vector3 end)
    {
        GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
        segment.name = "PathSegment";
        
        Vector3 center = (start + end) / 2f;
        float distance = Vector3.Distance(start, end);
        
        segment.transform.position = center;
        segment.transform.LookAt(end);
        segment.transform.localScale = new Vector3(0.3f, 0.05f, distance);
        
        segment.GetComponent<Renderer>().material.color = Color.yellow;
        DestroyImmediate(segment.GetComponent<BoxCollider>());
    }
    
    void CreateEnemies()
    {
        Vector3 spawnPoint = new Vector3(-8, 0, 0);
        
        for (int i = 0; i < 3; i++)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "DemoEnemy_" + i;
            enemy.tag = "Enemy";
            enemy.transform.position = spawnPoint + Vector3.left * (i * 2f);
            enemy.GetComponent<Renderer>().material.color = Color.red;
            
            // Add basic movement
            BasicEnemyMovement movement = enemy.AddComponent<BasicEnemyMovement>();
        }
    }
    
    void CreateTowers()
    {
        Vector3[] positions = {
            new Vector3(-1, 0.5f, 2),
            new Vector3(1, 0.5f, 2),
            new Vector3(0, 0.5f, -1)
        };
        
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tower.name = "DemoTower_" + i;
            tower.transform.position = positions[i];
            tower.transform.localScale = Vector3.one * 1.2f;
            tower.GetComponent<Renderer>().material.color = Color.blue;
            
            // Add basic shooting
            BasicTowerShooting shooting = tower.AddComponent<BasicTowerShooting>();
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetupGame();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("Enemies: " + GameObject.FindGameObjectsWithTag("Enemy").Length);
            Debug.Log("Towers: " + FindObjectsOfType<BasicTowerShooting>().Length);
        }
    }
}