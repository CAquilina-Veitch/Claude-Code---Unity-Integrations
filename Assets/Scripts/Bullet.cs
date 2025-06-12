using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 10f;
    public float damage = 25f;
    public float lifeTime = 3f;
    
    [Header("Effects")]
    public GameObject impactEffect;
    public GameObject trailEffect;
    
    private Transform target;
    private Vector3 targetPosition;
    private bool hasTarget = false;
    
    void Start()
    {
        Destroy(gameObject, lifeTime);
        
        if (trailEffect != null)
        {
            trailEffect.SetActive(true);
        }
    }
    
    void Update()
    {
        if (hasTarget)
        {
            if (target != null)
            {
                targetPosition = target.position;
                Vector3 direction = (targetPosition - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;
                transform.LookAt(targetPosition);
                
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                    HitTarget();
                }
            }
            else
            {
                transform.position += transform.forward * speed * Time.deltaTime;
            }
        }
        else
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        hasTarget = true;
        if (target != null)
        {
            targetPosition = target.position;
        }
    }
    
    void HitTarget()
    {
        if (target != null)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, transform.rotation);
        }
        
        Destroy(gameObject);
    }
    
    void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            
            if (impactEffect != null)
            {
                Instantiate(impactEffect, transform.position, transform.rotation);
            }
            
            Destroy(gameObject);
        }
    }
}