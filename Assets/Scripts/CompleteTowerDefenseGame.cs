using UnityEngine;
using System.Collections.Generic;

public class CompleteTowerDefenseGame : MonoBehaviour
{
    [Header("Game Settings")]
    public bool autoStart = true;
    public int playerHealth = 100;
    public int playerMoney = 500;
    public int currentScore = 0;
    
    [Header("Prefab References - Leave null for auto-creation")]
    public GameObject enemyPrefab;
    public GameObject towerPrefab;
    
    private List<GameObject> enemies = new List<GameObject>();
    private List<GameObject> towers = new List<GameObject>();
    private Vector3[] pathPoints;
    private bool gameActive = true;
    
    void Start()
    {
        if (autoStart)
        {
            StartGame();
        }
    }
    
    public void StartGame()
    {
        Debug.Log("Starting Tower Defense Game...");
        
        SetupGameEnvironment();
        CreatePath();
        SpawnInitialEnemies();
        PlaceInitialTowers();
        
        InvokeRepeating("SpawnWave", 5f, 10f);
        
        Debug.Log("Tower Defense Game Started! Controls: F1=Restart, F2=Info, F3=Spawn Enemy");
    }
    
    void SetupGameEnvironment()
    {
        // Create main camera if not exists
        if (Camera.main == null)
        {
            GameObject cam = new GameObject("Main Camera");
            cam.AddComponent<Camera>();
            cam.AddComponent<AudioListener>();
            cam.transform.position = new Vector3(0, 10, -10);
            cam.transform.rotation = Quaternion.Euler(30, 0, 0);
        }
        
        // Ensure proper lighting
        if (FindObjectOfType<Light>() == null)
        {
            GameObject light = new GameObject("Directional Light");
            Light lightComp = light.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
    }
    
    void CreatePath()
    {
        pathPoints = new Vector3[]
        {
            new Vector3(-10, 0, 0),  // Start
            new Vector3(-5, 0, 0),   // Mid 1
            new Vector3(-5, 0, 5),   // Turn 1
            new Vector3(0, 0, 5),    // Mid 2
            new Vector3(0, 0, -5),   // Turn 2
            new Vector3(5, 0, -5),   // Mid 3
            new Vector3(5, 0, 0),    // Turn 3
            new Vector3(10, 0, 0)    // End
        };
        
        // Visual path
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            CreatePathVisual(pathPoints[i], pathPoints[i + 1]);
        }
        
        // Start and end markers
        CreateMarker(pathPoints[0], "START", Color.green);
        CreateMarker(pathPoints[pathPoints.Length - 1], "END", Color.red);
    }
    
    void CreatePathVisual(Vector3 start, Vector3 end)
    {
        GameObject path = GameObject.CreatePrimitive(PrimitiveType.Cube);
        path.name = "PathSegment";
        
        Vector3 center = (start + end) / 2f;
        float distance = Vector3.Distance(start, end);
        
        path.transform.position = center;
        path.transform.LookAt(end);
        path.transform.localScale = new Vector3(0.5f, 0.1f, distance);
        
        Renderer renderer = path.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.yellow;
        renderer.material = mat;
        
        // Remove collider to avoid interference
        Destroy(path.GetComponent<BoxCollider>());
    }
    
    void CreateMarker(Vector3 position, string text, Color color)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = text + "_Marker";
        marker.transform.position = position + Vector3.up * 0.5f;
        marker.transform.localScale = Vector3.one * 0.5f;
        
        Renderer renderer = marker.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        renderer.material = mat;
        
        Destroy(marker.GetComponent<SphereCollider>());
    }
    
    void SpawnInitialEnemies()
    {
        for (int i = 0; i < 3; i++)
        {
            SpawnEnemy();
        }
    }
    
    void SpawnEnemy()
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy_" + System.DateTime.Now.Ticks;
        enemy.tag = "Enemy";
        enemy.transform.position = pathPoints[0] + Vector3.left * Random.Range(0, 3f);
        
        // Visual
        Renderer renderer = enemy.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.red;
        renderer.material = mat;
        
        // Add movement component
        EnemyMovement movement = enemy.AddComponent<EnemyMovement>();
        movement.Initialize(pathPoints, this);
        
        enemies.Add(enemy);
    }
    
    void PlaceInitialTowers()
    {
        Vector3[] towerPositions = {
            new Vector3(-2, 0.5f, 2),
            new Vector3(2, 0.5f, 2),
            new Vector3(-2, 0.5f, -2),
            new Vector3(2, 0.5f, -2)
        };
        
        foreach (Vector3 pos in towerPositions)
        {
            CreateTower(pos);
        }
    }
    
    void CreateTower(Vector3 position)
    {
        GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tower.name = "Tower_" + towers.Count;
        tower.transform.position = position;
        tower.transform.localScale = new Vector3(1, 1.5f, 1);
        
        // Visual
        Renderer renderer = tower.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.blue;
        renderer.material = mat;
        
        // Add shooting component
        TowerShooting shooting = tower.AddComponent<TowerShooting>();
        shooting.Initialize(this);
        
        towers.Add(tower);
    }
    
    void SpawnWave()
    {
        if (!gameActive) return;
        
        int waveSize = Random.Range(3, 8);
        for (int i = 0; i < waveSize; i++)
        {
            Invoke("SpawnEnemy", i * 0.5f);
        }
        
        Debug.Log($"Spawning wave of {waveSize} enemies!");
    }
    
    public void EnemyDied(GameObject enemy)
    {
        enemies.Remove(enemy);
        playerMoney += 25;
        currentScore += 10;
        
        Debug.Log($"Enemy killed! Money: ${playerMoney}, Score: {currentScore}");
    }
    
    public void EnemyReachedEnd(GameObject enemy)
    {
        enemies.Remove(enemy);
        playerHealth -= 10;
        
        Debug.Log($"Enemy reached end! Health: {playerHealth}");
        
        if (playerHealth <= 0)
        {
            GameOver();
        }
    }
    
    void GameOver()
    {
        gameActive = false;
        CancelInvoke();
        Debug.Log($"GAME OVER! Final Score: {currentScore}");
    }
    
    public List<GameObject> GetNearbyEnemies(Vector3 position, float range)
    {
        List<GameObject> nearby = new List<GameObject>();
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && Vector3.Distance(position, enemy.transform.position) <= range)
            {
                nearby.Add(enemy);
            }
        }
        
        return nearby;
    }
    
    void Update()
    {
        // Controls
        if (Input.GetKeyDown(KeyCode.F1))
        {
            RestartGame();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ShowGameInfo();
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            SpawnEnemy();
        }
        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10));
            worldPos.y = 0.5f;
            CreateTower(worldPos);
        }
        
        // Clean up null references
        enemies.RemoveAll(e => e == null);
        towers.RemoveAll(t => t == null);
    }
    
    void RestartGame()
    {
        // Clean up
        foreach (GameObject enemy in enemies)
            if (enemy != null) Destroy(enemy);
        foreach (GameObject tower in towers)
            if (tower != null) Destroy(tower);
            
        enemies.Clear();
        towers.Clear();
        
        // Reset game state
        playerHealth = 100;
        playerMoney = 500;
        currentScore = 0;
        gameActive = true;
        
        // Restart
        StartGame();
    }
    
    void ShowGameInfo()
    {
        Debug.Log($"=== TOWER DEFENSE STATUS ===");
        Debug.Log($"Health: {playerHealth}");
        Debug.Log($"Money: ${playerMoney}");
        Debug.Log($"Score: {currentScore}");
        Debug.Log($"Active Enemies: {enemies.Count}");
        Debug.Log($"Active Towers: {towers.Count}");
        Debug.Log($"Game Active: {gameActive}");
        Debug.Log($"Controls: F1=Restart, F2=Info, F3=Spawn Enemy, F4=Place Tower");
    }
}

// Enemy movement component
public class EnemyMovement : MonoBehaviour
{
    private Vector3[] waypoints;
    private int currentWaypoint = 0;
    private float speed = 2f;
    private float health = 100f;
    private CompleteTowerDefenseGame gameManager;
    
    public void Initialize(Vector3[] path, CompleteTowerDefenseGame manager)
    {
        waypoints = path;
        gameManager = manager;
        speed = Random.Range(1.5f, 3f);
        gameObject.tag = "Enemy";
    }
    
    void Update()
    {
        if (waypoints == null || currentWaypoint >= waypoints.Length)
        {
            ReachedEnd();
            return;
        }
        
        Vector3 target = waypoints[currentWaypoint];
        Vector3 direction = (target - transform.position).normalized;
        
        transform.position += direction * speed * Time.deltaTime;
        
        if (direction != Vector3.zero)
            transform.LookAt(target);
        
        if (Vector3.Distance(transform.position, target) < 0.5f)
        {
            currentWaypoint++;
        }
    }
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        
        // Visual feedback
        Renderer renderer = GetComponent<Renderer>();
        Color newColor = Color.Lerp(Color.red, Color.black, 1f - (health / 100f));
        renderer.material.color = newColor;
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        if (gameManager != null)
            gameManager.EnemyDied(gameObject);
        Destroy(gameObject);
    }
    
    void ReachedEnd()
    {
        if (gameManager != null)
            gameManager.EnemyReachedEnd(gameObject);
        Destroy(gameObject);
    }
}

// Tower shooting component
public class TowerShooting : MonoBehaviour
{
    private float range = 5f;
    private float damage = 35f;
    private float fireRate = 1.5f;
    private float nextFireTime = 0f;
    private CompleteTowerDefenseGame gameManager;
    
    public void Initialize(CompleteTowerDefenseGame manager)
    {
        gameManager = manager;
        range = Random.Range(4f, 6f);
        fireRate = Random.Range(1f, 2f);
    }
    
    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            GameObject target = FindTarget();
            if (target != null)
            {
                Shoot(target);
                nextFireTime = Time.time + (1f / fireRate);
            }
        }
    }
    
    GameObject FindTarget()
    {
        if (gameManager == null) return null;
        
        List<GameObject> enemies = gameManager.GetNearbyEnemies(transform.position, range);
        
        if (enemies.Count == 0) return null;
        
        // Find closest enemy
        GameObject closest = null;
        float closestDistance = range + 1f;
        
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }
        
        return closest;
    }
    
    void Shoot(GameObject target)
    {
        // Rotate towards target
        Vector3 direction = (target.transform.position - transform.position).normalized;
        if (direction != Vector3.zero)
            transform.LookAt(target.transform.position);
        
        // Visual effect
        Debug.DrawLine(transform.position + Vector3.up, target.transform.position, Color.yellow, 0.2f);
        
        // Deal damage
        EnemyMovement enemy = target.GetComponent<EnemyMovement>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
        
        // Visual feedback
        StartCoroutine(MuzzleFlash());
    }
    
    System.Collections.IEnumerator MuzzleFlash()
    {
        Renderer renderer = GetComponent<Renderer>();
        Color original = renderer.material.color;
        renderer.material.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        renderer.material.color = original;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}