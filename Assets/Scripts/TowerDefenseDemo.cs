using UnityEngine;

public class TowerDefenseDemo : MonoBehaviour
{
    void Start()
    {
        CreateCompleteDemo();
    }
    
    void CreateCompleteDemo()
    {
        CreateGameCore();
        CreateEnemyPath();
        CreateSimpleEnemies();
        CreateSimpleTowers();
        SetupUI();
    }
    
    void CreateGameCore()
    {
        GameObject gm = GameObject.Find("GameManager");
        if (gm != null && gm.GetComponent<GameManager>() == null)
        {
            gm.AddComponent<GameManager>();
        }
    }
    
    void CreateEnemyPath()
    {
        GameObject pathParent = new GameObject("EnemyPath");
        Transform[] waypoints = new Transform[5];
        Vector3[] positions = {
            new Vector3(-8, 0, 0),   // Start
            new Vector3(-3, 0, 0),   // Mid 1
            new Vector3(-3, 0, 4),   // Turn
            new Vector3(3, 0, 4),    // Mid 2  
            new Vector3(8, 0, 4)     // End
        };
        
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject wp = new GameObject("Waypoint_" + i);
            wp.transform.position = positions[i];
            wp.transform.SetParent(pathParent.transform);
            waypoints[i] = wp.transform;
        }
        
        // Create path visual
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            DrawPathSegment(waypoints[i].position, waypoints[i + 1].position);
        }
    }
    
    void DrawPathSegment(Vector3 start, Vector3 end)
    {
        GameObject pathSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pathSegment.name = "PathSegment";
        
        Vector3 center = (start + end) / 2f;
        float distance = Vector3.Distance(start, end);
        
        pathSegment.transform.position = center;
        pathSegment.transform.LookAt(end);
        pathSegment.transform.localScale = new Vector3(0.5f, 0.1f, distance);
        
        Renderer renderer = pathSegment.GetComponent<Renderer>();
        renderer.material.color = Color.yellow;
        
        Destroy(pathSegment.GetComponent<BoxCollider>());
    }
    
    void CreateSimpleEnemies()
    {
        Transform[] waypoints = FindWaypoints();
        
        for (int i = 0; i < 5; i++)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "DemoEnemy_" + i;
            enemy.tag = "Enemy";
            
            Vector3 spawnPos = waypoints[0].position + Vector3.left * (i * 2f);
            enemy.transform.position = spawnPos;
            
            // Make enemy red
            Renderer renderer = enemy.GetComponent<Renderer>();
            renderer.material.color = Color.red;
            
            // Add movement script
            SimpleEnemyMover mover = enemy.AddComponent<SimpleEnemyMover>();
            mover.waypoints = waypoints;
            mover.health = 100f;
            mover.speed = 2f + (i * 0.5f); // Varying speeds
        }
    }
    
    void CreateSimpleTowers()
    {
        Vector3[] towerPositions = {
            new Vector3(-1, 0.5f, 2),
            new Vector3(1, 0.5f, 2), 
            new Vector3(-1, 0.5f, -2),
            new Vector3(1, 0.5f, 6)
        };
        
        for (int i = 0; i < towerPositions.Length; i++)
        {
            GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tower.name = "DemoTower_" + i;
            tower.transform.position = towerPositions[i];
            tower.transform.localScale = new Vector3(1, 1.5f, 1);
            
            // Make tower blue
            Renderer renderer = tower.GetComponent<Renderer>();
            renderer.material.color = Color.blue;
            
            // Add shooting script
            SimpleTowerShooter shooter = tower.AddComponent<SimpleTowerShooter>();
            shooter.range = 4f;
            shooter.damage = 25f;
            shooter.fireRate = 1f + (i * 0.3f); // Varying fire rates
        }
    }
    
    Transform[] FindWaypoints()
    {
        GameObject[] waypoints = GameObject.FindGameObjectsWithTag("Untagged");
        Transform[] result = new Transform[5];
        int found = 0;
        
        foreach (GameObject obj in waypoints)
        {
            if (obj.name.StartsWith("Waypoint_") && found < 5)
            {
                result[found] = obj.transform;
                found++;
            }
        }
        
        return result;
    }
    
    void SetupUI()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            GameObject infoText = new GameObject("GameInfo");
            infoText.transform.SetParent(canvas.transform);
            
            UnityEngine.UI.Text text = infoText.AddComponent<UnityEngine.UI.Text>();
            text.text = "TOWER DEFENSE DEMO\n\nRed enemies follow yellow path\nBlue towers shoot at enemies\n\nPress R to restart\nPress SPACE for debug info";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.color = Color.white;
            
            RectTransform rect = infoText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -20);
            rect.sizeDelta = new Vector2(400, 120);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            int enemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
            int towers = FindObjectsOfType<SimpleTowerShooter>().Length;
            Debug.Log($"Active Enemies: {enemies}, Active Towers: {towers}");
        }
    }
}