using UnityEngine;

public class BasicEnemyMovement : MonoBehaviour
{
    private Vector3[] waypoints = {
        new Vector3(-8, 0, 0),
        new Vector3(-2, 0, 0),
        new Vector3(-2, 0, 4),
        new Vector3(2, 0, 4),
        new Vector3(8, 0, 4)
    };
    
    private int currentWaypoint = 0;
    private float speed = 2f;
    private float health = 100f;
    
    void Start()
    {
        gameObject.tag = "Enemy";
    }
    
    void Update()
    {
        if (currentWaypoint >= waypoints.Length)
        {
            Destroy(gameObject);
            return;
        }
        
        Vector3 target = waypoints[currentWaypoint];
        Vector3 direction = (target - transform.position).normalized;
        
        transform.position += direction * speed * Time.deltaTime;
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
        GetComponent<Renderer>().material.color = Color.Lerp(Color.red, Color.black, 1f - (health / 100f));
        
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}