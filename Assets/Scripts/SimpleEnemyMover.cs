using UnityEngine;

public class SimpleEnemyMover : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 2f;
    public float health = 100f;
    
    private int currentWaypointIndex = 0;
    private bool isDead = false;
    
    void Start()
    {
        gameObject.tag = "Enemy";
    }
    
    void Update()
    {
        if (isDead || waypoints == null || waypoints.Length == 0) return;
        
        MoveTowardsWaypoint();
    }
    
    void MoveTowardsWaypoint()
    {
        if (currentWaypointIndex >= waypoints.Length)
        {
            ReachedEnd();
            return;
        }
        
        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = (target.position - transform.position).normalized;
        
        transform.position += direction * speed * Time.deltaTime;
        transform.LookAt(target.position);
        
        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            currentWaypointIndex++;
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        health -= damage;
        
        // Visual feedback
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color color = Color.Lerp(Color.red, Color.black, 1f - (health / 100f));
            renderer.material.color = color;
        }
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        isDead = true;
        
        // Add score if GameManager exists
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(25);
            GameManager.Instance.AddScore(10);
        }
        
        Destroy(gameObject);
    }
    
    void ReachedEnd()
    {
        // Damage player if GameManager exists
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage(10);
        }
        
        Destroy(gameObject);
    }
}