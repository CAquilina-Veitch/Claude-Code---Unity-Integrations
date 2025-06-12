using UnityEngine;

public class BasicTowerShooting : MonoBehaviour
{
    private float range = 4f;
    private float damage = 30f;
    private float fireRate = 1f;
    private float nextFireTime = 0f;
    
    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            GameObject target = FindNearestEnemy();
            if (target != null)
            {
                Shoot(target);
                nextFireTime = Time.time + (1f / fireRate);
            }
        }
    }
    
    GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float closestDistance = range;
        
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearest = enemy;
            }
        }
        
        return nearest;
    }
    
    void Shoot(GameObject target)
    {
        // Visual effect
        Debug.DrawLine(transform.position, target.transform.position, Color.yellow, 0.1f);
        
        // Rotate towards target
        transform.LookAt(target.transform.position);
        
        // Deal damage
        BasicEnemyMovement enemy = target.GetComponent<BasicEnemyMovement>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
        
        // Flash effect
        StartCoroutine(FlashEffect());
    }
    
    System.Collections.IEnumerator FlashEffect()
    {
        Renderer renderer = GetComponent<Renderer>();
        Color original = renderer.material.color;
        renderer.material.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        renderer.material.color = original;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}