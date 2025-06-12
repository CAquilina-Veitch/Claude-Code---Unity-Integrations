using UnityEngine;
using System.Collections;

public class TowerProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float damage = 25f;
    public float speed = 15f;
    public float lifetime = 5f;
    public bool seekTarget = true;
    public float turnSpeed = 180f;
    
    [Header("Effects")]
    public GameObject hitEffect;
    public GameObject trailEffect;
    public AudioClip hitSound;
    public float explosionRadius = 0f;
    
    [Header("Special Properties")]
    public TowerType projectileType = TowerType.Basic;
    public float slowDuration = 2f;
    public float slowAmount = 0.5f;
    public float poisonDuration = 3f;
    public float poisonDamagePerSecond = 5f;
    
    private Transform target;
    private Rigidbody rb;
    private AudioSource audioSource;
    private bool hasHit = false;
    private float lifetimeTimer;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        lifetimeTimer = lifetime;
        
        if (trailEffect != null)
        {
            trailEffect.SetActive(true);
        }
    }
    
    void Update()
    {
        lifetimeTimer -= Time.deltaTime;
        
        if (lifetimeTimer <= 0f)
        {
            DestroyProjectile();
            return;
        }
        
        if (seekTarget && target != null && !hasHit)
        {
            SeekTarget();
        }
    }
    
    public void Initialize(Transform targetTransform, float projectileDamage, float projectileSpeed, TowerType type)
    {
        target = targetTransform;
        damage = projectileDamage;
        speed = projectileSpeed;
        projectileType = type;
        
        if (rb != null && !seekTarget)
        {
            Vector3 direction = target != null ? 
                (target.position - transform.position).normalized : 
                transform.forward;
            rb.linearVelocity = direction * speed;
        }
    }
    
    void SeekTarget()
    {
        if (target == null || rb == null)
        {
            if (rb != null)
                rb.linearVelocity = transform.forward * speed;
            return;
        }
        
        Vector3 direction = (target.position - transform.position).normalized;
        
        // Rotate towards target
        Vector3 currentDirection = rb.linearVelocity.normalized;
        Vector3 newDirection = Vector3.RotateTowards(currentDirection, direction, turnSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f);
        
        rb.linearVelocity = newDirection * speed;
        transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasHit)
            return;
            
        if (other.CompareTag("Enemy"))
        {
            HitTarget(other.gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || 
                 other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            DestroyProjectile();
        }
    }
    
    void HitTarget(GameObject hitObject)
    {
        hasHit = true;
        
        EnemyController enemyController = hitObject.GetComponent<EnemyController>();
        if (enemyController != null && enemyController.IsAlive())
        {
            DealDamage(enemyController);
            ApplySpecialEffects(enemyController);
        }
        
        if (explosionRadius > 0f)
        {
            ApplyAreaDamage();
        }
        
        PlayHitEffects();
        DestroyProjectile();
    }
    
    void DealDamage(EnemyController enemy)
    {
        float finalDamage = damage;
        
        // Apply type-specific damage modifiers
        switch (projectileType)
        {
            case TowerType.Heavy:
                finalDamage *= 1.5f;
                break;
            case TowerType.Rapid:
                finalDamage *= 0.8f;
                break;
        }
        
        enemy.TakeDamage(finalDamage);
    }
    
    void ApplySpecialEffects(EnemyController enemy)
    {
        switch (projectileType)
        {
            case TowerType.Ice:
                ApplySlowEffect(enemy);
                break;
            case TowerType.Poison:
                ApplyPoisonEffect(enemy);
                break;
        }
    }
    
    void ApplySlowEffect(EnemyController enemy)
    {
        // Apply slow effect - this would need a proper status effect system
        // For now, we'll directly modify the enemy's speed
        float originalSpeed = enemy.GetComponent<EnemyController>().moveSpeed;
        enemy.SetMoveSpeed(originalSpeed * slowAmount);
        
        StartCoroutine(RemoveSlowEffect(enemy, originalSpeed));
    }
    
    IEnumerator RemoveSlowEffect(EnemyController enemy, float originalSpeed)
    {
        yield return new WaitForSeconds(slowDuration);
        
        if (enemy != null && enemy.IsAlive())
        {
            enemy.SetMoveSpeed(originalSpeed);
        }
    }
    
    void ApplyPoisonEffect(EnemyController enemy)
    {
        // Apply poison damage over time
        StartCoroutine(PoisonDamageOverTime(enemy));
    }
    
    IEnumerator PoisonDamageOverTime(EnemyController enemy)
    {
        float poisonTimer = poisonDuration;
        
        while (poisonTimer > 0f && enemy != null && enemy.IsAlive())
        {
            enemy.TakeDamage(poisonDamagePerSecond * Time.deltaTime);
            poisonTimer -= Time.deltaTime;
            yield return null;
        }
    }
    
    void ApplyAreaDamage()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                EnemyController enemyController = enemy.GetComponent<EnemyController>();
                if (enemyController != null && enemyController.IsAlive())
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    float damageMultiplier = Mathf.Clamp01(1f - (distance / explosionRadius));
                    
                    enemyController.TakeDamage(damage * damageMultiplier * 0.7f);
                    ApplySpecialEffects(enemyController);
                }
            }
        }
    }
    
    void PlayHitEffects()
    {
        if (hitEffect != null)
        {
            GameObject effect = Instantiate(hitEffect, transform.position, transform.rotation);
            Destroy(effect, 2f);
        }
        
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }
    
    void DestroyProjectile()
    {
        if (trailEffect != null)
        {
            trailEffect.transform.SetParent(null);
            Destroy(trailEffect, 2f);
        }
        
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        if (explosionRadius > 0f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
        
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}