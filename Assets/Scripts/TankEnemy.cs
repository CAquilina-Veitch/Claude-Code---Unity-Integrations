using UnityEngine;

public class TankEnemy : Enemy
{
    [Header("Tank Enemy Settings")]
    public float armor = 0.5f;
    public GameObject shieldEffect;
    
    private bool shieldActive = false;
    
    protected override void Start()
    {
        base.Start();
        moveSpeed = 1f;
        maxHealth = 200f;
        damageToPlayer = 20;
        moneyReward = 50;
        scoreReward = 25;
        
        currentHealth = maxHealth;
        
        if (shieldEffect != null)
        {
            shieldEffect.SetActive(true);
        }
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
        
        transform.localScale *= 1.5f;
    }
    
    public override void TakeDamage(float damage)
    {
        if (isDead) return;
        
        float actualDamage = damage;
        
        if (currentHealth > maxHealth * 0.5f)
        {
            actualDamage = damage * (1f - armor);
            if (!shieldActive)
            {
                ActivateShield();
            }
        }
        else
        {
            if (shieldActive)
            {
                DeactivateShield();
            }
        }
        
        currentHealth -= actualDamage;
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyHit();
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void ActivateShield()
    {
        shieldActive = true;
        if (shieldEffect != null)
        {
            shieldEffect.SetActive(true);
        }
    }
    
    void DeactivateShield()
    {
        shieldActive = false;
        if (shieldEffect != null)
        {
            shieldEffect.SetActive(false);
        }
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.gray;
        }
    }
    
    protected override void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayEnemyDeath(transform.position);
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDeath();
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(moneyReward);
            GameManager.Instance.AddScore(scoreReward);
        }
        
        Destroy(gameObject);
    }
}