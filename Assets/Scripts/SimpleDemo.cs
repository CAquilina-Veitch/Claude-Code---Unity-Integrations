using UnityEngine;

public class SimpleDemo : MonoBehaviour
{
    void Start()
    {
        SetupDemo();
    }
    
    void SetupDemo()
    {
        CreateGameManager();
        CreatePathAndEnemies();
        CreateTowers();
        CreateUI();
    }
    
    void CreateGameManager()
    {
        GameObject gm = GameObject.Find("GameManager");
        if (gm == null)
        {
            gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }
    }
    
    void CreatePathAndEnemies()
    {
        GameObject[] waypoints = new GameObject[4];
        Vector3[] positions = {
            new Vector3(-5, 0, 0),
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 5),
            new Vector3(5, 0, 5)
        };
        
        for (int i = 0; i < 4; i++)
        {
            waypoints[i] = new GameObject("Waypoint" + i);
            waypoints[i].transform.position = positions[i];
        }
        
        for (int i = 0; i < 3; i++)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "Enemy" + i;
            enemy.transform.position = positions[0] + Vector3.left * i * 2;
            enemy.tag = "Enemy";
            enemy.GetComponent<Renderer>().material.color = Color.red;
            
            Enemy enemyScript = enemy.AddComponent<Enemy>();
            Transform[] enemyWaypoints = new Transform[4];
            for (int j = 0; j < 4; j++)
            {
                enemyWaypoints[j] = waypoints[j].transform;
            }
            enemyScript.waypoints = enemyWaypoints;
        }
    }
    
    void CreateTowers()
    {
        Vector3[] towerPositions = {
            new Vector3(-2, 0.5f, 2),
            new Vector3(2, 0.5f, 2),
            new Vector3(-2, 0.5f, -2)
        };
        
        for (int i = 0; i < 3; i++)
        {
            GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tower.name = "Tower" + i;
            tower.transform.position = towerPositions[i];
            tower.GetComponent<Renderer>().material.color = Color.blue;
            tower.AddComponent<Tower>();
        }
    }
    
    void CreateUI()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            GameObject statusText = new GameObject("StatusText");
            statusText.transform.SetParent(canvas.transform);
            
            UnityEngine.UI.Text text = statusText.AddComponent<UnityEngine.UI.Text>();
            text.text = "Tower Defense Game Running\nPress SPACE to debug\nRight-click towers to upgrade";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            
            RectTransform rect = statusText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(10, -10);
            rect.sizeDelta = new Vector2(300, 60);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Enemies: " + GameObject.FindGameObjectsWithTag("Enemy").Length);
            Debug.Log("Towers: " + FindObjectsOfType<Tower>().Length);
            if (GameManager.Instance != null)
            {
                Debug.Log("Health: " + GameManager.Instance.currentHealth + 
                         " Money: " + GameManager.Instance.currentMoney +
                         " Score: " + GameManager.Instance.currentScore);
            }
        }
    }
}