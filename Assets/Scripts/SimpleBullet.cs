using UnityEngine;

public class SimpleBullet : MonoBehaviour
{
    public Vector3 target;
    public float speed = 10f;
    
    void Update()
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        if (Vector3.Distance(transform.position, target) < 0.5f)
        {
            Destroy(gameObject);
        }
        
        // Auto-destroy after 3 seconds
        Destroy(gameObject, 3f);
    }
}