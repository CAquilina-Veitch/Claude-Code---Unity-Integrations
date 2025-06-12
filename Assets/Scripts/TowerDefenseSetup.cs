using UnityEngine;

public class TowerDefenseSetup : MonoBehaviour
{
    [Header("Basic Enemy Setup")]
    public GameObject enemyPrefab;
    public GameObject towerPrefab;
    public bool autoSetup = true;
    
    void Start()
    {
        if (autoSetup)
        {
            SetupBasicGame();
        }
    }
    
    void SetupBasicGame()
    {
        SetupGameManager();
        SetupPathManager();
        SetupWaveSpawner();
        SetupEnemyPrefabs();
        CreateBasicEnemies();
    }
    
    void SetupGameManager()
    {
        if (GameManager.Instance == null)
        {
            GameObject gmObj = GameObject.Find("GameManager");
            if (gmObj == null)
            {
                gmObj = new GameObject("GameManager");
            }
            
            if (gmObj.GetComponent<GameManager>() == null)
            {
                gmObj.AddComponent<GameManager>();
            }
        }
    }
    
    void SetupPathManager()
    {
        PathManager pathManager = FindObjectOfType<PathManager>();
        if (pathManager == null)
        {
            GameObject pathObj = GameObject.Find("PathManager");
            if (pathObj == null)
            {
                pathObj = new GameObject("PathManager");
            }
            pathManager = pathObj.AddComponent<PathManager>();
        }
    }
    
    void SetupWaveSpawner()
    {
        WaveSpawner spawner = FindObjectOfType<WaveSpawner>();
        if (spawner == null)
        {
            GameObject spawnerObj = GameObject.Find("WaveSpawner");
            if (spawnerObj == null)
            {
                spawnerObj = new GameObject("WaveSpawner");
            }
            spawner = spawnerObj.AddComponent<WaveSpawner>();
        }
        
        if (spawner.waves == null || spawner.waves.Length == 0)
        {
            CreateDefaultWaves(spawner);
        }
    }
    
    void CreateDefaultWaves(WaveSpawner spawner)
    {
        if (enemyPrefab == null)
        {
            enemyPrefab = CreateBasicEnemyPrefab();
        }
        
        spawner.waves = new WaveSpawner.Wave[3];
        
        spawner.waves[0] = new WaveSpawner.Wave()
        {
            name = "Wave 1",
            enemyPrefab = enemyPrefab,
            count = 5,
            timeBetweenSpawns = 1f
        };
        
        spawner.waves[1] = new WaveSpawner.Wave()
        {
            name = "Wave 2", 
            enemyPrefab = enemyPrefab,
            count = 8,
            timeBetweenSpawns = 0.8f
        };
        
        spawner.waves[2] = new WaveSpawner.Wave()
        {
            name = "Wave 3",
            enemyPrefab = enemyPrefab,
            count = 12,
            timeBetweenSpawns = 0.6f
        };
    }
    
    GameObject CreateBasicEnemyPrefab()
    {
        GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        prefab.name = "BasicEnemyPrefab";
        prefab.AddComponent<Enemy>();
        prefab.tag = "Enemy";
        
        Renderer renderer = prefab.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
        
        DontDestroyOnLoad(prefab);
        prefab.SetActive(false);
        
        return prefab;
    }
    
    void SetupEnemyPrefabs()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy.GetComponent<Collider>() == null)
            {
                enemy.AddComponent<CapsuleCollider>();
            }
            
            Renderer renderer = enemy.GetComponent<Renderer>();
            if (renderer != null && renderer.material.color == Color.white)
            {
                renderer.material.color = Color.red;
            }
        }
    }
    
    void CreateBasicEnemies()
    {
        PathManager pathManager = FindObjectOfType<PathManager>();
        if (pathManager != null && pathManager.GetWaypoints().Length > 0)
        {
            Vector3 spawnPos = pathManager.GetWaypoints()[0].position;
            
            for (int i = 0; i < 3; i++)
            {
                Vector3 pos = spawnPos + Vector3.right * i * 2f;
                GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = "TestEnemy" + i;
                enemy.transform.position = pos;
                enemy.tag = "Enemy";
                
                Enemy enemyScript = enemy.AddComponent<Enemy>();
                enemyScript.waypoints = pathManager.GetWaypoints();
                
                Renderer renderer = enemy.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }
            }
        }
    }
}