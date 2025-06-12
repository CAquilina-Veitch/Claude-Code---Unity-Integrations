using UnityEngine;

public class LaserTower : Tower
{
    [Header("Laser Settings")]
    public LineRenderer laserLine;
    public Transform laserOrigin;
    public GameObject laserEffect;
    public float laserDuration = 0.1f;
    
    private bool isFiring = false;
    
    protected override void Start()
    {
        base.Start();
        
        if (laserLine == null)
        {
            laserLine = gameObject.AddComponent<LineRenderer>();
            laserLine.material = new Material(Shader.Find("Sprites/Default"));
            laserLine.material.color = Color.red;
            laserLine.startWidth = 0.1f;
            laserLine.endWidth = 0.1f;
            laserLine.enabled = false;
        }
        
        if (laserOrigin == null)
        {
            laserOrigin = transform;
        }
    }
    
    protected override void Shoot()
    {
        if (target == null || isFiring) return;
        
        StartCoroutine(FireLaser());
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTowerShoot();
        }
    }
    
    System.Collections.IEnumerator FireLaser()
    {
        isFiring = true;
        
        if (laserLine != null)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(0, laserOrigin.position);
            laserLine.SetPosition(1, target.position);
        }
        
        if (laserEffect != null)
        {
            GameObject effect = Instantiate(laserEffect, target.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
        
        yield return new WaitForSeconds(laserDuration);
        
        if (laserLine != null)
        {
            laserLine.enabled = false;
        }
        
        isFiring = false;
    }
}