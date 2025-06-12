using UnityEngine;

public class CannonTower : Tower
{
    [Header("Cannon Settings")]
    public float explosionRadius = 2f;
    public float explosionDamage = 50f;
    public GameObject explosionEffect;
    public LayerMask enemyLayer = 1;
    
    protected override void Shoot()
    {
        if (target == null) return;
        
        Vector3 targetPosition = target.position;
        
        if (bulletPrefab != null)
        {
            GameObject cannonball = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            CannonBall cannonBallScript = cannonball.GetComponent<CannonBall>();
            if (cannonBallScript == null)
            {
                cannonBallScript = cannonball.AddComponent<CannonBall>();
            }
            
            cannonBallScript.SetTarget(targetPosition);
            cannonBallScript.explosionRadius = explosionRadius;
            cannonBallScript.explosionDamage = explosionDamage;
            cannonBallScript.explosionEffect = explosionEffect;
            cannonBallScript.enemyLayer = enemyLayer;
        }
        else
        {
            ExplodeAtPosition(targetPosition);
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTowerShoot();
        }
    }
    
    void ExplodeAtPosition(Vector3 position)
    {
        Collider[] enemies = Physics.OverlapSphere(position, explosionRadius, enemyLayer);
        
        foreach (Collider enemyCol in enemies)
        {
            Enemy enemy = enemyCol.GetComponent<Enemy>();
            if (enemy != null)
            {
                float distance = Vector3.Distance(position, enemy.transform.position);
                float damageMultiplier = 1f - (distance / explosionRadius);
                enemy.TakeDamage(explosionDamage * damageMultiplier);
            }
        }
        
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, position, Quaternion.identity);
        }
        
        if (ParticleManager.Instance != null)
        {
            ParticleManager.Instance.PlayExplosion(position);
        }
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}