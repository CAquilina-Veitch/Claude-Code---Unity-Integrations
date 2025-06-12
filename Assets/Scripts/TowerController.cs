using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TowerStats
{
    public float damage = 25f;
    public float fireRate = 1f;
    public float range = 10f;
    public float projectileSpeed = 15f;
    public int cost = 100;
    public float sellValue = 50f;
}

[System.Serializable]
public class TowerUpgrade
{
    public string upgradeName = "Basic Upgrade";
    public TowerStats upgradeStats;
    public int upgradeCost = 150;
    public GameObject upgradeVisualPrefab;
    public string upgradeDescription = "Improves tower performance";
}

public enum TowerType
{
    Basic,
    Rapid,
    Heavy,
    Splash,
    Laser,
    Ice,
    Poison
}

public enum TargetingMode
{
    First,
    Last,
    Closest,
    Strongest,
    Weakest
}

public class TowerController : MonoBehaviour
{
    [Header("Tower Configuration")]
    public TowerType towerType = TowerType.Basic;
    public TargetingMode targetingMode = TargetingMode.First;
    public TowerStats baseStats;
    public bool isPlaced = false;
    public bool canUpgrade = true;
    
    [Header("Combat System")]
    public Transform firePoint;
    public GameObject projectilePrefab;
    public LayerMask enemyLayerMask = 1;
    public string enemyTag = "Enemy";
    public bool useLaser = false;
    public LineRenderer laserLine;
    
    [Header("Visual Effects")]
    public GameObject muzzleFlash;
    public GameObject hitEffect;
    public ParticleSystem chargeEffect;
    public Transform rotatingPart;
    public float rotationSpeed = 90f;
    
    [Header("Upgrade System")]
    public TowerUpgrade[] availableUpgrades;
    public int currentUpgradeLevel = 0;
    public int maxUpgradeLevel = 3;
    public GameObject[] upgradedVisuals;
    
    [Header("Placement System")]
    public bool showRangeIndicator = true;
    public GameObject rangeIndicatorPrefab;
    public Material validPlacementMaterial;
    public Material invalidPlacementMaterial;
    public LayerMask obstacleLayerMask = 1;
    
    [Header("Audio")]
    public AudioClip[] fireSounds;
    public AudioClip upgradeSounds;
    public AudioClip placementSound;
    private AudioSource audioSource;
    
    [Header("Special Abilities")]
    public bool hasSpecialAbility = false;
    public float specialCooldown = 10f;
    public GameObject specialEffectPrefab;
    
    // Private variables
    private TowerStats currentStats;
    private Transform currentTarget;
    private List<Transform> enemiesInRange = new List<Transform>();
    private float nextFireTime = 0f;
    private float specialAbilityTimer = 0f;
    private GameObject rangeIndicator;
    private bool isPreview = false;
    private Renderer[] renderers;
    private Collider[] colliders;
    
    // Coroutines
    private Coroutine targetingCoroutine;
    private Coroutine firingCoroutine;
    
    // Events
    public System.Action<TowerController> OnTowerPlaced;
    public System.Action<TowerController> OnTowerUpgraded;
    public System.Action<TowerController, Transform> OnEnemyKilled;
    public System.Action<TowerController> OnTowerSold;

    void Start()
    {
        InitializeTower();
    }

    void InitializeTower()
    {
        currentStats = baseStats;
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponents<Collider>();
        
        SetupRangeIndicator();
        
        if (isPlaced)
        {
            StartTowerOperations();
        }
        else
        {
            SetPreviewMode(true);
        }
    }

    void Update()
    {
        if (!isPlaced)
            return;
            
        HandleSpecialAbility();
        
        if (rotatingPart != null && currentTarget != null)
        {
            RotateTowardsTarget();
        }
    }

    #region Tower Placement System
    public void SetPreviewMode(bool preview)
    {
        isPreview = preview;
        
        foreach (Collider col in colliders)
        {
            col.enabled = !preview;
        }
        
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(preview || showRangeIndicator);
        }
        
        UpdatePreviewMaterial(CanPlaceHere());
    }
    
    public bool CanPlaceHere()
    {
        if (!isPreview)
            return true;
            
        Collider towerCollider = GetComponent<Collider>();
        if (towerCollider == null)
            return true;
            
        Collider[] overlapping = Physics.OverlapBox(
            towerCollider.bounds.center,
            towerCollider.bounds.extents,
            transform.rotation,
            obstacleLayerMask
        );
        
        return overlapping.Length == 0;
    }
    
    void UpdatePreviewMaterial(bool canPlace)
    {
        Material materialToUse = canPlace ? validPlacementMaterial : invalidPlacementMaterial;
        
        if (materialToUse != null)
        {
            foreach (Renderer renderer in renderers)
            {
                renderer.material = materialToUse;
            }
        }
    }
    
    public void PlaceTower()
    {
        if (!CanPlaceHere())
            return;
            
        isPlaced = true;
        isPreview = false;
        
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
        
        if (placementSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(placementSound);
        }
        
        SetupRangeIndicator();
        StartTowerOperations();
        
        OnTowerPlaced?.Invoke(this);
    }
    
    void SetupRangeIndicator()
    {
        if (rangeIndicatorPrefab != null && rangeIndicator == null)
        {
            rangeIndicator = Instantiate(rangeIndicatorPrefab, transform);
            rangeIndicator.transform.localPosition = Vector3.zero;
            rangeIndicator.transform.localScale = Vector3.one * currentStats.range * 2f;
            rangeIndicator.SetActive(showRangeIndicator || isPreview);
        }
    }
    #endregion

    #region Tower Operations
    void StartTowerOperations()
    {
        if (targetingCoroutine == null)
            targetingCoroutine = StartCoroutine(TargetingRoutine());
            
        if (firingCoroutine == null)
            firingCoroutine = StartCoroutine(FiringRoutine());
    }
    
    void StopTowerOperations()
    {
        if (targetingCoroutine != null)
        {
            StopCoroutine(targetingCoroutine);
            targetingCoroutine = null;
        }
        
        if (firingCoroutine != null)
        {
            StopCoroutine(firingCoroutine);
            firingCoroutine = null;
        }
    }
    
    IEnumerator TargetingRoutine()
    {
        while (isPlaced)
        {
            UpdateEnemiesInRange();
            SelectTarget();
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    IEnumerator FiringRoutine()
    {
        while (isPlaced)
        {
            if (CanFire())
            {
                Fire();
            }
            yield return new WaitForSeconds(0.05f);
        }
    }
    #endregion

    #region Target Detection System
    void UpdateEnemiesInRange()
    {
        enemiesInRange.Clear();
        
        Collider[] enemies = Physics.OverlapSphere(transform.position, currentStats.range, enemyLayerMask);
        
        foreach (Collider enemy in enemies)
        {
            if (enemy.CompareTag(enemyTag))
            {
                EnemyController enemyController = enemy.GetComponent<EnemyController>();
                if (enemyController != null && enemyController.IsAlive())
                {
                    enemiesInRange.Add(enemy.transform);
                }
            }
        }
    }
    
    void SelectTarget()
    {
        if (enemiesInRange.Count == 0)
        {
            currentTarget = null;
            return;
        }
        
        switch (targetingMode)
        {
            case TargetingMode.First:
                currentTarget = GetFirstEnemy();
                break;
            case TargetingMode.Last:
                currentTarget = GetLastEnemy();
                break;
            case TargetingMode.Closest:
                currentTarget = GetClosestEnemy();
                break;
            case TargetingMode.Strongest:
                currentTarget = GetStrongestEnemy();
                break;
            case TargetingMode.Weakest:
                currentTarget = GetWeakestEnemy();
                break;
        }
    }
    
    Transform GetFirstEnemy()
    {
        Transform firstEnemy = null;
        float longestDistance = 0f;
        
        foreach (Transform enemy in enemiesInRange)
        {
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                float pathProgress = GetEnemyPathProgress(enemyController);
                if (pathProgress > longestDistance)
                {
                    longestDistance = pathProgress;
                    firstEnemy = enemy;
                }
            }
        }
        
        return firstEnemy;
    }
    
    Transform GetLastEnemy()
    {
        Transform lastEnemy = null;
        float shortestDistance = float.MaxValue;
        
        foreach (Transform enemy in enemiesInRange)
        {
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                float pathProgress = GetEnemyPathProgress(enemyController);
                if (pathProgress < shortestDistance)
                {
                    shortestDistance = pathProgress;
                    lastEnemy = enemy;
                }
            }
        }
        
        return lastEnemy;
    }
    
    Transform GetClosestEnemy()
    {
        Transform closestEnemy = null;
        float closestDistance = float.MaxValue;
        
        foreach (Transform enemy in enemiesInRange)
        {
            float distance = Vector3.Distance(transform.position, enemy.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }
        
        return closestEnemy;
    }
    
    Transform GetStrongestEnemy()
    {
        Transform strongestEnemy = null;
        float highestHealth = 0f;
        
        foreach (Transform enemy in enemiesInRange)
        {
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null && enemyController.currentHealth > highestHealth)
            {
                highestHealth = enemyController.currentHealth;
                strongestEnemy = enemy;
            }
        }
        
        return strongestEnemy;
    }
    
    Transform GetWeakestEnemy()
    {
        Transform weakestEnemy = null;
        float lowestHealth = float.MaxValue;
        
        foreach (Transform enemy in enemiesInRange)
        {
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null && enemyController.currentHealth < lowestHealth)
            {
                lowestHealth = enemyController.currentHealth;
                weakestEnemy = enemy;
            }
        }
        
        return weakestEnemy;
    }
    
    float GetEnemyPathProgress(EnemyController enemy)
    {
        // This would need to be implemented based on your path system
        // For now, return distance from start point as approximation
        return Vector3.Distance(enemy.transform.position, Vector3.zero);
    }
    #endregion

    #region Shooting Mechanics
    bool CanFire()
    {
        return currentTarget != null && 
               Time.time >= nextFireTime && 
               Vector3.Distance(transform.position, currentTarget.position) <= currentStats.range;
    }
    
    void Fire()
    {
        nextFireTime = Time.time + (1f / currentStats.fireRate);
        
        ApplyTowerTypeEffects();
        
        if (useLaser)
        {
            FireLaser();
        }
        else
        {
            FireProjectile();
        }
        
        PlayFireSound();
        ShowMuzzleFlash();
    }
    
    void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null || currentTarget == null)
            return;
            
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        TowerProjectile projectileScript = projectile.GetComponent<TowerProjectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(currentTarget, currentStats.damage, currentStats.projectileSpeed, towerType);
        }
        else
        {
            // Fallback: simple projectile movement
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (currentTarget.position - firePoint.position).normalized;
                rb.linearVelocity = direction * currentStats.projectileSpeed;
            }
        }
    }
    
    void FireLaser()
    {
        if (laserLine == null || currentTarget == null)
            return;
            
        laserLine.enabled = true;
        laserLine.SetPosition(0, firePoint.position);
        laserLine.SetPosition(1, currentTarget.position);
        
        // Deal damage directly for laser
        EnemyController enemyController = currentTarget.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.TakeDamage(currentStats.damage * Time.deltaTime);
        }
        
        StartCoroutine(DisableLaser());
    }
    
    IEnumerator DisableLaser()
    {
        yield return new WaitForSeconds(0.1f);
        if (laserLine != null)
            laserLine.enabled = false;
    }
    
    void ApplyTowerTypeEffects()
    {
        switch (towerType)
        {
            case TowerType.Ice:
                ApplySlowEffect();
                break;
            case TowerType.Poison:
                ApplyPoisonEffect();
                break;
            case TowerType.Splash:
                ApplySplashDamage();
                break;
        }
    }
    
    void ApplySlowEffect()
    {
        if (currentTarget == null)
            return;
            
        // Apply slow effect to target
        // This would need a slow component or effect system
    }
    
    void ApplyPoisonEffect()
    {
        if (currentTarget == null)
            return;
            
        // Apply poison damage over time
        // This would need a poison component or effect system
    }
    
    void ApplySplashDamage()
    {
        if (currentTarget == null)
            return;
            
        Collider[] nearbyEnemies = Physics.OverlapSphere(currentTarget.position, 3f, enemyLayerMask);
        
        foreach (Collider enemy in nearbyEnemies)
        {
            if (enemy.CompareTag(enemyTag))
            {
                EnemyController enemyController = enemy.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    float distance = Vector3.Distance(currentTarget.position, enemy.transform.position);
                    float damageMultiplier = Mathf.Clamp01(1f - (distance / 3f));
                    enemyController.TakeDamage(currentStats.damage * damageMultiplier);
                }
            }
        }
    }
    
    void RotateTowardsTarget()
    {
        if (currentTarget == null)
            return;
            
        Vector3 direction = (currentTarget.position - rotatingPart.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rotatingPart.rotation = Quaternion.RotateTowards(rotatingPart.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
    
    void PlayFireSound()
    {
        if (fireSounds != null && fireSounds.Length > 0 && audioSource != null)
        {
            AudioClip soundToPlay = fireSounds[Random.Range(0, fireSounds.Length)];
            audioSource.PlayOneShot(soundToPlay);
        }
    }
    
    void ShowMuzzleFlash()
    {
        if (muzzleFlash != null)
        {
            GameObject flash = Instantiate(muzzleFlash, firePoint.position, firePoint.rotation);
            Destroy(flash, 0.5f);
        }
    }
    #endregion

    #region Upgrade System
    public bool CanUpgrade()
    {
        return canUpgrade && 
               currentUpgradeLevel < maxUpgradeLevel && 
               currentUpgradeLevel < availableUpgrades.Length;
    }
    
    public int GetUpgradeCost()
    {
        if (!CanUpgrade())
            return -1;
            
        return availableUpgrades[currentUpgradeLevel].upgradeCost;
    }
    
    public TowerUpgrade GetNextUpgrade()
    {
        if (!CanUpgrade())
            return null;
            
        return availableUpgrades[currentUpgradeLevel];
    }
    
    public bool UpgradeTower()
    {
        if (!CanUpgrade())
            return false;
            
        TowerUpgrade upgrade = availableUpgrades[currentUpgradeLevel];
        
        // Apply upgrade stats
        currentStats.damage += upgrade.upgradeStats.damage;
        currentStats.fireRate += upgrade.upgradeStats.fireRate;
        currentStats.range += upgrade.upgradeStats.range;
        currentStats.projectileSpeed += upgrade.upgradeStats.projectileSpeed;
        
        // Update visual
        if (currentUpgradeLevel < upgradedVisuals.Length && upgradedVisuals[currentUpgradeLevel] != null)
        {
            upgradedVisuals[currentUpgradeLevel].SetActive(true);
        }
        
        if (upgrade.upgradeVisualPrefab != null)
        {
            Instantiate(upgrade.upgradeVisualPrefab, transform);
        }
        
        // Update range indicator
        if (rangeIndicator != null)
        {
            rangeIndicator.transform.localScale = Vector3.one * currentStats.range * 2f;
        }
        
        currentUpgradeLevel++;
        
        if (upgradeSounds != null && audioSource != null)
        {
            audioSource.PlayOneShot(upgradeSounds);
        }
        
        OnTowerUpgraded?.Invoke(this);
        
        return true;
    }
    #endregion

    #region Special Abilities
    void HandleSpecialAbility()
    {
        if (!hasSpecialAbility)
            return;
            
        specialAbilityTimer -= Time.deltaTime;
        
        if (specialAbilityTimer <= 0f && enemiesInRange.Count > 0)
        {
            ActivateSpecialAbility();
            specialAbilityTimer = specialCooldown;
        }
    }
    
    void ActivateSpecialAbility()
    {
        switch (towerType)
        {
            case TowerType.Heavy:
                HeavySpecialAbility();
                break;
            case TowerType.Laser:
                LaserSpecialAbility();
                break;
            default:
                BasicSpecialAbility();
                break;
        }
        
        if (specialEffectPrefab != null)
        {
            Instantiate(specialEffectPrefab, transform.position, transform.rotation);
        }
    }
    
    void BasicSpecialAbility()
    {
        // Rapid fire burst
        StartCoroutine(RapidFireBurst());
    }
    
    void HeavySpecialAbility()
    {
        // Area damage explosion
        Collider[] enemies = Physics.OverlapSphere(transform.position, currentStats.range * 1.5f, enemyLayerMask);
        
        foreach (Collider enemy in enemies)
        {
            if (enemy.CompareTag(enemyTag))
            {
                EnemyController enemyController = enemy.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.TakeDamage(currentStats.damage * 2f);
                }
            }
        }
    }
    
    void LaserSpecialAbility()
    {
        // Multi-target laser
        StartCoroutine(MultiTargetLaser());
    }
    
    IEnumerator RapidFireBurst()
    {
        float originalFireRate = currentStats.fireRate;
        currentStats.fireRate *= 5f;
        
        yield return new WaitForSeconds(2f);
        
        currentStats.fireRate = originalFireRate;
    }
    
    IEnumerator MultiTargetLaser()
    {
        if (laserLine == null)
            yield break;
            
        foreach (Transform enemy in enemiesInRange)
        {
            if (enemy != null)
            {
                laserLine.enabled = true;
                laserLine.SetPosition(0, firePoint.position);
                laserLine.SetPosition(1, enemy.position);
                
                EnemyController enemyController = enemy.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.TakeDamage(currentStats.damage);
                }
                
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        laserLine.enabled = false;
    }
    #endregion

    #region Public Utility Methods
    public float GetSellValue()
    {
        float sellValue = currentStats.sellValue;
        
        // Add partial upgrade costs to sell value
        for (int i = 0; i < currentUpgradeLevel && i < availableUpgrades.Length; i++)
        {
            sellValue += availableUpgrades[i].upgradeCost * 0.7f;
        }
        
        return sellValue;
    }
    
    public void SellTower()
    {
        StopTowerOperations();
        OnTowerSold?.Invoke(this);
        Destroy(gameObject);
    }
    
    public TowerStats GetCurrentStats()
    {
        return currentStats;
    }
    
    public void SetTargetingMode(TargetingMode newMode)
    {
        targetingMode = newMode;
    }
    
    public void ToggleRangeIndicator()
    {
        showRangeIndicator = !showRangeIndicator;
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(showRangeIndicator);
        }
    }
    
    public int GetEnemiesInRangeCount()
    {
        return enemiesInRange.Count;
    }
    
    public float GetSpecialAbilityProgress()
    {
        if (!hasSpecialAbility)
            return 0f;
            
        return 1f - (specialAbilityTimer / specialCooldown);
    }
    #endregion

    #region Gizmos
    void OnDrawGizmosSelected()
    {
        // Draw range circle
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, currentStats != null ? currentStats.range : baseStats.range);
        
        // Draw line to current target
        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
        
        // Draw fire point
        if (firePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
        }
    }
    #endregion
}