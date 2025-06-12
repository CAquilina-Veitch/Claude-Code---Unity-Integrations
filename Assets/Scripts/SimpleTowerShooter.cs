using UnityEngine;
using System.Collections;

public class SimpleTowerShooter : MonoBehaviour
{
    public float range = 4f;
    public float damage = 25f;
    public float fireRate = 1f;
    
    private Transform target;
    private float nextFireTime = 0f;
    
    void Start()
    {
        InvokeRepeating("FindTarget", 0f, 0.2f);
    }
    
    void Update()
    {
        if (target == null) return;
        
        // Check if target is still in range
        if (Vector3.Distance(transform.position, target.position) > range)
        {
            target = null;
            return;
        }
        
        // Rotate towards target
        Vector3 direction = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3f);
        
        // Shoot if ready
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + (1f / fireRate);
        }
    }
    
    void FindTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        if (enemies.Length == 0)
        {
            target = null;
            return;
        }
        
        Transform closestEnemy = null;
        float shortestDistance = Mathf.Infinity;
        
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= range && distance < shortestDistance)
            {
                shortestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }
        
        target = closestEnemy;
    }
    
    void Shoot()
    {
        if (target == null) return;
        
        // Create bullet effect
        CreateBulletEffect();
        
        // Deal damage to target
        SimpleEnemyMover enemy = target.GetComponent<SimpleEnemyMover>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
        
        // Visual feedback
        StartCoroutine(MuzzleFlash());
    }
    
    void CreateBulletEffect()
    {
        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "Bullet";
        bullet.transform.position = transform.position + Vector3.up;
        bullet.transform.localScale = Vector3.one * 0.2f;
        
        Renderer renderer = bullet.GetComponent<Renderer>();
        renderer.material.color = Color.yellow;
        
        Destroy(bullet.GetComponent<SphereCollider>());
        
        // Move bullet towards target
        SimpleBullet bulletScript = bullet.AddComponent<SimpleBullet>();
        bulletScript.target = target.position;
        bulletScript.speed = 10f;
    }
    
    IEnumerator MuzzleFlash()
    {
        Renderer renderer = GetComponent<Renderer>();
        Color originalColor = renderer.material.color;
        
        renderer.material.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        renderer.material.color = originalColor;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}