using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyWave
{
    public int enemyCount = 5;
    public float spawnDelay = 1f;
    public GameObject enemyPrefab;
    public float waveDelay = 5f;
}

public class EnemyController : MonoBehaviour
{
    [Header("Health System")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    [Header("Movement")]
    public Transform[] pathPoints;
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public bool loopPath = true;
    public bool reverseAtEnd = false;
    
    [Header("Wave Spawning")]
    public bool isWaveSpawner = false;
    public EnemyWave[] waves;
    public Transform[] spawnPoints;
    public bool autoStartWaves = true;
    public float timeBetweenWaves = 10f;

    [Header("Death Effects")]
    public GameObject deathEffect;
    public float deathDelay = 2f;
    public bool dropLoot = false;
    public GameObject[] lootPrefabs;

    [Header("Audio")]
    public AudioClip[] damageSounds;
    public AudioClip deathSound;
    private AudioSource audioSource;

    // Private variables
    private int currentPathIndex = 0;
    private bool movingForward = true;
    private Rigidbody rb;
    private Animator animator;
    private bool isMoving = true;
    
    // Wave spawning variables
    private int currentWaveIndex = 0;
    private bool spawningWave = false;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    // Events
    public System.Action<EnemyController> OnEnemyDeath;
    public System.Action<EnemyController, float> OnEnemyDamaged;

    void Start()
    {
        InitializeEnemy();
    }

    void InitializeEnemy()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Initialize path movement
        if (pathPoints != null && pathPoints.Length > 0)
        {
            transform.position = pathPoints[0].position;
        }

        // Start wave spawning if this is a wave spawner
        if (isWaveSpawner && autoStartWaves)
        {
            StartCoroutine(StartWaveSpawning());
        }
    }

    void Update()
    {
        if (!isDead)
        {
            HandleMovement();
            UpdateAnimations();
        }
    }

    #region Movement System
    void HandleMovement()
    {
        if (!isMoving || pathPoints == null || pathPoints.Length == 0)
            return;

        Transform targetPoint = pathPoints[currentPathIndex];
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        
        // Move towards target
        if (rb != null)
        {
            rb.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        // Rotate towards movement direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Check if reached target point
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.5f)
        {
            HandlePathProgression();
        }
    }

    void HandlePathProgression()
    {
        if (pathPoints.Length <= 1)
            return;

        if (loopPath)
        {
            currentPathIndex = (currentPathIndex + 1) % pathPoints.Length;
        }
        else if (reverseAtEnd)
        {
            if (movingForward)
            {
                currentPathIndex++;
                if (currentPathIndex >= pathPoints.Length - 1)
                {
                    movingForward = false;
                }
            }
            else
            {
                currentPathIndex--;
                if (currentPathIndex <= 0)
                {
                    movingForward = true;
                }
            }
        }
        else
        {
            currentPathIndex++;
            if (currentPathIndex >= pathPoints.Length)
            {
                isMoving = false;
            }
        }
    }

    public void SetPath(Transform[] newPath)
    {
        pathPoints = newPath;
        currentPathIndex = 0;
        movingForward = true;
        isMoving = true;
    }

    public void StopMovement()
    {
        isMoving = false;
    }

    public void ResumeMovement()
    {
        isMoving = true;
    }
    #endregion

    #region Health System
    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Play damage sound
        PlayDamageSound();

        // Trigger damage event
        OnEnemyDamaged?.Invoke(this, damage);

        // Update animations
        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead)
            return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isMoving = false;

        // Play death sound
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Trigger death event
        OnEnemyDeath?.Invoke(this);

        // Update animations
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Spawn death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
        }

        // Drop loot
        if (dropLoot && lootPrefabs != null && lootPrefabs.Length > 0)
        {
            DropLoot();
        }

        // Disable components
        if (rb != null)
            rb.isKinematic = true;

        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // Start death sequence
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
    }

    void DropLoot()
    {
        if (lootPrefabs == null || lootPrefabs.Length == 0)
            return;

        GameObject lootToDrop = lootPrefabs[Random.Range(0, lootPrefabs.Length)];
        if (lootToDrop != null)
        {
            Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
            Instantiate(lootToDrop, dropPosition, Random.rotation);
        }
    }

    void PlayDamageSound()
    {
        if (damageSounds != null && damageSounds.Length > 0 && audioSource != null)
        {
            AudioClip soundToPlay = damageSounds[Random.Range(0, damageSounds.Length)];
            audioSource.PlayOneShot(soundToPlay);
        }
    }
    #endregion

    #region Wave Spawning System
    IEnumerator StartWaveSpawning()
    {
        while (currentWaveIndex < waves.Length)
        {
            yield return StartCoroutine(SpawnWave(waves[currentWaveIndex]));
            
            currentWaveIndex++;
            
            if (currentWaveIndex < waves.Length)
            {
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }
    }

    IEnumerator SpawnWave(EnemyWave wave)
    {
        spawningWave = true;
        
        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemy(wave.enemyPrefab);
            yield return new WaitForSeconds(wave.spawnDelay);
        }
        
        spawningWave = false;
        yield return new WaitForSeconds(wave.waveDelay);
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
            return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        
        spawnedEnemies.Add(spawnedEnemy);

        // Set up death callback to remove from list
        EnemyController enemyController = spawnedEnemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.OnEnemyDeath += RemoveSpawnedEnemy;
        }
    }

    void RemoveSpawnedEnemy(EnemyController enemy)
    {
        if (spawnedEnemies.Contains(enemy.gameObject))
        {
            spawnedEnemies.Remove(enemy.gameObject);
        }
    }

    public void StartNextWave()
    {
        if (!spawningWave && currentWaveIndex < waves.Length)
        {
            StartCoroutine(SpawnWave(waves[currentWaveIndex]));
            currentWaveIndex++;
        }
    }

    public void ResetWaves()
    {
        currentWaveIndex = 0;
        StopAllCoroutines();
        
        // Clean up spawned enemies
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        spawnedEnemies.Clear();
    }
    #endregion

    #region Animation Updates
    void UpdateAnimations()
    {
        if (animator == null)
            return;

        animator.SetBool("IsMoving", isMoving);
        animator.SetFloat("Speed", isMoving ? moveSpeed : 0f);
        animator.SetFloat("Health", currentHealth / maxHealth);
        animator.SetBool("IsDead", isDead);
    }
    #endregion

    #region Public Utility Methods
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool IsAlive()
    {
        return !isDead;
    }

    public int GetCurrentWaveIndex()
    {
        return currentWaveIndex;
    }

    public int GetRemainingEnemies()
    {
        return spawnedEnemies.Count;
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
    #endregion

    #region Gizmos
    void OnDrawGizmosSelected()
    {
        if (pathPoints == null || pathPoints.Length == 0)
            return;

        // Draw path
        Gizmos.color = Color.red;
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            if (pathPoints[i] != null && pathPoints[i + 1] != null)
            {
                Gizmos.DrawLine(pathPoints[i].position, pathPoints[i + 1].position);
            }
        }

        // Draw loop connection
        if (loopPath && pathPoints.Length > 2)
        {
            if (pathPoints[pathPoints.Length - 1] != null && pathPoints[0] != null)
            {
                Gizmos.DrawLine(pathPoints[pathPoints.Length - 1].position, pathPoints[0].position);
            }
        }

        // Draw spawn points
        if (isWaveSpawner && spawnPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                }
            }
        }
    }
    #endregion
}