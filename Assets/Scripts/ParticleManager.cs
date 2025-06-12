using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [Header("Particle Effects")]
    public GameObject explosionEffect;
    public GameObject muzzleFlashEffect;
    public GameObject enemyDeathEffect;
    public GameObject bulletTrailEffect;
    
    public static ParticleManager Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PlayExplosion(Vector3 position)
    {
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
    
    public void PlayMuzzleFlash(Vector3 position, Vector3 direction)
    {
        if (muzzleFlashEffect != null)
        {
            GameObject effect = Instantiate(muzzleFlashEffect, position, Quaternion.LookRotation(direction));
            Destroy(effect, 0.5f);
        }
    }
    
    public void PlayEnemyDeath(Vector3 position)
    {
        if (enemyDeathEffect != null)
        {
            GameObject effect = Instantiate(enemyDeathEffect, position, Quaternion.identity);
            Destroy(effect, 3f);
        }
    }
    
    public GameObject CreateBulletTrail(Vector3 startPosition, Vector3 endPosition)
    {
        if (bulletTrailEffect != null)
        {
            GameObject trail = Instantiate(bulletTrailEffect, startPosition, Quaternion.LookRotation(endPosition - startPosition));
            return trail;
        }
        return null;
    }
}