using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string name;
        public GameObject enemyPrefab;
        public int count;
        public float timeBetweenSpawns;
    }
    
    [Header("Wave Settings")]
    public Wave[] waves;
    public Transform spawnPoint;
    public float timeBetweenWaves = 5f;
    
    private int currentWaveIndex = 0;
    private bool spawningWave = false;
    private bool allWavesComplete = false;
    
    void Start()
    {
        if (spawnPoint == null)
        {
            PathManager pathManager = FindObjectOfType<PathManager>();
            if (pathManager != null && pathManager.GetWaypoints().Length > 0)
            {
                spawnPoint = pathManager.GetWaypoints()[0];
            }
            else
            {
                spawnPoint = transform;
            }
        }
        
        StartCoroutine(SpawnWaves());
    }
    
    IEnumerator SpawnWaves()
    {
        while (currentWaveIndex < waves.Length && GameManager.Instance != null && GameManager.Instance.gameActive)
        {
            yield return new WaitForSeconds(timeBetweenWaves);
            
            if (!spawningWave)
            {
                StartCoroutine(SpawnWave(waves[currentWaveIndex]));
            }
            
            yield return new WaitUntil(() => !spawningWave);
            
            currentWaveIndex++;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NextWave();
            }
        }
        
        allWavesComplete = true;
    }
    
    IEnumerator SpawnWave(Wave wave)
    {
        spawningWave = true;
        
        for (int i = 0; i < wave.count; i++)
        {
            if (GameManager.Instance == null || !GameManager.Instance.gameActive) break;
            
            SpawnEnemy(wave.enemyPrefab);
            yield return new WaitForSeconds(wave.timeBetweenSpawns);
        }
        
        spawningWave = false;
    }
    
    void SpawnEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab != null && spawnPoint != null)
        {
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            
            PathManager pathManager = FindObjectOfType<PathManager>();
            if (pathManager != null)
            {
                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.waypoints = pathManager.GetWaypoints();
                }
            }
        }
    }
    
    public bool IsWaveActive()
    {
        return spawningWave || GameObject.FindGameObjectsWithTag("Enemy").Length > 0;
    }
    
    public bool AllWavesComplete()
    {
        return allWavesComplete;
    }
}