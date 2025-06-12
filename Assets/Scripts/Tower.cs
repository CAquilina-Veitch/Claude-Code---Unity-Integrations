using UnityEngine;
using System.Collections;

public class Tower : MonoBehaviour
{
    [Header("Tower Stats")]
    public float damage = 25f;
    public float range = 3f;
    public float fireRate = 1f;
    public int cost = 100;
    public int upgradeCost = 150;
    public int currentLevel = 1;
    public int maxLevel = 3;
    
    [Header("Targeting")]
    public Transform target;
    public LayerMask enemyLayer = 1;
    
    [Header("References")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public GameObject muzzleFlash;
    
    private float nextFireTime = 0f;
    private bool canFire = true;
    
    protected virtual void Start()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        InvokeRepeating("FindTarget", 0f, 0.5f);
    }
    
    void Update()
    {
        if (target == null) return;
        
        if (Vector3.Distance(transform.position, target.position) > range)
        {
            target = null;
            return;
        }
        
        if (Time.time >= nextFireTime && canFire)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
        
        Vector3 direction = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
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
    
    protected virtual void Shoot()
    {
        if (target == null) return;
        
        if (bulletPrefab != null)
        {
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.SetTarget(target);
                bullet.damage = damage;
            }
        }
        else
        {
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        
        if (muzzleFlash != null)
        {
            StartCoroutine(ShowMuzzleFlash());
        }
    }
    
    IEnumerator ShowMuzzleFlash()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        muzzleFlash.SetActive(false);
    }
    
    public bool CanUpgrade()
    {
        return currentLevel < maxLevel && GameManager.Instance.currentMoney >= upgradeCost;
    }
    
    public void Upgrade()
    {
        if (!CanUpgrade()) return;
        
        if (GameManager.Instance.SpendMoney(upgradeCost))
        {
            currentLevel++;
            damage *= 1.5f;
            range *= 1.1f;
            fireRate *= 1.2f;
            upgradeCost = Mathf.RoundToInt(upgradeCost * 1.5f);
            
            transform.localScale *= 1.1f;
        }
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}