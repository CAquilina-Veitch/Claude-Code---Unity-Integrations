using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 2f;
    public int damageToPlayer = 10;
    public int moneyReward = 25;
    public int scoreReward = 10;
    
    [Header("References")]
    public Transform[] waypoints;
    
    protected float currentHealth;
    private int currentWaypointIndex = 0;
    protected bool isDead = false;
    
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        gameObject.tag = "Enemy";
        
        if (waypoints == null || waypoints.Length == 0)
        {
            waypoints = FindObjectOfType<PathManager>()?.GetWaypoints();
        }
    }
    
    protected virtual void Update()
    {
        if (!isDead && waypoints != null && waypoints.Length > 0)
        {
            MoveTowardsWaypoint();
        }
    }
    
    void MoveTowardsWaypoint()
    {
        if (currentWaypointIndex >= waypoints.Length)
        {
            ReachEnd();
            return;
        }
        
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.LookAt(targetWaypoint.position);
        
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex++;
        }
    }
    
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    protected virtual void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(moneyReward);
            GameManager.Instance.AddScore(scoreReward);
        }
        
        Destroy(gameObject);
    }
    
    void ReachEnd()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage(damageToPlayer);
        }
        
        Destroy(gameObject);
    }
    
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}